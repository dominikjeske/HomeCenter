using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events.Service;
using HomeCenter.Services.Configuration;
using HomeCenter.Utils;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Remote;
using System;
using System.Linq;
using System.Net.Http;
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

            var bindingAddress = _controllerOptions.RemoteActorAddress ?? GetAddressBinding();

            Remote.Start(bindingAddress, _controllerOptions.RemoteActorPort ?? 8000);
        }

        private string GetAddressBinding() => NetworkHelper.GetNetworkAddresses().Select(a => a?.ToString()).OrderByDescending(x => x).FirstOrDefault() ?? "127.0.0.1";

        private Task RunScheduler() => Scheduler.Start(_disposables.Token);
    }
}