using HomeCenter.Abstractions;
using HomeCenter.Actors.Core;
using HomeCenter.Messages.Commands.Service;
using HomeCenter.Messages.Events.Service;
using HomeCenter.Messages.Queries;
using HomeCenter.Services.Configuration;
using Microsoft.Extensions.Options;
using Proto;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace HomeCenter.Actors.Controllers
{
    [Proxy]
    public class RootActor : DeviceActor
    {
        private readonly IActorFactory _actorFactory;
        private readonly IActorScheduler _actorScheduler;
        private PID? _configService;
        private readonly HomeCenterOptions _homeCenterOptions;

        protected RootActor(IOptions<HomeCenterOptions> homeCenterOptions, IActorFactory actorFactory, IActorScheduler actorScheduler)
        {
            _actorFactory = actorFactory;
            _actorScheduler = actorScheduler;
            _homeCenterOptions = homeCenterOptions.Value;
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context);

            StartSystemFromConfiguration(context);
        }

        private void StartSystemFromConfiguration(IContext context)
        {
            if (string.IsNullOrWhiteSpace(_homeCenterOptions.ConfigurationLocation)) throw new ArgumentException(nameof(_homeCenterOptions.ConfigurationLocation));

            _configService = _actorFactory.CreateActor<ConfigurationService>(parent: context);

            var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (assemblyLocation is null) throw new InvalidOperationException(); ;

            var configPath = Path.Combine(assemblyLocation, _homeCenterOptions.ConfigurationLocation);
            MessageBroker.Send(StartSystemCommand.Create(configPath), _configService);
        }

        protected override Task OnSystemStarted(SystemStartedEvent systemStartedEvent)
        {
            return RunScheduler();
        }

        protected async Task<bool> Handle(StopSystemQuery stopSystemCommand)
        {
            if (_configService is null) throw new InvalidOperationException();

            await MessageBroker.Request<StopSystemQuery, bool>(StopSystemQuery.Default, _configService);

            await _actorFactory.Context.StopAsync(_configService);

            await _actorScheduler.ShutDown();

            await _actorFactory.Context.StopAsync(_actorFactory.GetExistingActor(nameof(RootActor)));

            return true;
        }

        private Task RunScheduler() => _actorScheduler.Start(Token);
    }
}