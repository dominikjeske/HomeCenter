using System;
using System.Collections.Generic;
using HomeCenter.Core.Services.Logging;

namespace HomeCenter.Core.Services.DependencyInjection
{
    public class ControllerOptions
    {
        public Action<IContainer> NativeServicesRegistration { get; set; }
        public Action<IContainer> BaseServicesRegistration { get; set; }
        public AdapterMode AdapterMode { get; set; }
        public IEnumerable<ILogAdapter> Loggers { get; set; }
        public int? HttpServerPort { get; set; }
    }
}