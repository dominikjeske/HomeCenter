using HomeCenter.Adapters.Common;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Queries.Device;
using Proto;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.HSREL8
{
    [ProxyCodeGenerator]
    public abstract class HSREL8Adapter : CCToolsBaseAdapter
    {
        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            await ConfigureDriver(true, false).ConfigureAwait(false);
            await FetchState().ConfigureAwait(false);
        }

        protected DiscoveryResponse QueryCapabilities(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState());
        }

        public Task Handle(TurnOnCommand message)
        {
            int pinNumber = GetPin(message);

            return SetPortState(pinNumber, true);
        }

        public Task Handle(TurnOffCommand message)
        {
            int pinNumber = GetPin(message);

            return SetPortState(pinNumber, false);
        }

        public Task Handle(SwitchPowerStateCommand message)
        {
            int pinNumber = GetPin(message);
            var currentState = _driver.GetState(pinNumber);

            return SetPortState(pinNumber, !currentState);
        }

        

        private int GetPin(Command message)
        {
            var pinNumber = message.AsInt(MessageProperties.PinNumber);
            //TODO change to 7 if we cant use other pins
            if (pinNumber < 0 || pinNumber > 15) throw new ArgumentOutOfRangeException(nameof(pinNumber));
            return pinNumber;
        }
    }
}