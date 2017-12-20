using Automation.Models;
using Nancy.Security;
using SQLite;
using SQLiteNetExtensions.Attributes;
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
            GroupMeId = -1;
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string UserName { get; set; }

        public int GroupMeId { get; set; }
        
        [ManyToMany(typeof(UserClaim))]
        public List<Claim> ClaimObjects { get; set; }

        // Needed for IUserIdentity
        public IEnumerable<string> Claims { get
            {
                return ClaimObjects.Select(t => t.Name);
            }
        }

        public override string ToString()
        {
            var output = "";

            if (UserName != null)
            {
                output += "UserName: " + UserName + Environment.NewLine;
            }

            if (GroupMeId != -1)
            {
                output += "GroupMeId: " + GroupMeId + Environment.NewLine;
            }

            if (Claims != null)
            {
                output += "Claims: " + String.Join(Environment.NewLine, Claims) + Environment.NewLine;
            }

            return output;
        }
    }
}