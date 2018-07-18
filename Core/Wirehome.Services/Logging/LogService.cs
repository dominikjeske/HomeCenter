using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Wirehome.Core.Services.Logging
{
    public sealed class LogService : ILogService
    {
        private long _id;

        public ILogger CreatePublisher(string source)
        {
            return new LogServicePublisher(source, this);
        }

        public void Dispose()
        {
        }

        public LogService(IEnumerable<ILogAdapter> adapters)
        {
            _adapters = adapters;
        }

        private readonly IEnumerable<ILogAdapter> _adapters;

        public Task Initialize()
        {
            return Task.CompletedTask;
        }

        public void Publish(LogEntrySeverity severity, string source, string message, Exception exception)
        {
            var id = Interlocked.Increment(ref _id);

            var logEntry = new LogEntry(id, SystemTime.Now.UtcDateTime, Environment.CurrentManagedThreadId, severity, source ?? "System", message, exception?.ToString());
            foreach (var adapter in _adapters)
            {
                adapter.ProcessLogEntry(logEntry);
            }

            if (Debugger.IsAttached)
            {
                Debug.WriteLine($"[{logEntry.Severity}] [{logEntry.Source}] [{logEntry.ThreadId}]: {logEntry.Message}");
            }
        }
    }
}