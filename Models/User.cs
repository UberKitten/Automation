using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation
{
    public class User : IUserIdentity
    {
        public User()
        {
            Claims = new List<String>();
        }

        public string UserName { get; set; }

        public IEnumerable<string> Claims { get; set; }
    }
}