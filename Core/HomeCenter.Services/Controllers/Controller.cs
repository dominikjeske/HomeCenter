using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Contracts;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events.Service;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Services.Configuration;
using Proto;
using Quartz;
using System.Threading.Tasks;

namespace HomeCenter.Services.Controllers
{
    [ProxyCodeGenerator]
    public abstract class Controller : DeviceActor
    {
        private readonly StartupConfiguration _startupConfiguration;
        private readonly IActorFactory _actorFactory;
        private PID _configService;
        private readonly IScheduler _scheduler;

        protected Controller(StartupConfiguration startupConfiguration, IActorFactory actorFactory, IScheduler scheduler)
        {
            _actorFactory = actorFactory;
            _startupConfiguration = startupConfiguration;
            _scheduler = scheduler;
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context);

            StartSystemFromConfiguration(context);
        }

        private void StartSystemFromConfiguration(IContext context)
        {
            _configService = _actorFactory.CreateActor<ConfigurationService>(parent: context);

            MessageBroker.Send(StartSystemCommand.Create(_startupConfiguration.ConfigurationLocation), _configService);
        }

        protected override Task OnSystemStarted(SystemStartedEvent systemStartedEvent)
        {
            return RunScheduler();
        }

        protected async Task<bool> Handle(StopSystemQuery stopSystemCommand)
        {
            await MessageBroker.Request<StopSystemQuery, bool>(StopSystemQuery.Default, _configService);
            await _configService.StopAsync();
            await _scheduler.Shutdown();

            _actorFactory.GetExistingActor(nameof(Controller)).Stop();

            return true;
        }

        private Task RunScheduler() => _scheduler.Start(Token);
    }
}