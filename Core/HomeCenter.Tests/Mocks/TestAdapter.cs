using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Commands;
using HomeCenter.Model.Commands.Responses;
using System.Threading.Tasks;

namespace HomeCenter.Core.Tests.Mocks
{
    public class TestAdapter : Adapter
    {
        private readonly object locki = new object();
        public int Counter { get; private set; }

        public DiscoveryResponse DiscoveryResponse { get; set; }

        public TestAdapter(string uid, IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
            Uid = uid;
            DiscoveryResponse = new DiscoveryResponse(new PowerState());
        }

        //public override Task Initialize()
        //{
        //    //TODO
        //    //_disposables.Add(_eventAggregator.SubscribeForDeviceQuery<DeviceCommand>(DeviceCommandHandler, Uid));

        //    return base.Initialize();
        //}

        //TODO
        //private Task<object> DeviceCommandHandler(IMessageEnvelope<DeviceCommand> messageEnvelope)
        //{
        //    return ExecuteCommand(messageEnvelope.Message);
        //}

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