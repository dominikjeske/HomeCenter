using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Contracts;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events.Service;
using HomeCenter.Services.Configuration;
using System.Threading.Tasks;

namespace HomeCenter.Services.Controllers
{
    [ProxyCodeGenerator]
    public abstract class Controller : DeviceActor
    {
        private readonly StartupConfiguration _startupConfiguration;
        private readonly IActorFactory _actorFactory;

        protected Controller(StartupConfiguration startupConfiguration, IActorFactory actorFactory)
        {
            _actorFactory = actorFactory;
            _startupConfiguration = startupConfiguration;
        }

        protected override async Task OnStarted(Proto.IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            StartSystemFromConfiguration();
        }

        private void StartSystemFromConfiguration()
        {
            var confService = _actorFactory.GetActor<ConfigurationService>(nameof(ConfigurationService));
            MessageBroker.Send(StartSystemCommand.Create(_startupConfiguration.ConfigurationLocation), confService);
        }

        [Subscibe]
        protected Task Handle(SystemStartedEvent systemStartedEvent)
        {
            return RunScheduler();
        }

        private Task RunScheduler() => Scheduler.Start(_disposables.Token);
    }
}