﻿using AutoMapper;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Contracts;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events.Service;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Services.Configuration;
using Proto;
using System.Threading.Tasks;

namespace HomeCenter.Services.Controllers
{
    [ProxyCodeGenerator]
    public abstract class Controller : DeviceActor
    {
        private readonly StartupConfiguration _startupConfiguration;
        private readonly IActorFactory _actorFactory;
        private PID _configService;

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
            _configService = _actorFactory.CreateActor<ConfigurationService>();
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
            await Scheduler.Shutdown();
            Mapper.Reset(); // TODO configuration is using static mapper - fix this because second execution need this static reset

            _actorFactory.GetExistingActor(nameof(Controller)).Stop();

            return true;
            
        }

        private Task RunScheduler() => Scheduler.Start(_disposables.Token);
    }
}