using HomeCenter.WindowsService.Core.Power;

namespace HomeCenter.WindowsService.Core.Interfaces
{
    public interface IPowerService
    {
        void SetPowerMode(ComputerPowerState powerState);
    }
}