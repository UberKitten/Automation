using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class Token
    {

        [PrimaryKey]
        public string Value { get; set; }

        [ForeignKey(typeof(User))]
        public int UserId { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public User User { get; set; }

    }
}