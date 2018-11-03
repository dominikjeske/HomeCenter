using HomeCenter.Adapters.Common;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.ValueTypes;
using Proto;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.HSPE16InputOnly
{
    [ProxyCodeGenerator]
    public abstract class HSPE16InputOnlyAdapter : CCToolsBaseAdapter
    {
        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            var address = (IntValue)this[AdapterProperties.I2cAddress];
            byte[] setupAsInputs = { 0x06, 0xFF, 0xFF };
            var cmd = I2cCommand.Create(address, setupAsInputs);

            //TODO
            //await ExecuteCommand(RefreshCommand.Default).ConfigureAwait(false);
        }

        //public IBinaryInput GetInput(int number)
        //{
        //    // All ports have a pullup resistor.
        //    return ((IBinaryInput)GetPort(number)).WithInvertedState();
        //}

        //public IBinaryInput this[HSPE16Pin pin] => GetInput((int)pin);
    }
}