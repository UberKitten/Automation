using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class Chore
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class ChoreGroup
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Dictionary<Chore, User> Chores { get; set; }
    }
}