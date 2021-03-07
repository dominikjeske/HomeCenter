using Raven.Client.Documents;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Storage.RavenDB
{
    internal class RavenBatchedLogEventSink : IBatchedLogEventSink, IDisposable
    {
        private readonly IDocumentStore _documentStore;
        private readonly LogKeyProvider _logKeyProvider;
        private readonly RavenSerializer _ravenSerializer;
        private readonly string? _defaultDatabase;
        private readonly TimeSpan? _expiration;

        public RavenBatchedLogEventSink(IDocumentStore documentStore, LogKeyProvider logKeyProvider, RavenSerializer ravenSerializer, string? defaultDatabase = null, TimeSpan? expiration = null)
        {
            _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            _logKeyProvider = logKeyProvider;
            _ravenSerializer = ravenSerializer;
            _defaultDatabase = defaultDatabase;
            _expiration = expiration;
        }

        public void Dispose() => _documentStore?.Dispose();

        async Task IBatchedLogEventSink.EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            using var session = string.IsNullOrWhiteSpace(_defaultDatabase) ? _documentStore.OpenAsyncSession() : _documentStore.OpenAsyncSession(_defaultDatabase);
            foreach (var logEvent in events)
            {
                var data = _ravenSerializer.Serialize(logEvent);
                await session.StoreAsync(data, _logKeyProvider.GetKey());
                session.SetExpirationDate(_expiration, data);
            }
            await session.SaveChangesAsync();
        }

        public Task OnEmptyBatchAsync() => Task.CompletedTask;
    }
}