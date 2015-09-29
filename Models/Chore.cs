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

        public User User { get; set; }
    }

    public class ChoreGroup
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string RecurrenceDatePart { get; set; }
        public int RecurrenceCount { get; set; }
        public string SkipDatePart { get; set; }
        public int SkipCount { get; set; }
        public DateTime CurrentRecurrenceStart { get; set; }
        public DateTime CurrentRecurrenceEnd { get; set; }

        public List<Chore> Chores { get; set; }
    }
}