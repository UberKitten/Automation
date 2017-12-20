using Nancy.Security;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Automation
{
    public class User : IUserIdentity
    {
        public User()
        {
            Claims = new List<String>();
            GroupMeId = -1;
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string UserName { get; set; }

        [Ignore]
        public string Token { get; set; }

        public int GroupMeId { get; set; }

        [XmlIgnoreAttribute]
        [Ignore]
        public IEnumerable<string> Claims { get; set; }

        [XmlIgnoreAttribute]
        [Ignore]
        public IEnumerable<string> ClaimURLs { get; set; }

        public override string ToString()
        {
            var output = "";

            if (UserName != null)
            {
                output += "UserName: " + UserName + Environment.NewLine;
            }
            
            if (Token != null)
            {
                output += "Token " + Token.Length + " characters" + Environment.NewLine;
            }

            if (GroupMeId != -1)
            {
                output += "GroupMeId: " + GroupMeId + Environment.NewLine;
            }

            if (Claims != null)
            {
                output += "Claims: " + String.Join(Environment.NewLine, Claims) + Environment.NewLine;
            }

            if (ClaimURLs != null)
            {
                output += "ClaimURLs: " + String.Join(Environment.NewLine, ClaimURLs) + Environment.NewLine;
            }

            return output;
        }
    }
}