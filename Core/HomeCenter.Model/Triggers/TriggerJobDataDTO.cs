using HomeCenter.Model.Conditions;
using HomeCenter.Model.Messages.Commands;
using Proto;
using Quartz;
using System;
using System.Threading;

namespace HomeCenter.Model.Triggers
{
    public class TriggerJobDataDTO
    {
        public IValidable Condition { get; set; }
        public PID Actor { get; set; }
        public Command Command { get; set; }
        public Command FinishCommand { get; set; }
        public TimeSpan? FinishCommandTime { get; set; }
        public CancellationToken Token { get; set; }

        public JobDataMap ToJobDataMap()
        {
            return new JobDataMap { { "context", this } };
        }
    }
}