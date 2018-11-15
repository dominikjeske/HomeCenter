using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events.Service;
using HomeCenter.Services.Configuration;
using Proto.Remote;
using System.Threading.Tasks;

namespace HomeCenter.Services.Controllers
{
    [ProxyCodeGenerator]
    public abstract class Controller : DeviceActor
    {
        private readonly ControllerOptions _controllerOptions;
        private readonly IActorFactory _actorFactory;

        protected Controller(ControllerOptions controllerOptions, IActorFactory actorFactory)
        {
            _actorFactory = actorFactory;
            _controllerOptions = controllerOptions;
        }

        protected override async Task OnStarted(Proto.IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            StartSystemFromConfiguration();
        }
        
        private void StartSystemFromConfiguration()
        {
            var confService = _actorFactory.GetActor<ConfigurationService>(nameof(ConfigurationService));
            MessageBroker.Send(StartSystemCommand.Create(), confService);
        }

        protected async Task Handle(SystemStartedEvent systemStartedEvent)
        {
            await RunScheduler().ConfigureAwait(false);

            Remote.Start(_controllerOptions.RemoteActorAddress ?? "127.0.0.1", _controllerOptions.RemoteActorPort ?? 8000);
        }

        private Task RunScheduler() => Scheduler.Start(_disposables.Token);
    }
}