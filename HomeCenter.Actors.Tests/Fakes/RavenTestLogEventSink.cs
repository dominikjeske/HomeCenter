using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Serilog.Core;
using Serilog.Events;
using System;

namespace HomeCenter.Storage.RavenDB
{
    internal class RavenTestLogEventSink : ILogEventSink, IDisposable
    {
        private readonly IDocumentStore _documentStore;
        private readonly LogKeyProvider _logKeyProvider;
        private readonly RavenSerializer _ravenSerializer;

        private IDocumentSession _documentSession;

        public RavenTestLogEventSink(IDocumentStore documentStore, LogKeyProvider logKeyProvider, RavenSerializer ravenSerializer)
        {
            _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            _logKeyProvider = logKeyProvider;
            _ravenSerializer = ravenSerializer;
            
            _documentSession = _documentStore.OpenSession();
        }

        public void Dispose()
        {
            _documentSession.SaveChanges();
            _documentSession.Dispose();
        }

        public void Emit(LogEvent logEvent)
        {
            var data = _ravenSerializer.Serialize(logEvent);
            _documentSession.Store(data, _logKeyProvider.GetKey());
        }
    }
}