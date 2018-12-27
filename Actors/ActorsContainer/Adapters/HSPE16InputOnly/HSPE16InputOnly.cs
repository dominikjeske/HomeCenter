using HomeCenter.Adapters.Common;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries.Device;
using Microsoft.Extensions.Logging;
using Proto;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.HSPE16InputOnly
{
    [ProxyCodeGenerator]
    public abstract class HSPE16InputOnlyAdapter : CCToolsBaseAdapter
    {
        private int _interruptPin;

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _interruptPin = AsInt(MessageProperties.InterruptPin);

            byte[] setupInputs = { 0x06, 0xFF, 0xFF };
            var cmd = I2cCommand.Create(_i2cAddress, setupInputs);
            await MessageBroker.SendToService(cmd).ConfigureAwait(false);

            await FetchState().ConfigureAwait(false);
            
        }

        protected DiscoveryResponse QueryCapabilities(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(ReadWriteMode.Read));
        }

        [Subscribe]
        protected async Task Hadle(PinValueChangedEvent pinValueChangedEvent)
        {
            Logger.LogInformation($"PIN is changing");
            
            var pinNumber = pinValueChangedEvent.AsInt(MessageProperties.PinNumber);

            if (pinNumber == _interruptPin)
            {
                await FetchState().ConfigureAwait(false);
            }
        }
    }
}