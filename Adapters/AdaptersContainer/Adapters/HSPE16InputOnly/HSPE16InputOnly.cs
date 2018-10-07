using HomeCenter.Adapters.Common;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Native;
using HomeCenter.Model.ValueTypes;
using Proto;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.HSPE16InputOnly
{
    [ProxyCodeGenerator]
    public abstract class HSPE16InputOnlyAdapter : CCToolsBaseAdapter
    {
        protected HSPE16InputOnlyAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {

        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            var address = (IntValue)this[AdapterProperties.I2cAddress];
            var i2cAddress = new I2CSlaveAddress(address.Value);
            _portExpanderDriver = new MAX7311Driver(i2cAddress, _i2CBusService);

            await base.OnStarted(context).ConfigureAwait(false);

            byte[] setupAsInputs = { 0x06, 0xFF, 0xFF };
            _i2CBusService.Write(i2cAddress, setupAsInputs);

            //TODO
            //await ExecuteCommand(RefreshCommand.Default).ConfigureAwait(false);
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