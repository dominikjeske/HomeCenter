using Microsoft.Extensions.Logging;

namespace HomeCenter.Tests.ComponentModel
{
    public class LogEntry
    {
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; }

        public override string ToString() => $"{LogLevel}: {Message}";
    }
}