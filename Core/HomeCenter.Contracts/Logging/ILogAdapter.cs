using System;

namespace HomeCenter.Core.Services.Logging
{
    public interface ILogAdapter : IDisposable
    {
        void ProcessLogEntry(LogEntry logEntry);
    }
}
