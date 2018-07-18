using Quartz;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Commands;
using Wirehome.Model.Extensions;

namespace Wirehome.ComponentModel.Adapters
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