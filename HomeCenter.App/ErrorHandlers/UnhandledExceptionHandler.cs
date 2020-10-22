using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HomeCenter.App
{
    internal class UnhandledExceptionHandler : IDisposable
    {
        private readonly ILogger<UnhandledExceptionHandler> _logger;

        public UnhandledExceptionHandler(ILogger<UnhandledExceptionHandler> logger)
        {
            _logger = logger;
        }

        public void Register()
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        public void Dispose()
        {
            TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogException(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                LogException(exception);
            }
        }

        private void LogException(Exception e)
        {
            _logger.LogCritical(e, "Unhandled exception in application");
        }
    }
}