using HomeCenter.Adapters.Common;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries.Device;
using Microsoft.Extensions.Logging;
using Proto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.HSPE16InputOnly
{
    [ProxyCodeGenerator]
    public abstract class HSPE16InputOnlyAdapter : CCToolsBaseAdapter
    {
        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            Subscribe<PinValueChangedEvent>(new Broker.RoutingFilter(new Dictionary<string, string>()
            {
                [MessageProperties.MessageSource] = AsString(MessageProperties.InterruptSource),
                [MessageProperties.PinNumber] = AsString(MessageProperties.InterruptPin)
            }));

            await ConfigureDriver(false, false).ConfigureAwait(false);
            await FetchState().ConfigureAwait(false);
        }

        protected DiscoveryResponse QueryCapabilities(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(ReadWriteMode.Read));
        }

        protected async Task Hadle(PinValueChangedEvent pinValueChangedEvent)
        {
            Logger.LogInformation($"PIN is changing");

            await FetchState().ConfigureAwait(false);
        }
    }
}