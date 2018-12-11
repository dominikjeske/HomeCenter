using HomeCenter.Model.Devices;
using HomeCenter.Services.Bootstrapper;
using HomeCenter.Services.Controllers;
using Moq;
using SimpleInjector;

namespace HomeCenter.Runner
{
    public class WirehomeBootstrapper : Bootstrapper
    {
        private readonly string _configuration;

        public WirehomeBootstrapper(Container container, string configuration) : base(container)
        {
            _configuration = configuration;
            _container.Options.AllowOverridingRegistrations = true;
        }

        public Container Container => _container;

        protected override void RegisterNativeServices()
        {
            var i2cBus = Mock.Of<II2cBus>();

            _container.RegisterInstance(i2cBus);
            _container.RegisterInstance(Mock.Of<ISerialDevice>());
        }

        protected override void RegisterControllerOptions()
        {
            _container.RegisterInstance(new ControllerOptions
            {
                AdapterMode = "Embedded",
                RemoteActorPort = 8000,
                Configuration = _configuration
            });
        }
    }
}