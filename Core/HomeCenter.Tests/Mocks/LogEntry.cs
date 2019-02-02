using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace HomeCenter.Tests.ComponentModel
{

    public class LogEntry
    {
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; }
    }
}