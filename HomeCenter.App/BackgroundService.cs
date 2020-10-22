using HomeCenter.Abstractions;
using HomeCenter.Actors.Controllers;
using HomeCenter.Quartz;
using Microsoft.Extensions.Hosting;
using Proto;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.App
{
    internal class BackgroundService : IHostedService, IDisposable
    {
        private readonly IActorFactory _actorFactory;
        private readonly QuartzInitializer _quartzInitializer;

        //TODO generalize initializers?
        public BackgroundService(IActorFactory actorFactory, QuartzInitializer quartzInitializer)
        {
            _actorFactory = actorFactory;
            _quartzInitializer = quartzInitializer;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //TODO start actor
            await _quartzInitializer.Initialize();
        }

        private PID CreateController()
        {
            return _actorFactory.CreateActor<Controller>(nameof(Controller));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}