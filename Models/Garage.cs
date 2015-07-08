using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class GarageStatus
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Open { get; set; }
    }
}