using System.Threading.Tasks;
using HomeCenter.ComponentModel.Adapters.Drivers;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Core.Services.I2C;

namespace HomeCenter.ComponentModel.Adapters
{
    public class HSREL8Adapter : CCToolsBaseAdapter
    {
        public HSREL8Adapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
        }

        public override async Task Initialize()
        {
            if (!IsEnabled) return;

            var address = (IntValue)this[AdapterProperties.I2cAddress];
            _portExpanderDriver = new MAX7311Driver(new I2CSlaveAddress(address.Value), _i2CBusService);

            await base.Initialize().ConfigureAwait(false);

            SetState(new byte[] { 0x00, 255 }, true);
        }

        public void TurnOnCommandHandler(Command message)
        {
            
        }

        public void TurnOffCommandHandler(Command message)
        {

        }
    }
}