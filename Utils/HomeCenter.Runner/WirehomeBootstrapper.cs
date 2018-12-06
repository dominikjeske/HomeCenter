using HomeCenter.Model.Native;
using HomeCenter.Services.Bootstrapper;
using HomeCenter.Services.Configuration;
using HomeCenter.Services.Controllers;
using HomeCenter.Services.DI;
using HomeCenter.Services.Logging;
using HomeCenter.Utils.LogProviders;
using Microsoft.Extensions.Logging;
using Moq;
using Proto;
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
            var transferResult = new I2cTransferResult() { Status = I2cTransferStatus.FullTransfer, BytesTransferred = 0 };
            var i2cBus = Mock.Of<II2cBus>();
            var i2cNativeDevice = Mock.Of<II2cDevice>();

            Mock.Get(i2cBus).Setup(s => s.CreateDevice(It.IsAny<string>(), It.IsAny<int>())).Returns(i2cNativeDevice);
            Mock.Get(i2cNativeDevice).Setup(s => s.WritePartial(It.IsAny<byte[]>())).Returns(transferResult);
            Mock.Get(i2cNativeDevice).Setup(s => s.ReadPartial(It.IsAny<byte[]>())).Returns(transferResult);

            _container.RegisterInstance(i2cBus);

            _container.RegisterInstance(Mock.Of<IGpioController>());
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

        protected override void RegisterLogging()
        {
            var loggerFactory = new LoggerFactory()
                                  .AddDebug();

            loggerFactory.AddProvider(new ConsoleLogProvider());

            Log.SetLoggerFactory(loggerFactory);

            _container.RegisterInstance(loggerFactory);
            _container.Register(typeof(ILogger<>), typeof(GenericLogger<>), Lifestyle.Singleton);
        }
    }
}