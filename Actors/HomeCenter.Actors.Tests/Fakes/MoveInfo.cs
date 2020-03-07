using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;

namespace HomeCenter.Services.MotionService.Tests
{

    internal class MoveInfo
    {
        public DateTimeOffset Time { get; set; }

        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public string Message { get; set; }

        public LogLevel LogLevel { get; set; }

        public EventId EventId { get; set; }

        public Exception Exception { get; set; }

        public string Template { get; set; }
    }
}