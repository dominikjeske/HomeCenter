using HomeCenter.Model.Adapters;
using HomeCenter.Model.Configuration;
using HomeCenter.Core.Interface.Native;
using HomeCenter.Core.Services;
using HomeCenter.Core.Services.DependencyInjection;
using HomeCenter.Core.Services.I2C;
using HomeCenter.Core.Services.Roslyn;
using HomeCenter.Messaging;
using HomeCenter.Model.Core;
using HomeCenter.Model.Extensions;
using HomeCenter.Services.Configuration;
using HomeCenter.Services.DI;
using HomeCenter.Services.Networking;
using Moq;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.IO;

namespace HomeCenter.Core.Tests.ComponentModel
{
    public class MockBootstrapper : Bootstrapper
    {
        private readonly string _repositoryPath;
        private readonly string _configuration;

        public MockBootstrapper(string repositoryPath, string configuration)
        {
            _repositoryPath = repositoryPath;
            _configuration = configuration;
        }

        public Container Container => _container;

        protected override void RegisterBaseDependencies()
        {
            var resourceLocator = Mock.Of<IResourceLocatorService>();

            //@"W:\Projects\HA4IoT\Adapters\AdaptersContainer\bin\Debug\netstandard2.0
            //var adaptersRepo = Path.Combine(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\..")), @"Adapters\AdaptersContainer\bin\Debug\netstandard2.0");
            var adaptersRepo = _repositoryPath ?? Path.Combine(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\..")), @"Adapters\AdaptersContainer\Adapters");
            var configFile = Path.Combine(Directory.GetCurrentDirectory(), $@"ComponentModel\SampleConfigs\{_configuration}.json");
            Mock.Get(resourceLocator).Setup(x => x.GetRepositoyLocation()).Returns(adaptersRepo);
            Mock.Get(resourceLocator).Setup(x => x.GetConfigurationPath()).Returns(configFile);

            _container.RegisterInstance(resourceLocator);

            var actorRegistry = new ActorPropsRegistry();

            _container.RegisterInstance(actorRegistry);
            _container.RegisterSingleton<IServiceProvider, SimpleInjectorServiceProvider>();
            _container.RegisterSingleton<IActorFactory, ActorFactory>();

            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterSingleton<IRoslynCompilerService, RoslynCompilerService>();
            _container.RegisterSingleton<IAdapterServiceFactory, AdapterServiceFactory>();
            _container.RegisterSingleton<IConfigurationService, ConfigurationService>();
        }

        protected override void RegisterServices()
        {
            var services = new List<Registration>
            {
                _container.RegisterService<II2CBusService, I2CBusService>(),
                _container.RegisterService<IHttpServerService, HttpServerService>()
            };

            _container.Collection.Register<IService>(services);
        }

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