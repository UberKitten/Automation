using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class UserClaim
    {

        [ForeignKey(typeof(User))]
        public int UserId { get; set; }

        [ForeignKey(typeof(Claim))]
        public int ClaimId { get; set; }
        
    }
}