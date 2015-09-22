using Nancy.Security;
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
        }

        public string UserName { get; set; }

        public int GroupMeId { get; set; }

        [XmlIgnoreAttribute]
        public IEnumerable<string> Claims { get; set; }
    }
}