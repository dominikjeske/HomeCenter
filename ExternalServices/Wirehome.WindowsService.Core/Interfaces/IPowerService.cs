using Wirehome.ComponentModel.Adapters.Pc;

namespace Wirehome.WindowsService.Services
{
    public interface IPowerService
    {
        void SetPowerMode(ComputerPowerState powerState);
    }
}