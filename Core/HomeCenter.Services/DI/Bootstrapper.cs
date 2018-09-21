using AutoMapper;
using AutoMapper.Configuration;
using HomeCenter.ComponentModel.Adapters;
using HomeCenter.ComponentModel.Configuration;
using HomeCenter.Core.Quartz;
using HomeCenter.Core.Services;
using HomeCenter.Core.Services.I2C;
using HomeCenter.Core.Services.Roslyn;
using HomeCenter.Core.Utils;
using HomeCenter.Messaging;
using HomeCenter.Model.Core;
using HomeCenter.Model.Extensions;
using HomeCenter.Services.DI;
using HomeCenter.Services.Networking;
using Microsoft.Extensions.Logging;
using Proto;
using Quartz;
using Quartz.Spi;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Services.Configuration
{
    public abstract class Bootstrapper : IBootstrapper
    {
        protected Container _container = new Container();

        public async Task<PID> BuildController()
        {
            RegisterControllerOptions();

            RegisterNativeServices();

            RegisterBaseDependencies();

            RegisterLogging();

            RegisterAutomapper();

            await RegisterQuartz().ConfigureAwait(false);

            RegisterServices();

            _container.Verify();

            return CreateController();
        }

        private PID CreateController()
        {
            var actorFactory = _container.GetInstance<IActorFactory>();
            return actorFactory.GetActor<Controller>();
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
            _container.RegisterSingleton<IAdapterServiceFactory, AdapterServiceFactory>();
            _container.RegisterSingleton<IRoslynCompilerService, RoslynCompilerService>();
        }

        protected virtual void RegisterServices()
        {
            var services = new List<Registration>
            {
                _container.RegisterService<ISerialMessagingService, SerialMessagingService>(),
                _container.RegisterService<II2CBusService, I2CBusService>(),
                _container.RegisterService<IHttpServerService, HttpServerService>()
            };

            _container.Collection.Register<IService>(services);
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

            Proto.Log.SetLoggerFactory(loggerFactory);

            _container.RegisterInstance(loggerFactory);
            _container.Register(typeof(ILogger<>), typeof(GenericLogger<>), Lifestyle.Singleton);
        }

        private class GenericLogger<T> : ILogger<T>
        {
            private readonly ILogger<T> _underlying;

            public GenericLogger(ILoggerFactory factory)
            {
                _underlying = factory.CreateLogger<T>();
            }

            public IDisposable BeginScope<TState>(TState state) => _underlying.BeginScope(state);

            public bool IsEnabled(LogLevel logLevel) => _underlying.IsEnabled(logLevel);

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                    => _underlying.Log(logLevel, eventId, state, exception, formatter);
        }

        protected abstract void RegisterNativeServices();

        protected abstract void RegisterControllerOptions();

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}