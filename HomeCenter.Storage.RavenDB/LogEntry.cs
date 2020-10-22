using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HomeCenter.Storage.RavenDB
{
    /// <summary>
    /// A wrapper class for <see cref="LogEvent"/> that is used to store as a document in RavenDB
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Construct a new <see cref="LogEntry"/>.
        /// </summary>
        public LogEntry()
        {
        }

        /// <summary>
        /// Construct a new <see cref="LogEntry"/>.
        /// </summary>
        public LogEntry(LogEvent logEvent, string renderedMessage)
        {
            Timestamp = logEvent.Timestamp;
            MessageTemplate = logEvent.MessageTemplate.Text;
            Level = logEvent.Level;
            RenderedMessage = renderedMessage;

            if (logEvent.Exception != null)
            {
                Exception = RavenPropertyFormatter.Simplify(logEvent.Properties.FirstOrDefault().Value);
            }
            else
            {
                Properties = new Dictionary<string, object>();
                foreach (var pair in logEvent.Properties)
                {
                    Properties.Add(pair.Key, RavenPropertyFormatter.Simplify(pair.Value));
                }
            }
        }

        /// <summary>
        /// The time at which the event occurred.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The template that was used for the log message.
        /// </summary>
        public string MessageTemplate { get; set; }

        /// <summary>
        /// The level of the log.
        /// </summary>
        public LogEventLevel Level { get; set; }

        /// <summary>
        /// The rendered log message.
        /// </summary>
        public string RenderedMessage { get; set; }

        /// <summary>
        /// Exception
        /// </summary>
        public object Exception { get; set; }

        /// <summary>
        /// Properties associated with the event, including those presented in <see cref="Serilog.Events.MessageTemplate"/>.
        /// </summary>
        public IDictionary<string, object> Properties { get; set; }
    }
}