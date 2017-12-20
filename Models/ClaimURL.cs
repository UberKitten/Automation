using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class ClaimURL
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ClaimId { get; set; }

        [MaxLength(500)]
        public string URL { get; set; }
    }
}