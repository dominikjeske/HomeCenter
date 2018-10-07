using HomeCenter.Adapters.Common;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Native;
using HomeCenter.Model.ValueTypes;
using Proto;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.HSREL8
{
    [ProxyCodeGenerator]
    public abstract class HSREL8Adapter : CCToolsBaseAdapter
    {
        protected HSREL8Adapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
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