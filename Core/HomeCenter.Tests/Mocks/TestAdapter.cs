using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Tests.Dummies;
using System.Threading.Tasks;

namespace HomeCenter.Tests.Mocks
{
    [ProxyCodeGenerator]
    public abstract class TestAdapter : Adapter
    {
        protected DiscoveryResponse Handle(DiscoverQuery discoverQuery)
        {
            return new DiscoveryResponse(new PowerState());
        }

        protected TestAdapter Handle(GetAdapterQuery getAdapterQuery)
        {
            return this;
        }

        public async Task PropertyChanged<T>(string state, T oldValue, T newValue)
        {
            await UpdateState(state, oldValue, newValue).ConfigureAwait(false);
        }
    }
}