using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Quartz;
using Quartz.Impl;

namespace ChoreBot
{
    class Program
    {
        static void Main()
        {
            var host = new JobHost();
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();

            // Quartz.NET scheduling
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            var householdJob = JobBuilder.Create<NagChoreGroupJob>()
                .UsingJobData("ChoreGroup", "Household")
                .Build();

            var trigger = TriggerBuilder.Create()
                .StartNow()
                .Build();

            scheduler.ScheduleJob(householdJob, trigger);
        }


        
    }

    public class NagChoreGroupJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Automation.Modules.GroupMeModule.NagChoreGroup((string)context.Get("ChoreGroup"));
        }
    }
}
