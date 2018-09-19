using HomeCenter.ComponentModel.Adapters.Drivers;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Core.Services.I2C;
using HomeCenter.Model.Commands.Specialized;
using Proto;
using System.Threading.Tasks;

namespace HomeCenter.ComponentModel.Adapters
{
    public class HSREL8Adapter : CCToolsBaseAdapter
    {
        public HSREL8Adapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
        }

        protected override async Task OnStarted(IContext context)
        {
            var address = (IntValue)this[AdapterProperties.I2cAddress];
            _portExpanderDriver = new MAX7311Driver(new I2CSlaveAddress(address.Value), _i2CBusService);

            await base.OnStarted(context).ConfigureAwait(false);

            SetState(new byte[] { 0x00, 255 }, true);
        }

        public void TurnOn(TurnOnCommand message)
        {
        }

        public void TurnOff(TurnOffCommand message)
        {
        }
    }
}