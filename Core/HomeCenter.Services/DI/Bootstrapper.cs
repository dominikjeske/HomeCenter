using AutoMapper;
using AutoMapper.Configuration;
using HomeCenter.Broker;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Core;
using HomeCenter.Services.Configuration;
using HomeCenter.Services.Controllers;
using HomeCenter.Services.Quartz;
using HomeCenter.Services.Roslyn;
using HomeCenter.Utils;
using Microsoft.Extensions.Logging;
using Proto;
using Quartz;
using Quartz.Spi;
using SimpleInjector;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Services.DI
{
    public abstract class Bootstrapper : IBootstrapper
    {
        public Bootstrapper(Container container)
        {
            _container = container;
        }

        protected Container _container;

        public async Task<PID> BuildController()
        {
            RegisterControllerOptions();

            RegisterNativeServices();

            RegisterBaseDependencies();

            RegisterLogging();

            RegisterAutomapper();

            await RegisterQuartz().ConfigureAwait(false);

            RegisterActorProxies();

            _container.Verify();

            return CreateController();
        }

        private void RegisterActorProxies()
        {
            foreach (var actorProxy in AssemblyHelper.GetTypesWithAttribute<ProxyClassAttribute>())
            {
                var registration = Lifestyle.Singleton.CreateRegistration(actorProxy, _container);
                _container.AddRegistration(actorProxy, registration);
            }
        }

        private PID CreateController()
        {
            var actorManager = _container.GetInstance<IActorFactory>();
            return actorManager.GetActor<Controller>();
        }

        protected virtual void RegisterBaseDependencies()
        {
            var actorRegistry = new ActorPropsRegistry();

            _container.RegisterInstance(actorRegistry);
            _container.RegisterSingleton<IServiceProvider, SimpleInjectorServiceProvider>();
            _container.RegisterSingleton<IActorFactory, ActorFactory>();

            //TODO add some special configuration for props per actor type on actorRegistry

            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterSingleton<IConfigurationService, ConfigurationService>();
            _container.RegisterSingleton<IResourceLocatorService, ResourceLocatorService>();
            _container.RegisterSingleton<IRoslynCompilerService, RoslynCompilerService>();
            _container.RegisterSingleton<IHttpServerService, HttpServerService>();
        }

        private async Task RegisterQuartz()
        {
            var jobFactory = new SimpleInjectorJobFactory(_container);
            var jobSchedulerFactory = new SimpleInjectorSchedulerFactory(jobFactory);
            var scheduler = await jobSchedulerFactory.GetScheduler().ConfigureAwait(false);

            _container.RegisterInstance<IJobFactory>(jobFactory);
            _container.RegisterInstance<ISchedulerFactory>(jobSchedulerFactory);
            _container.RegisterInstance(scheduler);
        }

        protected virtual void RegisterAutomapper()
        {
            var mce = new MapperConfigurationExpression();
            mce.ConstructServicesUsing(_container.GetInstance);

            foreach (var profile in AssemblyHelper.GetAllTypes<Profile>())
            {
                mce.AddProfile(profile);
            }

            var mapper = new Mapper(new MapperConfiguration(mce), t => _container.GetInstance(t));

            _container.RegisterInstance<IMapper>(mapper);
        }

        protected virtual void RegisterLogging()
        {
            var loggerFactory = new LoggerFactory()
                                   .AddDebug(LogLevel.Information) //TODO configure
                                   .AddEventSourceLogger()
                                   .AddConsole();

            Log.SetLoggerFactory(loggerFactory);

            _container.RegisterInstance(loggerFactory);
            _container.Register(typeof(ILogger<>), typeof(GenericLogger<>), Lifestyle.Singleton);
        }

        protected abstract void RegisterNativeServices();

        protected abstract void RegisterControllerOptions();

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}