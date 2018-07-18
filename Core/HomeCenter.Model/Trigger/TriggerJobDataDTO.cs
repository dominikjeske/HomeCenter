using Quartz;
using System;
using System.Threading;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.Components;
using HomeCenter.Conditions;

namespace HomeCenter.Model.Extensions
{
    public class TriggerJobDataDTO
    {
        public IValidable Condition { get; set; }
        public IActor Actor { get; set; }
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