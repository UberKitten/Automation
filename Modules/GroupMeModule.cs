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
                var command = commandRegex.Groups[commandRegex.Groups.Count - 1].Value.ToLower();

                if (firstword.Equals("@ChoreBot", StringComparison.CurrentCultureIgnoreCase))
                {
                    bool answerFound = false;
                    bool dateSpecified = false;

                    // Let empty commands fall straight though
                    if (!String.IsNullOrWhiteSpace(command))
                    {
                        List<ChoreGroup> choreGroups = null;

                        // Try parsing for date
                        var dateParser = new Chronic.Parser();
                        var parseObj = dateParser.Parse(command);
                        
                        if (parseObj != null && parseObj.Start.HasValue)
                        {
                            // Retrieve chores using that date
                            choreGroups = ChoreModule.GetChoresForDate(parseObj.Start.Value);
                            dateSpecified = true;
                        }
                        else
                        {
                            // Assume it's based on a current query
                            choreGroups = ChoreModule.GetChoresForDate(DateTime.Now);
                        }

                        // My chores
                        if (command.ToLower().Contains("me") || command.ToLower().Contains("mine") || command.ToLower().Contains("my"))
                        {
                            var myChores = choreGroups.SelectMany(t => t.Chores)
                                .Where(t => t.User.GroupMeId == model.sender_id);

                            foreach (var chore in myChores)
                            {
                                answerFound = true;

                                PostChore(chore, CloudConfigurationManager.GetSetting("GroupMeChoreBot"),
                                    GroupMeChoreDetail.Name | GroupMeChoreDetail.Description);
                            }
                        }

                        // Someone else's chores
                        if (!answerFound)
                        {
                            // Check user names
                            var userNameChores = choreGroups.SelectMany(t => t.Chores)
                                .Where(t => command.Contains(t.User.UserName.ToLower()));

                            foreach (var chore in userNameChores)
                            {
                                answerFound = true;

                                PostChore(chore, CloudConfigurationManager.GetSetting("GroupMeChoreBot"),
                                    GroupMeChoreDetail.Name | GroupMeChoreDetail.Description);
                            }

                            // Maybe check GroupMe names?
                        }

                        // Search for a chore name
                        if (!answerFound)
                        {
                            var choreNames = choreGroups.SelectMany(t => t.Chores)
                                .Where(t => command.Contains(t.Name.ToLower()));

                            foreach (var chore in choreNames)
                            {
                                answerFound = true;

                                var choreDetail = GroupMeChoreDetail.Name | GroupMeChoreDetail.Description;
                                if (command.Contains("notify"))
                                {
                                    choreDetail = choreDetail | GroupMeChoreDetail.CurrentUserMention;
                                }
                                else
                                {
                                    choreDetail = choreDetail | GroupMeChoreDetail.CurrentUser;
                                }

                                PostChore(chore, CloudConfigurationManager.GetSetting("GroupMeChoreBot"), choreDetail);
                            }
                        }

                        // Search for a chore group name
                        if (!answerFound)
                        {
                            var nameChoreGroups = choreGroups.Where(t => command.Contains(t.Name.ToLower()));

                            foreach (var choreGroup in nameChoreGroups)
                            {
                                var choreGroupDetail = GroupMeChoreGroupDetail.Name;
                                var choreDetail = GroupMeChoreDetail.Name;

                                if (command.Contains("notify"))
                                {
                                    choreDetail = choreDetail | GroupMeChoreDetail.CurrentUserMention;
                                }
                                else
                                {
                                    choreDetail = choreDetail | GroupMeChoreDetail.CurrentUser;
                                }

                                // If specific date requested use dates and don't include description
                                if (dateSpecified)
                                {
                                    choreGroupDetail = choreGroupDetail | GroupMeChoreGroupDetail.ScheduleDates;
                                }
                                else
                                {
                                    choreGroupDetail = choreGroupDetail | GroupMeChoreGroupDetail.Schedule;
                                    choreDetail = choreDetail | GroupMeChoreDetail.Description;
                                }

                                PostChoreGroup(choreGroup, CloudConfigurationManager.GetSetting("GroupMeChoreBot"),
                                    choreGroupDetail, choreDetail);

                                answerFound = true;
                            }
                        }

                        if (!answerFound)
                        {
                            // Post every group and every chore in every group
                            // A lot of data
                            if (command.Contains("all"))
                            {
                                var choreGroupDetail = GroupMeChoreGroupDetail.Name;
                                var choreDetail = GroupMeChoreDetail.Name | GroupMeChoreDetail.CurrentUser;

                                // If specific date requested use dates and don't include description
                                if (dateSpecified)
                                {
                                    choreGroupDetail = choreGroupDetail | GroupMeChoreGroupDetail.ScheduleDates;
                                }
                                else
                                {
                                    choreGroupDetail = choreGroupDetail | GroupMeChoreGroupDetail.Schedule;
                                    choreDetail = choreDetail | GroupMeChoreDetail.Description;
                                }
                                
                                foreach (var choreGroup in ChoreModule.GetChoresForDate(DateTime.Now).Where(t => t.Chores.Count > 0))
                                {
                                    PostChoreGroup(choreGroup, CloudConfigurationManager.GetSetting("GroupMeChoreBot"),
                                        choreGroupDetail, choreDetail);
                                }

                                answerFound = true;
                            }
                        }

                        if (!answerFound)
                        {
                            // Usage details
                            if (command.Contains("help"))
                            {
                                BotPost(CloudConfigurationManager.GetSetting("GroupMeChoreBot"), "Commands: all, my/me/mine, person name, chore group name (+ notify), chore name");
                                BotPost(CloudConfigurationManager.GetSetting("GroupMeChoreBot"), "All commands can be combined with a relative or absolute date: tomorrow, next Monday, 9/2, last month, etc");

                                answerFound = true;
                            }
                        }
                    }

                    // Post every group with minimal detail
                    if (!answerFound)
                    {
                        var choreGroupDetail = GroupMeChoreGroupDetail.Name | GroupMeChoreGroupDetail.ListChoreNames;
                        var choreDetail = GroupMeChoreDetail.None;

                        // If specific date requested use dates and don't include description
                        if (dateSpecified)
                        {
                            choreGroupDetail = choreGroupDetail | GroupMeChoreGroupDetail.ScheduleDates;
                        }
                        else
                        {
                            choreGroupDetail = choreGroupDetail | GroupMeChoreGroupDetail.Schedule;
                        }

                        var choreGroups = ChoreModule.GetChoresForDate(DateTime.Now);
                        foreach (var choreGroup in choreGroups.Where(t => t.Chores.Count > 0))
                        {
                            PostChoreGroup(choreGroup, CloudConfigurationManager.GetSetting("GroupMeChoreBot"), choreGroupDetail, choreDetail);
                        }
                    }
                }
                else if (firstword.Equals("@TorrentBot", StringComparison.CurrentCultureIgnoreCase))
                {
                    BotPost(CloudConfigurationManager.GetSetting("GroupMeTorrentBot"), "No commands for this bot");
                }
                return Negotiate.WithStatusCode(HttpStatusCode.NoContent);
            };

            Get["/chorebot/nag/{group}"] = _ => {
                this.RequiresClaims(new[] { "ChoreBotNag" });

                var choreGroups = ChoreModule.GetChoresForDate(DateTime.Now);
                var choreGroup = choreGroups.SingleOrDefault(t => t.Name.Equals(_.group));
                
                if (choreGroup == null)
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.BadRequest);
                }
                
                if (choreGroup.Chores.Count == 0)
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.NoContent);
                }

                PostChoreGroup(choreGroup, CloudConfigurationManager.GetSetting("GroupMeChoreBot"),
                    GroupMeChoreGroupDetail.Name | GroupMeChoreGroupDetail.ScheduleDates,
                    GroupMeChoreDetail.Name | GroupMeChoreDetail.CurrentUserMention);
                return Negotiate.WithStatusCode(HttpStatusCode.NoContent);
            };
        }
        
        private void PostChoreGroup(ChoreGroup choreGroup, string botId, GroupMeChoreGroupDetail choreGroupDetail, GroupMeChoreDetail choreDetail)
        {
            GroupMeGroup group = null;
            // Only get group info if we need it to @mention
            if (choreDetail.HasFlag(GroupMeChoreDetail.CurrentUserMention))
            {
                group = GetGroupInfo(CloudConfigurationManager.GetSetting("GroupMeGroupId"));
            }

            // Not "None"
            if (choreGroupDetail != 0)
            {
                string text = String.Empty;
                // Use Name
                if (choreGroupDetail.HasFlag(GroupMeChoreGroupDetail.Name))
                {
                    text += choreGroup.Name;

                    // Padding at end
                    if (choreGroupDetail.HasFlag(GroupMeChoreGroupDetail.Schedule) ||
                        choreGroupDetail.HasFlag(GroupMeChoreGroupDetail.ScheduleDates) ||
                        choreGroupDetail.HasFlag(GroupMeChoreGroupDetail.ListChoreNames))
                    {
                        text += " - ";
                    }
                }

                // Use Schedule
                if (choreGroupDetail.HasFlag(GroupMeChoreGroupDetail.Schedule))
                {
                    text += "Every " + choreGroup.RecurrenceCount + " " + choreGroup.RecurrenceDatePart;
                    if (choreGroup.RecurrenceCount > 1)
                    {
                        text += "s";
                    }

                    if (!String.IsNullOrEmpty(choreGroup.SkipDatePart))
                    {
                        text += ", skipping " + choreGroup.SkipCount + " " + choreGroup.SkipDatePart;
                        if (choreGroup.SkipCount > 1)
                        {
                            text += "s";
                        }
                        text += " in between";
                    }

                    // Padding at end
                    if (choreGroupDetail.HasFlag(GroupMeChoreGroupDetail.ScheduleDates) ||
                        choreGroupDetail.HasFlag(GroupMeChoreGroupDetail.ListChoreNames))
                    {
                        text += " - ";
                    }
                }

                // Use Schedule Dates
                if (choreGroupDetail.HasFlag(GroupMeChoreGroupDetail.ScheduleDates))
                {
                    text += choreGroup.CurrentRecurrenceStart.ToShortDateString();
                    text += " to " + choreGroup.CurrentRecurrenceEnd.ToShortDateString();

                    // Padding at end
                    if (choreGroupDetail.HasFlag(GroupMeChoreGroupDetail.ListChoreNames))
                    {
                        text += " - ";
                    }
                }

                // List chore names in header
                if (choreGroupDetail.HasFlag(GroupMeChoreGroupDetail.ListChoreNames))
                {
                    foreach(var chore in choreGroup.Chores)
                    {
                        text += chore.Name + ", ";
                    }
                    text = text.Substring(0, text.Length - 2); // remove final comma and space
                }

                // Post header message
                if (!String.IsNullOrEmpty(text))
                {
                    BotPost(botId, text);
                }
            }

            // Post message for each chore
            foreach (var chore in choreGroup.Chores)
            {
                PostChore(chore, botId, choreDetail, group);
            }
        }
        
        private void PostChore(Chore chore, string botId, GroupMeChoreDetail choreDetail, GroupMeGroup group = null)
        {
            // Only get group info if we need it to @mention
            if (group == null && choreDetail.HasFlag(GroupMeChoreDetail.CurrentUserMention))
            {
                group = GetGroupInfo(CloudConfigurationManager.GetSetting("GroupMeGroupId"));
            }

            // Not "None"
            if (choreDetail != 0)
            {
                string text = String.Empty;
                // Use Name
                if (choreDetail.HasFlag(GroupMeChoreDetail.Name))
                {
                    text += chore.Name;

                    // Use Description
                    if (choreDetail.HasFlag(GroupMeChoreDetail.Description))
                    {
                        // Use Current User
                        if (choreDetail.HasFlag(GroupMeChoreDetail.CurrentUser))
                        {
                            text += " (" + chore.User.UserName + ")";
                        }
                        // Use Current User w/ mentioning
                        if (choreDetail.HasFlag(GroupMeChoreDetail.CurrentUserMention) && group != null)
                        {
                            var groupMeUser = group.members.SingleOrDefault(t => t.user_id == chore.User.GroupMeId);
                            if (groupMeUser == null)
                            {
                                text += " - @" + chore.User.UserName;
                            }
                            else
                            {
                                text += " - @" + groupMeUser.nickname;
                            }
                        }
                        text += " - " + chore.Description;
                    }
                    // Don't use Description
                    else
                    {
                        // Use Current User
                        if (choreDetail.HasFlag(GroupMeChoreDetail.CurrentUser))
                        {
                            text += " - " + chore.User.UserName;
                        }
                        // Use Current User w/ mentioning
                        if (choreDetail.HasFlag(GroupMeChoreDetail.CurrentUserMention) && group != null)
                        {
                            var groupMeUser = group.members.SingleOrDefault(t => t.user_id == chore.User.GroupMeId);
                            if (groupMeUser == null)
                            {
                                text += " - @" + chore.User.UserName;
                            }
                            else
                            {
                                text += " - @" + groupMeUser.nickname;
                            }
                        }
                    }
                }
                // Use Current User with or without mentioning
                else if (choreDetail.HasFlag(GroupMeChoreDetail.CurrentUser) || choreDetail.HasFlag(GroupMeChoreDetail.CurrentUserMention))
                {
                    // Use Current User
                    if (choreDetail.HasFlag(GroupMeChoreDetail.CurrentUser))
                    {
                        text += chore.User.UserName;
                    }
                    // Use Current User w/ mentioning
                    if (choreDetail.HasFlag(GroupMeChoreDetail.CurrentUserMention) && group != null)
                    {
                        var groupMeUser = group.members.Single(t => t.id == chore.User.GroupMeId);
                        text += "@" + groupMeUser.nickname;
                    }
                    // Use Description
                    if (choreDetail.HasFlag(GroupMeChoreDetail.Description))
                    {
                        text += ": " + chore.Description;
                    }
                }
                // Use Description only
                else
                {
                    text += chore.Description;
                }

                BotPost(botId, text);
            }
        }

        private GroupMeGroup GetGroupInfo(string groupId)
        {
            var client = new RestClient("https://api.groupme.com/v3");

            var request = new RestRequest("groups/{id}", Method.GET);
            request.AddUrlSegment("id", groupId);
            request.AddParameter("token", CloudConfigurationManager.GetSetting("GroupMeAccessToken"));

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
            System.Threading.Thread.Sleep(750);
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