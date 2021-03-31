using Raven.Client.Documents;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Concurrent;

namespace HomeCenter.Storage.RavenDB
{
    internal class RavenTestLogEventSink : ILogEventSink, IDisposable
    {
        private readonly IDocumentStore _documentStore;
        private readonly LogKeyProvider _logKeyProvider;
        private readonly RavenSerializer _ravenSerializer;
        private readonly ConcurrentDictionary<string, LogEvent> _logs = new();

        public RavenTestLogEventSink(IDocumentStore documentStore, LogKeyProvider logKeyProvider, RavenSerializer ravenSerializer)
        {
            _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            _logKeyProvider = logKeyProvider;
            _ravenSerializer = ravenSerializer;
        }

        public void Dispose()
        {
            using var bulkInsert = _documentStore.BulkInsert();
            foreach (var log in _logs)
            {
                var data = _ravenSerializer.Serialize(log.Value);
                bulkInsert.Store(data, log.Key);
            }
        }

        public void Emit(LogEvent logEvent)
        {
            var key = _logKeyProvider.GetKey();
            _logs.TryAdd(key, logEvent);
        }
    }
}