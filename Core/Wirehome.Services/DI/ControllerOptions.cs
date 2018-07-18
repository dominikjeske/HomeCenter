using System;
using System.Collections.Generic;
using Wirehome.Core.Services.Logging;

namespace Wirehome.Core.Services.DependencyInjection
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