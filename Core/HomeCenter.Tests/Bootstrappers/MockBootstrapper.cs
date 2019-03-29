using HomeCenter.Model.Contracts;
using HomeCenter.Services.Bootstrapper;
using HomeCenter.Services.Controllers;
using Microsoft.Extensions.Logging;
using Moq;
using SimpleInjector;
using System.IO;

namespace HomeCenter.Tests.ComponentModel
{
    public class MockBootstrapper : Bootstrapper
    {
        private readonly string _configuration;
        public LogMock Logs { get; } = new LogMock();

        public MockBootstrapper(Container container, string configuration) : base(container)
        {
            _configuration = configuration;
        }

        public Container Container => _container;

        protected override void RegisterNativeServices()
        {
            var i2cBus = Mock.Of<II2cBus>();

            _container.RegisterInstance(i2cBus);
            _container.RegisterInstance(Mock.Of<ISerialDevice>());
            _container.RegisterInstance(Mock.Of<IGpioDevice>());
        }

        protected override void RegisterConfiguration()
        {
            _container.RegisterInstance(new StartupConfiguration
            {
                ConfigurationLocation = Path.Combine(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\")), $@"Configurations\{_configuration}")
            });
        }

        protected override ILoggerProvider[] GetLogProviders()
        {
            _container.RegisterInstance<ILoggerProvider>(Logs);

            return new ILoggerProvider[] { Logs };
        }
    }
}