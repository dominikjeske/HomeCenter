using HomeCenter.Broker;

namespace HomeCenter.Model.Core
{
    public abstract class Service : DeviceActor
    {
        protected Service(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }
    }
}