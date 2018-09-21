using HomeCenter.Model.Commands.Specialized;
using HomeCenter.Model.Extensions;
using Proto;
using Quartz;
using System.Threading.Tasks;

namespace HomeCenter.ComponentModel.Adapters
{
    public class RefreshStateJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var adapter = context.GetDataContext<PID>();
            RootContext.Empty.Send(adapter, RefreshCommand.Default);
            return Task.CompletedTask;
        }
    }

    public class RefreshLightStateJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var adapter = context.GetDataContext<PID>();
            RootContext.Empty.Send(adapter, RefreshLightCommand.Default);
            return Task.CompletedTask;
        }
    }
}