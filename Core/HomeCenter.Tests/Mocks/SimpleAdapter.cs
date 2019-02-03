using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Tests.Dummies;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace HomeCenter.Tests.Mocks
{
    [ProxyCodeGenerator]
    public abstract class SimpleAdapter : Adapter, ITestAdapter
    {

        public Subject<Command> CommandRecieved { get; } = new Subject<Command>();

        protected DiscoveryResponse Handle(DiscoverQuery discoverQuery)
        {
            return new DiscoveryResponse(new PowerState());
        }

        public void Handle(TurnOnCommand turnOnCommand)
        {
            CommandRecieved.OnNext(turnOnCommand);
        }

        protected SimpleAdapter Handle(GetAdapterQuery getAdapterQuery)
        {
            return this;
        }

        public async Task PropertyChanged<T>(string state, T oldValue, T newValue)
        {
            await UpdateState(state, oldValue, newValue).ConfigureAwait(false);
        }
    }
}