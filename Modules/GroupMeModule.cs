using Nancy;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.Security;
using Nancy.ModelBinding;
using Automation.Models;
using System.Text.RegularExpressions;
using System.Configuration;

namespace Automation.Modules
{
    public class GroupMeModule : NancyModule
    {
        public GroupMeModule()
        {
            this.RequiresAuthentication();

            Post["/torrent/finished/"] = _ =>
            {
                this.RequiresClaims(new[] { "Torrent" });
                var model = this.Bind<GroupMeTorrentFinished>();
                BotPost(ConfigurationManager.AppSettings["GroupMeTorrentBot"], model.Name + " is finished");
                return Negotiate.WithStatusCode(HttpStatusCode.NoContent);
            };

            Get["/uptime/alert"] = _ =>
            {
                this.RequiresClaims(new[] { "GroupMe" });
                var model = this.Bind<GroupMeUptimeRobotAlert>();

                var text = model.monitorFriendlyName + " is " + (model.alertType == 1 ? "down" : "up");
                BotPost(ConfigurationManager.AppSettings["GroupMeAutomationBot"], text);

                return Negotiate.WithStatusCode(HttpStatusCode.NoContent);
            };

            Post["/groupme/message/"] = _ =>
            {
                this.RequiresClaims(new[] { "GroupMe" });
                var model = this.Bind<GroupMeChatMessage>();

                var text = model.text.Trim();
                var firstword = Regex.Match(text, @"\S+").Value;
                var commandRegex = Regex.Match(text, @"\S+ (.*)");
                var command = commandRegex.Groups[commandRegex.Groups.Count - 1].Value.ToLower();
                if (firstword.Equals("@TorrentBot", StringComparison.CurrentCultureIgnoreCase))
                {
                    BotPost(ConfigurationManager.AppSettings["GroupMeTorrentBot"], "No commands for this bot");
                }
                return Negotiate.WithStatusCode(HttpStatusCode.NoContent);
            };
        }
        
        private static GroupMeGroup GetGroupInfo(string groupId)
        {
            var client = new RestClient("https://api.groupme.com/v3");

            var request = new RestRequest("groups/{id}", Method.GET);
            request.AddUrlSegment("id", groupId);
            request.AddParameter("token", ConfigurationManager.AppSettings["GroupMeAccessToken"]);

            var group = client.Execute<GroupMeResponseWrapper<GroupMeGroup>>(request);
            return group.Data.response;
        }

        public static void BotPost(string botId, string message, List<IGroupMeAttachment> attachments = null)
        {
            BotPost(new GroupMeBotPost
            {
                text = message,
                bot_id = botId,
                attachments = attachments
            });
        }

        public static void BotPost(GroupMeBotPost botPost)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(botPost.text);
#else
            var client = new RestClient("https://api.groupme.com/v3");

            var request = new RestRequest("bots/post", Method.POST);
            request.AddJsonBody(botPost);

            var response = client.Execute(request);
            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                throw new Exception("Error posting to GroupMe");
            }
#endif
        }
    }
}