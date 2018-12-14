using AutoMapper;
using AutoMapper.Configuration;
using HomeCenter.Broker;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Contracts;
using HomeCenter.Model.Core;
using HomeCenter.Services.Controllers;
using HomeCenter.Services.Devices;
using HomeCenter.Services.DI;
using HomeCenter.Services.Logging;
using HomeCenter.Services.Quartz;
using HomeCenter.Services.Roslyn;
using HomeCenter.Utils;
using HomeCenter.Utils.LogProviders;
using Microsoft.Extensions.Logging;
using Proto;
using Quartz;
using Quartz.Spi;
using SimpleInjector;
using SimpleInjector.Diagnostics;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Services.Bootstrapper
{
    public class Bootstrapper
    {
        public Bootstrapper(Container container)
        {
            _container = container;
            _container.Options.AllowOverridingRegistrations = true;
        }

        protected Container _container;

        public async Task<PID> BuildController()
        {
            RegisterConfiguration();

            RegisterNativeServices();

            RegisterBaseDependencies();

            RegisterLogging();

            RegisterAutomapper();

            await RegisterQuartz().ConfigureAwait(false);

            RegisterActorProxies();

            _container.Verify();

            return CreateController();
        }

        protected virtual void RegisterConfiguration()
        {
            _container.RegisterInstance(new StartupConfiguration { ConfigurationLocation = "componentConfiguration.json" });
        }

        private void RegisterActorProxies()
        {
            foreach (var actorProxy in AssemblyHelper.GetTypesWithAttribute<ProxyClassAttribute>())
            {
                var registration = Lifestyle.Transient.CreateRegistration(actorProxy, _container);

                _container.AddRegistration(actorProxy, registration);

                registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "Disposed by app");

                _container.Register(actorProxy);
            }
        }

        private PID CreateController()
        {
            var actorFactory = _container.GetInstance<IActorFactory>();
            return actorFactory.GetActor<Controller>(nameof(Controller));
        }

        protected virtual void RegisterBaseDependencies()
        {
            var actorRegistry = new ActorPropsRegistry();

            _container.RegisterInstance(actorRegistry);
            _container.RegisterSingleton<IServiceProvider, SimpleInjectorServiceProvider>();
            _container.RegisterSingleton<IActorFactory, ActorFactory>();
            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterSingleton<IActorMessageBroker, ActorMessageBroker>();
            _container.RegisterSingleton<IRoslynCompilerService, RoslynCompilerService>();
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

        protected virtual ILoggerProvider[] GetLogProviders()
        {
            return new ILoggerProvider[] { new ConsoleLogProvider() };
        }

        protected void RegisterLogging()
        {
            var loggerOptions = new LoggerFilterOptions { MinLevel = LogLevel.Debug };
            var loggerFactory = new LoggerFactory(GetLogProviders(), loggerOptions);

            Log.SetLoggerFactory(loggerFactory);

            _container.RegisterInstance<ILoggerFactory>(loggerFactory);
            _container.Register(typeof(ILogger<>), typeof(GenericLogger<>), Lifestyle.Singleton);
        }

        protected virtual void RegisterNativeServices()
        {
            _container.RegisterSingleton<II2cBus, I2cBus>();
            _container.RegisterSingleton<ISerialDevice, SerialDevice>();
            _container.RegisterSingleton<IGpioDevice, GpioDevice>();
        }

        public void Dispose() => _container.Dispose();

        protected void RegisterUnhandledExceptions()
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogException(e.ToString());
        }

        public void LogException(string message)
        {
            var logger = _container.GetInstance<ILogger<string>>();
            logger.LogError(message);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogException(e.ToString());
        }
    }
}