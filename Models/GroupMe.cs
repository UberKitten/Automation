using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class GroupMeTorrentFinished
    {
        public string Name { get; set; }
    }

    public class GroupMeChatMessage
    {
        public string avatar_url { get; set; }
        public long created_at { get; set; }
        public long group_id { get; set; }
        public long id { get; set; }
        public string name { get; set; }
        public long sender_id { get; set; }
        public string sender_type { get; set; }
        public string source_guid { get; set; }
        public bool system { get; set; }
        public string text { get; set; }
        public long user_id { get; set; }
    }

    public class GroupMeUptimeRobotAlert
    {
        public long monitorId { get; set; }
        public string monitorURL { get; set; }
        public string monitorFriendlyName { get; set; }
        public int alertType { get; set; }
        public string alertDetails { get; set; }
        public string monitorAlertContacts { get; set; }
    }
}