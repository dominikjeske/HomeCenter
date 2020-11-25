using Raven.Client.Documents;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using System;

namespace HomeCenter.Storage.RavenDB
{
    /// <summary>
    /// Adds the WriteTo.RavenDB() extension method to <see cref="LoggerConfiguration"/>.
    /// </summary>
    public static class LoggerConfigurationRavenDBExtensions
    {
        /// <summary>
        /// Adds a sink that writes log events as documents to a RavenDB database.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="documentStore">A documentstore for a RavenDB database.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="defaultDatabase">Optional default database</param>
        /// <param name="expiration">Optional time before a logged message will be expired assuming the expiration bundle is installed. <see cref="System.Threading.Timeout.InfiniteTimeSpan">Timeout.InfiniteTimeSpan</see> (-00:00:00.0010000) means no expiration. If this is not provided but errorExpiration is, errorExpiration will be used for non-errors too.</param>
        /// <param name="errorExpiration">Optional time before a logged error message will be expired assuming the expiration bundle is installed.  <see cref="System.Threading.Timeout.InfiniteTimeSpan">Timeout.InfiniteTimeSpan</see> (-00:00:00.0010000) means no expiration. If this is not provided but expiration is, expiration will be used for errors too.</param>
        /// <param name="logExpirationCallback">Optional callback to dynamically determine log expiration based on event properties.  <see cref="System.Threading.Timeout.InfiniteTimeSpan">Timeout.InfiniteTimeSpan</see> (-00:00:00.0010000) means no expiration. If this is provided, it will be used instead of expiration or errorExpiration.</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration RavenDB(
            this LoggerSinkConfiguration loggerConfiguration,
            IDocumentStore documentStore,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            int batchPostingLimit = RavenDBSink.DefaultBatchPostingLimit,
            TimeSpan? period = null,
            IFormatProvider? formatProvider = null,
            string? defaultDatabase = null,
            TimeSpan? expiration = null,
            TimeSpan? errorExpiration = null,
            Func<LogEvent, TimeSpan>? logExpirationCallback = null)
        {
            if (loggerConfiguration == null) throw new ArgumentNullException(nameof(loggerConfiguration));
            if (documentStore == null) throw new ArgumentNullException(nameof(documentStore));


            var defaultedPeriod = period ?? RavenDBSink.DefaultPeriod;
            return loggerConfiguration.Sink(
                new RavenDBSink(documentStore, batchPostingLimit, defaultedPeriod, formatProvider, defaultDatabase, expiration, errorExpiration, logExpirationCallback),
                restrictedToMinimumLevel);
        }
    }
}