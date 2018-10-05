using HomeCenter.Model.Native;
using HomeCenter.Services.Configuration;
using HomeCenter.Services.DI;
using Moq;
using SimpleInjector;

namespace HomeCenter.TestRunner
{
    public class WirehomeBootstrapper : Bootstrapper
    {
        private readonly string _configuration;

        public WirehomeBootstrapper(string configuration)
        {
            _configuration = configuration;

            _container.Options.AllowOverridingRegistrations = true;
        }

        public Container Container => _container;

        protected override void RegisterBaseDependencies()
        {
            base.RegisterBaseDependencies();

            var resourceLocator = Mock.Of<IResourceLocatorService>();
            Mock.Get(resourceLocator).Setup(x => x.GetRepositoyLocation()).Returns(@"..\..\..\..\..\Adapters\AdaptersContainer\Adapters");
            Mock.Get(resourceLocator).Setup(x => x.GetConfigurationPath()).Returns($@"..\..\..\..\..\Configurations\{_configuration}.json");
            _container.RegisterInstance(resourceLocator);
        }

        protected override void RegisterNativeServices()
        {
            var transferResult = new I2cTransferResult() { Status = I2cTransferStatus.FullTransfer, BytesTransferred = 0 };
            var i2cBus = Mock.Of<II2cBus>();
            var i2cNativeDevice = Mock.Of<II2cDevice>();
            var i2c = Mock.Of<II2CBusService>();
            Mock.Get(i2cBus).Setup(s => s.CreateDevice(It.IsAny<string>(), It.IsAny<int>())).Returns(i2cNativeDevice);
            Mock.Get(i2cNativeDevice).Setup(s => s.WritePartial(It.IsAny<byte[]>())).Returns(transferResult);
            Mock.Get(i2cNativeDevice).Setup(s => s.ReadPartial(It.IsAny<byte[]>())).Returns(transferResult);

            _container.RegisterInstance(i2cBus);
            _container.RegisterInstance(i2c);
            _container.RegisterInstance(Mock.Of<IGpioController>());
            _container.RegisterInstance(Mock.Of<ISerialDevice>());
            _container.RegisterInstance(Mock.Of<ISoundPlayer>());
            _container.RegisterInstance(Mock.Of<IStorage>());
        }

        protected override void RegisterControllerOptions()
        {
            _container.RegisterInstance<IControllerOptions>(new ControllerOptions
            {
                AdapterMode = AdapterMode.Embedded,
                HttpServerPort = 8080,
            });
        }
    }
}