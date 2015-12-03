using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace ChoreBot
{
    class Program
    {
        public static void NagChoreGroup(string choreGroup)
        {
            Automation.Modules.GroupMeModule.NagChoreGroup(choreGroup);
        }
    }
}
