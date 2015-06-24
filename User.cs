using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation
{
    public class User : IUserIdentity
    {
        public string UserName { get; set; }

        private List<String> _Claims;
        public IEnumerable<string> Claims {
            get
            {
                if (_Claims == null)
                {
                    _Claims = new List<string>();
                }
                return _Claims;
            }
            set
            {
                _Claims = new List<string>(value);
            }
        }
    }
}