using HomeCenter.Abstractions;
using HomeCenter.Actors.Core;
using HomeCenter.Messages.Events.Service;
using HomeCenter.Messages.Queries;
using HomeCenter.Services.Controllers;
using Proto;
using System.Threading.Tasks;

namespace HomeCenter.Actors.Controllers
{
    [Proxy]
    public class Controller : DeviceActor
    {
        private readonly StartupConfiguration _startupConfiguration;
        private readonly IActorFactory _actorFactory;
        private readonly IActorScheduler _actorScheduler;
        private PID _configService;

        protected Controller(StartupConfiguration startupConfiguration, IActorFactory actorFactory, IActorScheduler actorScheduler)
        {
            _actorFactory = actorFactory;
            _actorScheduler = actorScheduler;
            _startupConfiguration = startupConfiguration;
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context);

            StartSystemFromConfiguration(context);
        }

        private void StartSystemFromConfiguration(IContext context)
        {
            //TODO DNF
            //_configService = _actorFactory.CreateActor<ConfigurationService>(parent: context);

            //MessageBroker.Send(StartSystemCommand.Create(_startupConfiguration.ConfigurationLocation), _configService);
        }

        protected override Task OnSystemStarted(SystemStartedEvent systemStartedEvent)
        {
            return RunScheduler();
        }

        protected async Task<bool> Handle(StopSystemQuery stopSystemCommand)
        {
            await MessageBroker.Request<StopSystemQuery, bool>(StopSystemQuery.Default, _configService);

            await _actorFactory.Context.StopAsync(_configService);

            await _actorScheduler.ShutDown();

            await _actorFactory.Context.StopAsync(_actorFactory.GetExistingActor(nameof(Controller)));

            return true;
        }

        private Task RunScheduler() => _actorScheduler.Start(Token);
    }
}