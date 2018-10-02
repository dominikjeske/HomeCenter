using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Commands;
using Quartz;
using System.Threading.Tasks;

namespace HomeCenter.Model.Triggers
{
    public class DelayCommandJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var trigger = context.GetDataContext<Command>();
        }
    }
}