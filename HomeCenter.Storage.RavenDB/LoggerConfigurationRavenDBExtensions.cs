using Raven.Client.Documents;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using System;

namespace HomeCenter.Storage.RavenDB
{
    public static class LoggerConfigurationRavenDBExtensions
    {
        public static LoggerConfiguration UsePeriodicRavenDB(
            this LoggerSinkConfiguration loggerConfiguration,
            IDocumentStore documentStore,
            PeriodicBatchingSinkOptions batchingOptions,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string? defaultDatabase = null,
            TimeSpan? expiration = null)
        {
            if (loggerConfiguration == null) throw new ArgumentNullException(nameof(loggerConfiguration));
            if (documentStore == null) throw new ArgumentNullException(nameof(documentStore));

            var batchingSink = new PeriodicBatchingSink(new RavenBatchedLogEventSink(documentStore, new LogKeyProvider(), new RavenSerializer(), defaultDatabase, expiration), batchingOptions);

            return loggerConfiguration.Sink(batchingSink, restrictedToMinimumLevel);
        }

    }
}