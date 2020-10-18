//using HomeCenter.Assemblies;
//using HomeCenter.Broker;
//using HomeCenter.CodeGeneration;
//using HomeCenter.Model.Actors;
//using HomeCenter.Model.Components;
//using HomeCenter.Model.Contracts;
//using HomeCenter.Model.Core;
//using HomeCenter.Quartz;
//using HomeCenter.Services.Actors;
//using HomeCenter.Services.Controllers;
//using HomeCenter.Services.Roslyn;
//using Microsoft.Extensions.Logging;
//using Proto;
//using System;
//using System.Threading.Tasks;

namespace HomeCenter.Services.Bootstrapper
{
    //public abstract class Bootstrapper
    //{
    //    public Bootstrapper(Container container)
    //    {
    //        _container = container;
    //        _container.Options.AllowOverridingRegistrations = true;
    //    }

    //    protected Container _container;

    //    public async Task<PID> BuildController()
    //    {
    //        //TODO add quartz

    //        RegisterConfiguration();

    //        RegisterNativeServices();

    //        RegisterBaseDependencies();

    //        RegisterLogging();

    //        RegisterActorProxies();

    //        _container.Verify();

    //        return CreateController();
    //    }

    //    protected virtual void RegisterConfiguration()
    //    {
    //        _container.RegisterInstance(new StartupConfiguration { ConfigurationLocation = "componentConfiguration.json" });
    //    }

    //    private void RegisterActorProxies()
    //    {
    //        _container.Collection.Register(typeof(ITypeMapper), new Type[] { typeof(ServiceMapper), typeof(AdapterMapper), typeof(AreaMapper), typeof(ComponentMapper) });
    //        _container.RegisterSingleton<DeviceActorMapper>();
    //        _container.RegisterSingleton<BaseObjectMapper>();
    //        _container.RegisterSingleton<ClassActivator>();

    //        foreach (var actorProxy in AssemblyHelper.GetTypesWithAttribute<ProxyClassAttribute>())
    //        {
    //            if (actorProxy == typeof(ComponentProxy)) continue;

    //            var registration = Lifestyle.Transient.CreateRegistration(actorProxy, _container);

    //            _container.AddRegistration(actorProxy, registration);

    //            registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "Disposed by app");

    //            _container.Register(actorProxy);
    //        }
    //    }

    //    private PID CreateController()
    //    {
    //        var actorFactory = _container.GetInstance<IActorFactory>();

    //        return actorFactory.CreateActor<Controller>(nameof(Controller));
    //    }

    //    protected virtual void RegisterBaseDependencies()
    //    {
    //        var actorRegistry = new ActorPropsRegistry();

    //        _container.RegisterInstance(actorRegistry);
    //        _container.RegisterSingleton<IActorFactory, ActorFactory>();
    //        _container.RegisterSingleton<IActorLoader, ActorLoader>();
    //        _container.RegisterSingleton<IEventAggregator, EventAggregator>();
    //        _container.RegisterSingleton<IMessageBroker, MessageBroker>();
    //        _container.RegisterSingleton<IConcurrencyProvider, ConcurrencyProvider>();

    //        _container.RegisterSingleton<IRoslynCompilerService, RoslynCompilerService>();
    //    }

        

    //    protected abstract ILoggerProvider[] GetLogProviders();

    //    protected virtual void ConfigureLoggerFactory(ILoggerFactory loggerFacory)
    //    {
    //    }

    //    protected void RegisterLogging()
    //    {
    //        var loggerOptions = new LoggerFilterOptions { MinLevel = LogLevel.Debug };
    //        var loggerFactory = new LoggerFactory(GetLogProviders(), loggerOptions);
    //        ConfigureLoggerFactory(loggerFactory);

    //        Log.SetLoggerFactory(loggerFactory);

    //        _container.RegisterInstance<ILoggerFactory>(loggerFactory);
    //        _container.Register(typeof(ILogger<>), typeof(Logger<>), Lifestyle.Singleton);
    //    }

    //    protected abstract void RegisterNativeServices();

    //    public void Dispose() => _container.Dispose();

    //    protected void RegisterUnhandledExceptions()
    //    {
    //        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    //        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    //    }

    //    private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    //    {
    //        LogException(e.ToString());
    //    }

    //    public void LogException(string message)
    //    {
    //        var logger = _container.GetInstance<ILogger<string>>();
    //        logger.LogError(message);
    //    }

    //    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    //    {
    //        LogException(e.ToString());
    //    }
    //}
}