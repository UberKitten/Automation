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

            Post["/uptime/alert"] = _ =>
            {
                this.RequiresClaims(new[] { "GroupMe" });
                var model = this.Bind<GroupMeUptimeRobotAlert>();

                var text = model.monitorFriendlyName + " is ";
                text += model.alertType == 1 ? "down" : "up";
                if (!String.IsNullOrWhiteSpace(model.alertDetails))
                {
                    text += " (" + model.alertDetails + ")";
                }

                BotPost(CloudConfigurationManager.GetSetting("GroupMeAutomationBot"), text);

                return Negotiate.WithStatusCode(HttpStatusCode.NoContent);
            };

            Post["/groupme/message/"] = _ =>
            {
                this.RequiresClaims(new[] { "GroupMe" });
                var model = this.Bind<GroupMeChatMessage>();

                var text = model.text.Trim();
                var firstword = text.Substring(0, text.IndexOf(' '));
                var command = text.Substring(text.IndexOf(' ') + 1);

                if (firstword.Equals("@ChoreBot", StringComparison.CurrentCultureIgnoreCase))
                {
                    BotPost(CloudConfigurationManager.GetSetting("GroupMeChoreBot"), command);
                }
                else if (firstword.Equals("@TorrentBot", StringComparison.CurrentCultureIgnoreCase))
                {
                    BotPost(CloudConfigurationManager.GetSetting("GroupMeTorrentBot"), command);
                }
                return Negotiate.WithStatusCode(HttpStatusCode.NoContent);
            };
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