using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace HomeCenter.Services.MotionService.Tests
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