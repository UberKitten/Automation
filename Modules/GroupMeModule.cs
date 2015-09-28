using Nancy;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.Security;
using Microsoft.Azure;
using Nancy.ModelBinding;
using Automation.Models;
using System.Text.RegularExpressions;

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
                BotPost(CloudConfigurationManager.GetSetting("GroupMeTorrentBot"), model.Name + " is finished");
                return Negotiate.WithStatusCode(HttpStatusCode.NoContent);
            };

            Get["/uptime/alert"] = _ =>
            {
                this.RequiresClaims(new[] { "GroupMe" });
                var model = this.Bind<GroupMeUptimeRobotAlert>();

                var text = model.monitorFriendlyName + " is " + (model.alertType == 1 ? "down" : "up");
                BotPost(CloudConfigurationManager.GetSetting("GroupMeAutomationBot"), text);

                return Negotiate.WithStatusCode(HttpStatusCode.NoContent);
            };

            Post["/groupme/message/"] = _ =>
            {
                this.RequiresClaims(new[] { "GroupMe" });
                var model = this.Bind<GroupMeChatMessage>();

                var text = model.text.Trim();
                var firstword = Regex.Match(text, @"\S+").Value;
                var commandRegex = Regex.Match(text, @"\S+ (.*)");
                var command = commandRegex.Value;

                if (firstword.Equals("@ChoreBot", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Empty command
                    if (String.IsNullOrWhiteSpace(command))
                    {
                        var choreGroups = ChoreModule.GetChoresForDate(DateTime.Now);
                        foreach (var choreGroup in choreGroups.Where(t => t.Chores.Count > 0))
                        {
                            PostChoreGroup(choreGroup, CloudConfigurationManager.GetSetting("GroupMeChoreBot"));
                        }
                    }

                    BotPost(CloudConfigurationManager.GetSetting("GroupMeChoreBot"), command);
                }
                else if (firstword.Equals("@TorrentBot", StringComparison.CurrentCultureIgnoreCase))
                {
                    BotPost(CloudConfigurationManager.GetSetting("GroupMeTorrentBot"), command);
                }
                return Negotiate.WithStatusCode(HttpStatusCode.NoContent);
            };
        }
        
        private void PostChoreGroup(ChoreGroup choreGroup, string botId)
        {
            var client = new RestClient("https://api.groupme.com/v3");

            var request = new RestRequest("groups", Method.GET);
            request.AddParameter("bot_id", botId);

            var response = client.Execute(request);
            ;
        }

        public static void BotPost(string botId, string message)
        {
            var client = new RestClient("https://api.groupme.com/v3");

            var request = new RestRequest("bots/post", Method.POST);
            request.AddParameter("bot_id", botId);
            request.AddParameter("text", message);

            var response = client.Execute(request);
            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                throw new Exception("Error posting to GroupMe");
            }
        }
    }
}