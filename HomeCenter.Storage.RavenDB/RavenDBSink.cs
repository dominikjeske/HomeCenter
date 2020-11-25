using Raven.Client.Documents;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.Storage.RavenDB
{
    public class RavenDBSink : PeriodicBatchingSink
    {
        private readonly IFormatProvider? _formatProvider;
        private readonly IDocumentStore _documentStore;
        private readonly string? _defaultDatabase;
        private readonly TimeSpan? _expiration;
        private readonly TimeSpan? _errorExpiration;
        private readonly Func<LogEvent, TimeSpan>? _logExpirationCallback;
        private readonly bool _disposeDocumentStore;

        /// <summary>
        /// A reasonable default for the number of events posted in
        /// each batch.
        /// </summary>
        public const int DefaultBatchPostingLimit = 50;

        /// <summary>
        /// A reasonable default time to wait between checking for event batches.
        /// </summary>
        public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Construct a sink posting to the specified database.
        /// </summary>
        /// <param name="documentStore">A documentstore for a RavenDB database.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="defaultDatabase">Optional name of default database</param>
        /// <param name="expiration">Optional time before a logged message will be expired assuming the expiration bundle is installed. <see cref="System.Threading.Timeout.InfiniteTimeSpan">Timeout.InfiniteTimeSpan</see> (-00:00:00.0010000) means no expiration. If this is not provided but errorExpiration is, errorExpiration will be used for non-errors too.</param>
        /// <param name="errorExpiration">Optional time before a logged error message will be expired assuming the expiration bundle is installed. <see cref="System.Threading.Timeout.InfiniteTimeSpan">Timeout.InfiniteTimeSpan</see> (-00:00:00.0010000) means no expiration. If this is not provided but expiration is, expiration will be used for errors too.</param>
        /// <param name="logExpirationCallback">Optional callback to dynamically determine log expiration based on event properties.  <see cref="System.Threading.Timeout.InfiniteTimeSpan">Timeout.InfiniteTimeSpan</see> (-00:00:00.0010000) means no expiration. If this is provided, it will be used instead of expiration or errorExpiration.</param>
        public RavenDBSink(IDocumentStore documentStore, int batchPostingLimit, TimeSpan period, IFormatProvider? formatProvider, string? defaultDatabase = null,
            TimeSpan? expiration = null, TimeSpan? errorExpiration = null, Func<LogEvent, TimeSpan>? logExpirationCallback = null)
            : base(batchPostingLimit, period)
        {
            if (documentStore == null) throw new ArgumentNullException(nameof(documentStore));
            _formatProvider = formatProvider;
            _documentStore = documentStore;
            _defaultDatabase = defaultDatabase;
            _expiration = expiration;
            _errorExpiration = errorExpiration ?? expiration;
            _expiration = expiration ?? errorExpiration;
            _logExpirationCallback = logExpirationCallback;
            _disposeDocumentStore = false;
        }

        /// <summary>
        /// Emit a batch of log events, running asynchronously.
        /// </summary>
        /// <param name="events">The events to emit.</param>
        /// <remarks>Override either <see cref="PeriodicBatchingSink.EmitBatch"/> or <see cref="PeriodicBatchingSink.EmitBatchAsync"/>,
        /// not both.</remarks>
        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            using var session = string.IsNullOrWhiteSpace(_defaultDatabase) ? _documentStore.OpenAsyncSession() : _documentStore.OpenAsyncSession(_defaultDatabase);
            foreach (var logEvent in events)
            {
                var logEventDoc = new LogEntry(logEvent, logEvent.RenderMessage(_formatProvider));
                await session.StoreAsync(logEventDoc);

                var expiration =
                    _logExpirationCallback != null ? _logExpirationCallback(logEvent) :
                    _expiration == null ? Timeout.InfiniteTimeSpan :
                    logEvent.Level == LogEventLevel.Error || logEvent.Level == LogEventLevel.Fatal ? _errorExpiration.GetValueOrDefault() : _expiration.Value;

                if (expiration != Timeout.InfiniteTimeSpan)
                {
                    var metaData = session.Advanced.GetMetadataFor(logEventDoc);
                    metaData[Raven.Client.Constants.Documents.Metadata.Expires] = DateTime.UtcNow.Add(expiration);
                }
            }
            await session.SaveChangesAsync();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && _disposeDocumentStore)
            {
                _documentStore.Dispose();
            }
        }
    }
}