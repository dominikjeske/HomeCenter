using System.Threading.Tasks;
using HomeCenter.ComponentModel.Adapters;
using HomeCenter.ComponentModel.Capabilities;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.Commands.Responses;
using HomeCenter.Messaging;
using HomeCenter.Model.Extensions;

namespace HomeCenter.Core.Tests.Mocks
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