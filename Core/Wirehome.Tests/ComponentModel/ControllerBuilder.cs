using AutoMapper;
using AutoMapper.Configuration;
using Moq;
using Quartz;
using Quartz.Spi;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using HomeCenter.ComponentModel.Adapters;
using HomeCenter.ComponentModel.Configuration;
using HomeCenter.Core.EventAggregator;
using HomeCenter.Core.Interface.Native;
using HomeCenter.Core.Services;
using HomeCenter.Core.Services.DependencyInjection;
using HomeCenter.Core.Services.I2C;
using HomeCenter.Core.Services.Logging;
using HomeCenter.Core.Services.Quartz;
using HomeCenter.Core.Services.Roslyn;
using HomeCenter.Model.Core;
using HomeCenter.Services.Networking;

namespace HomeCenter.Core.Tests.ComponentModel
{
    public class ControllerBuilder
    {
        private IContainer _container;
        private string _configuration;
        private string _repositoryPath;

        private ControllerOptions GetControllerOptions()
        {
            return new ControllerOptions
            {
                NativeServicesRegistration = RegisterRaspberryServices,
                BaseServicesRegistration = RegisterContainerServices,
                AdapterMode = AdapterMode.Embedded,
                HttpServerPort = 8080
            };
        }

        public ControllerBuilder WithConfiguration(string configuration)
        {
            _configuration = configuration;
            return this;
        }

        public ControllerBuilder WithAdapterRepositoryPath(string repositoryPath)
        {
            _repositoryPath = repositoryPath;
            return this;
        }

        public HomeCenterController Build()
        {
            return new HomeCenterController(GetControllerOptions());
        }

        public async Task<(HomeCenterController controller, IContainer container)> BuildAndRun()
        {
            var controller = Build();
            await controller.Initialize().ConfigureAwait(false);
            
            return (controller, _container);
        }

        private void RegisterRaspberryServices(IContainer container)
        {
            var transferResult = new NativeI2cTransferResult() { Status = NativeI2cTransferStatus.FullTransfer, BytesTransferred = 0 };
            var i2cBus = Mock.Of<INativeI2cBus>();
            var i2cNativeDevice = Mock.Of<INativeI2cDevice>();
            Mock.Get(i2cBus).Setup(s => s.CreateDevice(It.IsAny<string>(), It.IsAny<int>())).Returns(i2cNativeDevice);
            Mock.Get(i2cNativeDevice).Setup(s => s.WritePartial(It.IsAny<byte[]>())).Returns(transferResult);
            Mock.Get(i2cNativeDevice).Setup(s => s.ReadPartial(It.IsAny<byte[]>())).Returns(transferResult);

            container.RegisterInstance(i2cBus);
            container.RegisterSingleton(Mock.Of<INativeGpioController>());
            container.RegisterSingleton(Mock.Of<INativeSerialDevice>());
            container.RegisterSingleton(Mock.Of<INativeSoundPlayer>());
            container.RegisterSingleton(Mock.Of<INativeStorage>());
            container.RegisterSingleton(Mock.Of<INativeTimerSerice>());
        }

        private void RegisterContainerServices(IContainer container)
        {
            _container = container;

            var logService = Mock.Of<ILogService>();
            var logger = Mock.Of<ILogger>();
            var resourceLocator = Mock.Of<IResourceLocatorService>();

            //@"W:\Projects\HA4IoT\Adapters\AdaptersContainer\bin\Debug\netstandard2.0
            //var adaptersRepo = Path.Combine(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\..")), @"Adapters\AdaptersContainer\bin\Debug\netstandard2.0");
            var adaptersRepo = _repositoryPath ?? Path.Combine(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\..")), @"Adapters\AdaptersContainer\Adapters");
            var configFile = Path.Combine(Directory.GetCurrentDirectory(), $@"ComponentModel\SampleConfigs\{_configuration}.json");
            Mock.Get(resourceLocator).Setup(x => x.GetRepositoyLocation()).Returns(adaptersRepo);
            Mock.Get(resourceLocator).Setup(x => x.GetConfigurationPath()).Returns(configFile);
            

            Mock.Get(logService).Setup(x => x.CreatePublisher(It.IsAny<string>())).Returns(logger);
            Mock.Get(logger).Setup(x => x.Error(It.IsAny<string>())).Callback<string>(message => Debug.WriteLine($"Error: {message}"));
            Mock.Get(logger).Setup(x => x.Error(It.IsAny<Exception>(), It.IsAny<string>())).Callback<Exception, string>((exception, message) => Debug.WriteLine($"Error: {message} | Details: {exception}"));

            container.RegisterInstance(logService);
            container.RegisterInstance(resourceLocator);
            
            container.RegisterInstance(Mock.Of<ISerialMessagingService>());
            container.RegisterSingleton<IEventAggregator, EventAggregator.EventAggregator>();
            container.RegisterSingleton<IConfigurationService, ConfigurationService>();
            container.RegisterSingleton<II2CBusService, I2CBusService>();
            container.RegisterSingleton<IAdapterServiceFactory, AdapterServiceFactory>();
            container.RegisterSingleton<IHttpMessagingService, HttpMessagingService>();
            container.RegisterSingleton<IRoslynCompilerService, RoslynCompilerService>();
            container.RegisterService<IHttpServerService, HttpServerService>(100);

            //Quartz
            container.RegisterSingleton<IJobFactory, SimpleInjectorJobFactory>();
            container.RegisterSingleton<ISchedulerFactory, SimpleInjectorSchedulerFactory>();

            //Auto mapper
            container.RegisterSingleton(GetMapper);
        }

        public IMapper GetMapper()
        {
            var mce = new MapperConfigurationExpression();
            mce.ConstructServicesUsing(_container.GetInstance);

            var profile = new HomeCenterMappingProfile();
            mce.AddProfile(profile);

            return new Mapper(new MapperConfiguration(mce), t => _container.GetInstance(t));
        }
    }
}