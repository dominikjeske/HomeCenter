using System;

namespace Wirehome.Core.Services.Logging
{
    public interface ILogger
    {
        void Verbose(string message);

        void Info(string message);

        void Warning(string message);

        void Warning(Exception exception, string message);

        void Error(string message);

        void Error(Exception exception, string message);

        void Publish(LogEntrySeverity severity, string message, Exception exception);
    }
}
