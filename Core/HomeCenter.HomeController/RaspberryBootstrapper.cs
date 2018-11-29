using HomeCenter.HomeController.NativeServices;
using HomeCenter.Model.Native;
using HomeCenter.Services.Bootstrapper;
using HomeCenter.Services.Controllers;
using HomeCenter.Services.Logging;
using HomeCenter.Utils.LogProviders;
using Microsoft.Extensions.Logging;
using Moq;
using Proto;
using SimpleInjector;

namespace HomeCenter.HomeController
{
    internal class RaspberryBootstrapper : Bootstrapper
    {
        internal RaspberryBootstrapper(Container container) : base(container)
        {
            _container.Options.AllowOverridingRegistrations = true;

        }

        //protected override void RegisterNativeServices()
        //{
        //    _container.RegisterSingleton<IGpioController, RaspberryGpioController>();
        //    _container.RegisterSingleton<II2cBus, RaspberryI2cBus>();
        //    _container.RegisterSingleton<ISerialDevice, RaspberrySerialDevice>();
        //    _container.RegisterSingleton<ISoundPlayer, RaspberrySoundPlayer>();
        //    _container.RegisterSingleton<IStorage, RaspberryStorage>();
        //}

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
            _container.RegisterInstance(Mock.Of<ISoundPlayer>());
            _container.RegisterSingleton<IStorage, RaspberryStorage>();
        }

        protected override void RegisterControllerOptions()
        {
            _container.RegisterInstance(new ControllerOptions
            {
                AdapterMode = "Embedded",
                Configuration = "componentConfiguration.json"
            });
        }

        protected override void RegisterLogging()
        {
            var loggerFactory = new LoggerFactory()
                                    .AddDebug(LogLevel.Information); //TODO configure

            loggerFactory.AddProvider(new ConsoleLogProvider());
            loggerFactory.AddProvider(new EtwLogProvider());

            Log.SetLoggerFactory(loggerFactory);

            _container.RegisterInstance(loggerFactory);
            _container.Register(typeof(ILogger<>), typeof(GenericLogger<>), Lifestyle.Singleton);
        }
    }
}