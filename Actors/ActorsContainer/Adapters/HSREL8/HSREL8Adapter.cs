using HomeCenter.Adapters.Common;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.ValueTypes;
using Proto;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.HSREL8
{
    [ProxyCodeGenerator]
    public abstract class HSREL8Adapter : CCToolsBaseAdapter
    {
        protected override async Task OnStarted(IContext context)
        {
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