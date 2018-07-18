using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Wirehome.WindowsService.Interop;

namespace Wirehome.WindowsService.Services
{
    public class ProcessService : IProcessService
    {
        public void StartProcess(string path, bool restoreWhenRunning = true)
        {
            if(!File.Exists(path)) throw new Exception($"Process {path} cannot be found on PC");

            if(restoreWhenRunning)
            {
                var process = GetActiveProcess(Path.GetFileNameWithoutExtension(path));
                if(process.Any())
                {
                    BringProcessToFront(process.FirstOrDefault());
                    return;
                }
            }
            Process.Start(path);
        }

        public void StopProcess(string name)
        {
            var active = GetActiveProcess(Path.GetFileNameWithoutExtension(name));

            if(active.Any())
            {
                Process.GetProcessById(active.FirstOrDefault().PID)?.Kill();
            }
        }

        public bool IsProcessStarted(string processName)
        {
            return GetActiveProcess(Path.GetFileNameWithoutExtension(processName)).Any();
        }
     
        private IEnumerable<ProcessDetails> GetActiveProcess(string processIndicator)
        {
            var activeProcess = Process.GetProcesses().Select(x => new ProcessDetails
                                                                   {
                                                                       ProcessName = x.ProcessName,
                                                                       PID = x.Id,
                                                                       MainWindowHandle = x.MainWindowHandle
                                                                   }
            ).ToList();

            var list = new List<ProcessDetails>();
            foreach (var item in activeProcess)
            {
                if (item.ProcessName.IndexOf(processIndicator, 0, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    list.Add(item);
                }
            }
            return list;
        }

        private static void BringProcessToFront(ProcessDetails process)
        {
            const int SW_RESTORE = 9;

            IntPtr handle = process.MainWindowHandle;
            if (Win32Api.IsIconic(handle))
            {
                Win32Api.ShowWindow(handle, SW_RESTORE);
            }

            Win32Api.SetForegroundWindow(handle);
        }
    }
}
