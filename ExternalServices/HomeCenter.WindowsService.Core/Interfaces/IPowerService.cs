using HomeCenter.Model.Adapters.Pc;

namespace HomeCenter.WindowsService.Services
{
    public interface IPowerService
    {
        void SetPowerMode(ComputerPowerState powerState);
    }
}