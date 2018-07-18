using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.Core.EventAggregator;
using Wirehome.Model.Extensions;

namespace Wirehome.Core.Tests.Mocks
{
    public class TestAdapter : Adapter
    {
        private object locki = new object();
        public int Counter { get; private set; }

        public DiscoveryResponse DiscoveryResponse { get; set; }

        public TestAdapter(string uid, IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
            Uid = uid;
            DiscoveryResponse = new DiscoveryResponse(new PowerState());
        }

        public override Task Initialize()
        {
            _disposables.Add(_eventAggregator.SubscribeForDeviceQuery<DeviceCommand>(DeviceCommandHandler, Uid));

            return base.Initialize();
        }

        private Task<object> DeviceCommandHandler(IMessageEnvelope<DeviceCommand> messageEnvelope)
        {
            return ExecuteCommand(messageEnvelope.Message);
        }

        protected async Task RefreshCommandHandler(Command messageEnvelope)
        {
            lock (locki)
            {
                Counter++;

                if (Counter > 1)
                {
                    Counter += 100;
                }
            }

            await Task.Delay(100).ConfigureAwait(false);

            lock (locki)
            {
                Counter--;
            }
        }

        protected async Task<object> DiscoverCapabilitiesHandler(Command message)
        {
            await Task.Delay(100).ConfigureAwait(false);

            return DiscoveryResponse;
        }
    }
}