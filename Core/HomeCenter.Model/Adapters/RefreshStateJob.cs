using HomeCenter.Model.Commands.Specialized;
using HomeCenter.Model.Extensions;
using Quartz;
using System.Threading.Tasks;

namespace HomeCenter.ComponentModel.Adapters
{
    public class RefreshStateJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var adapter = context.GetDataContext<Adapter>();
            return adapter.ExecuteCommand(RefreshCommand.Default);
        }
    }

    public class RefreshLightStateJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var adapter = context.GetDataContext<Adapter>();
            return adapter.ExecuteCommand(RefreshLightCommand.Default);
        }
    }
}