using HomeCenter.Model.Conditions;
using HomeCenter.Model.Messages.Commands;
using Proto;
using Quartz;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HomeCenter.Model.Triggers
{
    public class TriggerJobDataDTO
    {
        public IValidable Condition { get; set; }
        public PID Actor { get; set; }
        public IList<Command> Commands { get; set; }
        public IList<Command> FinishCommands { get; set; }
        public TimeSpan? FinishCommandTime { get; set; }
        public CancellationToken Token { get; set; }

        public JobDataMap ToJobDataMap() => new JobDataMap { { "context", this } };
    }
}