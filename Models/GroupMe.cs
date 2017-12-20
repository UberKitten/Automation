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

    public class GroupMeBotPost
    {
        public string text { get; set; }
        public string bot_id { get; set; }
        public List<IGroupMeAttachment> attachments { get; set; }
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

    public class GroupMeResponseWrapper<T>
    {
        public GroupMeResponseMeta meta { get; set; }

        public T response { get; set; }
    }

    public class GroupMeResponseMeta
    {
        public int code { get; set; }
    }

    public class GroupMeGroup
    {
        public long created_at { get; set; }
        public long creator_user_id { get; set; }
        public string description { get; set; }
        public long group_id { get; set; }
        public long id { get; set; }
        public string image_url { get; set; }
        public int max_members { get; set; }
        public string name { get; set; }
        public bool office_mode { get; set; }
        public string phone_number { get; set; }
        public string share_url { get; set; }
        public string type { get; set; }
        public long updated_at { get; set; }

        public List<GroupMeGroupMember> members { get; set; }

        public List<GroupMeGroupMessages> messages { get; set; }
    }

    public class GroupMeGroupMember
    {
        public bool autokicked { get; set; }
        public long id { get; set; }
        public string image_url { get; set; }
        public bool muted { get; set; }
        public string nickname { get; set; }
        public long user_id { get; set; }
    }

    public class GroupMeGroupMessages
    {
        public long last_message_created_at { get; set; }
        public long last_message_id { get; set; }

        public GroupMeGroupMessagesPreview preview { get; set; }
    }

    public class GroupMeGroupMessagesPreview
    {
        public List<object> attachments { get; set; }
        public string image_url { get; set; }
        public string nickname { get; set; }
        public string text { get; set; }
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

    public interface IGroupMeAttachment
    {
        string type { get; }
    }

    public class GroupMeAttachmentMention : IGroupMeAttachment
    {
        public string type { get { return "mentions";  } }
        public List<long> user_ids { get; set; }
        public List<List<int>> loci { get; set; }
    }
    
}