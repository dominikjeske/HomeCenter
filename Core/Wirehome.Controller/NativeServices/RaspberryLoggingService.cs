using System;
using Windows.Foundation.Diagnostics;
using Wirehome.Core.Services.Logging;

namespace Wirehome.Raspberry
{
    internal class RaspberryLoggingService : ILogAdapter
    {
        private readonly LoggingChannel _loggingChannel;

        public RaspberryLoggingService() => _loggingChannel = new LoggingChannel("Wirehome", null, new Guid("4bd2826e-54a1-4ba9-bf63-92b73ea1ac4a"));

        public void ProcessLogEntry(LogEntry logEntry)
        {
            var fields = new LoggingFields();

            fields.AddString("Source", logEntry.Source ?? "");
            fields.AddString("Exception", logEntry.Exception ?? "");
            
            _loggingChannel.LogEvent(logEntry.Message, fields, MapSeverity(logEntry.Severity));
        }

        private static LoggingLevel MapSeverity(LogEntrySeverity severity)
        {
            if (severity == LogEntrySeverity.Error)
            {
                return LoggingLevel.Error;
            }
            else if (severity == LogEntrySeverity.Verbose)
            {
                return LoggingLevel.Verbose;
            }
            else if (severity == LogEntrySeverity.Warning)
            {
                return LoggingLevel.Warning;
            }

            return LoggingLevel.Information;
        }

        public void Dispose()
        {
            _loggingChannel.Dispose();
        }
    }
}
