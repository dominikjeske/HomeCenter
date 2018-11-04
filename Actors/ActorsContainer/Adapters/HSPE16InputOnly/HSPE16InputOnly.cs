using HomeCenter.Adapters.Common;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Queries.Device;
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

            byte[] setupInputs = { 0x06, 0xFF, 0xFF };
            var cmd = I2cCommand.Create(_i2cAddress, setupInputs);
            await MessageBroker.SendToService(cmd).ConfigureAwait(false);

            await ScheduleDeviceRefresh<RefreshStateJob>(_poolInterval).ConfigureAwait(false);
        }

        protected DiscoveryResponse QueryCapabilities(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(ReadWriteMode.Read));
        }
    }
}