using HomeCenter.WindowsService.Core.Interfaces;
using HomeCenter.WindowsService.Core.Interop;
using HomeCenter.WindowsService.Core.Power;
using System.Diagnostics;

namespace HomeCenter.WindowsService.Core.Services
{
    public class PowerService : IPowerService
    {
        public void SetPowerMode(ComputerPowerState powerState)
        {
            switch (powerState)
            {
                case ComputerPowerState.Hibernate:
                    Win32Api.SetSuspendState(true, true, true);
                    break;

                case ComputerPowerState.Sleep:
                    Win32Api.SetSuspendState(false, true, true);
                    break;

                case ComputerPowerState.Shutdown:
                    Process.Start("shutdown", "/s /t 0");
                    break;

                case ComputerPowerState.Restart:
                    Process.Start("shutdown", "/r /t 0");
                    break;

                case ComputerPowerState.LogOff:
                    Win32Api.ExitWindowsEx(0, 0);
                    break;

                case ComputerPowerState.Lock:
                    Win32Api.LockWorkStation();
                    break;
            }
        }
    }
}