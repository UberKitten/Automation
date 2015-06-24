using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class Operation
    {
        public OperationZone Downstairs { get; set; }
        public OperationZone Upstairs { get; set; }
    }

    public class OperationZone
    {
        public OperationItem Compressor { get; set; }
        public OperationItem Fan { get; set; }
        public OperationItem Heat { get; set; }
    }

    public class OperationItem
    {
        public bool Active { get; set; }
        public TimeSpan Timeout { get; set; }
    }
}