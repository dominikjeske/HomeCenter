using System.Threading.Tasks;
using HomeCenter.ComponentModel.Adapters.Drivers;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Core.Services.I2C;

namespace HomeCenter.ComponentModel.Adapters
{
    public sealed class HSPE16InputOnlyAdapter : CCToolsBaseAdapter
    {
        public HSPE16InputOnlyAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
        }

        public override async Task Initialize()
        {
            if (!IsEnabled) return;

            var address = (IntValue)this[AdapterProperties.I2cAddress];
            var i2cAddress = new I2CSlaveAddress(address.Value);
            _portExpanderDriver = new MAX7311Driver(i2cAddress, _i2CBusService);

            await base.Initialize().ConfigureAwait(false);

            byte[] setupAsInputs = { 0x06, 0xFF, 0xFF };
            _i2CBusService.Write(i2cAddress, setupAsInputs);

            await ExecuteCommand(new Command(CommandType.RefreshCommand)).ConfigureAwait(false);
        }

        //public HSPE16InputOnlyAdapter(string id, I2CSlaveAddress address, II2CBusService i2CBusService, ILogger log)
        //    : base(id, new MAX7311Driver(address, i2CBusService), log)
        //{
        //    byte[] setupAsInputs = { 0x06, 0xFF, 0xFF };
        //    i2CBusService.Write(address, setupAsInputs);

        //    FetchState();
        //}

        //public IBinaryInput GetInput(int number)
        //{
        //    // All ports have a pullup resistor.
        //    return ((IBinaryInput)GetPort(number)).WithInvertedState();
        //}

        //public IBinaryInput this[HSPE16Pin pin] => GetInput((int)pin);
    }
}