using System;

namespace Wirehome.Core.Services.Logging
{
    public interface ILogAdapter : IDisposable
    {
        void ProcessLogEntry(LogEntry logEntry);
    }
}
