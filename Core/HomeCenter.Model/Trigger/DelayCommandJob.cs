using HomeCenter.Model.Commands;
using Quartz;
using System.Threading.Tasks;

namespace HomeCenter.Model.Extensions
{
    public class DelayCommandJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var trigger = context.GetDataContext<Command>();
            
        }

    }
}