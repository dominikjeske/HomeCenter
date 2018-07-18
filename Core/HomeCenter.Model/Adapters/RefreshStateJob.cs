using Quartz;
using System.Threading.Tasks;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.Model.Extensions;

namespace HomeCenter.ComponentModel.Adapters
{
    public class RefreshStateJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var adapter = context.GetDataContext<Adapter>();
            return adapter.ExecuteCommand(new Command(CommandType.RefreshCommand));
        }
    }

    public class RefreshLightStateJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var adapter = context.GetDataContext<Adapter>();
            return adapter.ExecuteCommand(new Command(CommandType.RefreshLightCommand));
        }
    }
}