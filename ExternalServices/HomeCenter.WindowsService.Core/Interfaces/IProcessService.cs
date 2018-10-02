namespace HomeCenter.WindowsService.Core.Interfaces
{
    public interface IProcessService
    {
        bool IsProcessStarted(string processName);

        void StartProcess(string path, bool restoreWhenRunning = true);

        void StopProcess(string name);
    }
}