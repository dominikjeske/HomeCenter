using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Queries.Device;
using System.Threading.Tasks;

namespace HomeCenter.Tests.Mocks
{
    public class TestAdapter : Adapter
    {
        private readonly object locki = new object();
        public int Counter { get; private set; }

        public DiscoveryResponse DiscoveryResponse { get; set; }

        public TestAdapter(string uid)
        {
            Uid = uid;
            DiscoveryResponse = new DiscoveryResponse(new PowerState());
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