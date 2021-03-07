using Serilog.Events;
using System.Collections.Generic;

namespace HomeCenter.Storage.RavenDB
{
    internal class RavenSerializer
    {
        public IDictionary<string, object> Serialize(LogEvent logEvent)
        {
            var dictionary = new Dictionary<string, object>
                {
                    { "ts", logEvent.Timestamp },
                    { "mt", logEvent.MessageTemplate.Text },
                    { "lvl", logEvent.Level }
                };

            foreach (var pair in logEvent.Properties)
            {
                dictionary.Add(pair.Key, RavenPropertyFormatter.Simplify(pair.Value));
            }

            return dictionary;
        }
    }
}