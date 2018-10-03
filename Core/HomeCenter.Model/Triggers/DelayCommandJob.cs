using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Commands;
using Quartz;
using System.Threading.Tasks;

namespace HomeCenter.Model.Triggers
{
    public class DelayCommandJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var trigger = context.GetDataContext<Command>();
            //TODO send command
            return Task.CompletedTask;
        }
    }
}