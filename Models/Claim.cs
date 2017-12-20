using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class Claim
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public string Name { get; set; }
        
        [ManyToMany(typeof(UserClaim))]
        public List<User> Users { get; set; }

        [OneToMany]
        public List<ClaimURL> ClaimURLs { get; set; }
    }
}