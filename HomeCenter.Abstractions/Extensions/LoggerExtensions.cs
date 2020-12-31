using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace HomeCenter.Abstractions.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogDeviceEvent(this ILogger logger, string Uid, EventId eventId, string template = "", params object[] arguments)
        {
            if (string.IsNullOrEmpty(template))
            {
                template = eventId.Name;
            }

            template = "[{Uid}] " + template;
            var args = new List<object>(arguments);
            args.Insert(0, Uid);

            logger.LogInformation(eventId, template, args.ToArray());
        }
    }
}