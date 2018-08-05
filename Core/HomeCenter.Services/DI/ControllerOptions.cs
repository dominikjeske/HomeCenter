using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeCenter.Core.Services.Logging;
using SimpleInjector;

namespace HomeCenter.Core.Services.DependencyInjection
{
    public class ControllerOptions
    {
        public Action<Container> NativeServicesRegistration { get; set; }
        public Func<Container, Task> BaseServicesRegistration { get; set; }
        public AdapterMode AdapterMode { get; set; }
        public IEnumerable<ILogAdapter> Loggers { get; set; }
        public int? HttpServerPort { get; set; }
    }
}