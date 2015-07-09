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