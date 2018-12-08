using HomeCenter.Model.Core;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Commands.Device;
using Proto;
using Quartz;
using System.Threading.Tasks;

namespace HomeCenter.Model.Adapters
{

    public class RefreshLightStateJob : IJob
    {
        private readonly IActorMessageBroker _actorMessageBroker;

        public RefreshLightStateJob(IActorMessageBroker actorMessageBroker)
        {
            _actorMessageBroker = actorMessageBroker;
        }

        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                var adapter = context.GetDataContext<PID>();
                _actorMessageBroker.Send(RefreshLightCommand.Default, adapter);
                return Task.CompletedTask;
            }
            catch (System.Exception)
            {

                throw;
            }


        }
    }
}