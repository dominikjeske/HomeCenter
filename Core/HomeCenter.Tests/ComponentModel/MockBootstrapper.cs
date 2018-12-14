using HomeCenter.Broker;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Contracts;
using HomeCenter.Services.Bootstrapper;
using HomeCenter.Services.Controllers;
using HomeCenter.Services.DI;
using HomeCenter.Services.Roslyn;
using Moq;
using SimpleInjector;
using System;
using System.IO;

namespace HomeCenter.Tests.ComponentModel
{
    public class MockBootstrapper : Bootstrapper
    {
        private readonly string _repositoryPath;
        private readonly string _configuration;

        public MockBootstrapper(Container container, string repositoryPath, string configuration) : base(container)
        {
            _repositoryPath = repositoryPath;
            _configuration = configuration;
        }

        public Container Container => _container;

        protected override void RegisterBaseDependencies()
        {
            var actorRegistry = new ActorPropsRegistry();

            _container.RegisterInstance(actorRegistry);
            _container.RegisterSingleton<IServiceProvider, SimpleInjectorServiceProvider>();
            _container.RegisterSingleton<IActorFactory, ActorFactory>();
            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterSingleton<IRoslynCompilerService, RoslynCompilerService>();
        }

        protected override void RegisterNativeServices()
        {
            var i2cBus = Mock.Of<II2cBus>();

            _container.RegisterInstance(i2cBus);
            _container.RegisterInstance(Mock.Of<ISerialDevice>());
            _container.RegisterInstance(Mock.Of<ISoundPlayer>());
        }

        protected override void RegisterControllerOptions()
        {
            _container.RegisterInstance(new ControllerOptions
            {
                AdapterMode = "Embedded",
                RemoteActorPort = 8080,
                Configuration = Path.Combine(Directory.GetCurrentDirectory(), $@"ComponentModel\SampleConfigs\{_configuration}.json"),
                AdapterRepoName = _repositoryPath ?? Path.Combine(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\..")), @"Actors\ActorsContainer\Adapters")
            });
        }
    }
}