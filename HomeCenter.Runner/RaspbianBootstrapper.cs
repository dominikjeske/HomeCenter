using HomeCenter.Services.Bootstrapper;
using HomeCenter.Utils.LogProviders;
using Microsoft.Extensions.Logging;
using SimpleInjector;

namespace HomeCenter.Runner
{
    public class RaspbianBootstrapper : Bootstrapper
    {
        public RaspbianBootstrapper(Container container) : base(container)
        {
            _container.Options.AllowOverridingRegistrations = true;
        }

        protected override ILoggerProvider[] GetLogProviders()
        {
            return new ILoggerProvider[] { new ConsoleLogProvider() };
        }
    }
}