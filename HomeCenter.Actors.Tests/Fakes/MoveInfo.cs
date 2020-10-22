using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace HomeCenter.Actors.Tests.Fakes
{
    internal class MoveInfo
    {
        public string Time { get; set; }

        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public string Message { get; set; }

        public LogLevel LogLevel { get; set; }

        public EventId EventId { get; set; }

        public Exception Exception { get; set; }

        public string Template { get; set; }
    }
}