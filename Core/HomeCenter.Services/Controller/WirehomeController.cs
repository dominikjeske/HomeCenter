using AutoMapper;
using AutoMapper.Configuration;
using CSharpFunctionalExtensions;
using HTTPnet.Core.Pipeline;
using Newtonsoft.Json;
using Quartz;
using Quartz.Spi;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using HomeCenter.ComponentModel.Adapters;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.Components;
using HomeCenter.ComponentModel.Configuration;
using HomeCenter.Core.ComponentModel.Configuration;
using HomeCenter.Core.EventAggregator;
using HomeCenter.Core.Services;
using HomeCenter.Core.Services.DependencyInjection;
using HomeCenter.Core.Services.I2C;
using HomeCenter.Core.Services.Logging;
using HomeCenter.Core.Services.Quartz;
using HomeCenter.Core.Services.Roslyn;
using HomeCenter.Core.Utils;
using HomeCenter.Model.Extensions;
using HomeCenter.Services.Networking;

namespace HomeCenter.Model.Core
{
    public class HomeCenterController : Actor
    {
        private readonly IContainer _container;
        private readonly ControllerOptions _options;

        private ILogger _log;
        private IConfigurationService _confService;
        private IResourceLocatorService _resourceLocator;
        private IRoslynCompilerService _roslynCompilerService;
        private ISchedulerFactory _schedulerFactory;
        private IHttpServerService _httpServerService;
        private IEventAggregator _eventAggregator;
        private IMapper _mapper;

        private HomeCenterConfiguration _homeConfiguration;

        public HomeCenterController(ControllerOptions options)
        {
            _container = new HomeCenterContainer();
            _options = options;
        }

        public override async Task Initialize()
        {
            try
            {
                await base.Initialize().ConfigureAwait(false);

                RegisterServices();
                RegisterRestCommandHanler();

                LoadDynamicAdapters(_options.AdapterMode);

                await LoadCalendars().ConfigureAwait(false);
                await InitializeServices().ConfigureAwait(false);
                await InitializeConfiguration().ConfigureAwait(false);
                await RunScheduler().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _log.Error(e, "Unhanded exception while application startup");
                throw;
            }
        }

        private void RegisterRestCommandHanler()
        {
            _httpServerService.AddRequestHandler(new RestCommandHandler(_eventAggregator, _mapper));

            if (_options.HttpServerPort.HasValue)
            {
                _httpServerService.UpdateServerPort(_options.HttpServerPort.Value);
            }
        }

        private async Task LoadCalendars()
        {
            var calendars = AssemblyHelper.GetProjectAssemblies()
                                 .SelectMany(s => s.GetTypes())
                                 .Where(p => typeof(ICalendar).IsAssignableFrom(p));

            var scheduler = await _schedulerFactory.GetScheduler().ConfigureAwait(false);

            foreach (var calendarType in calendars)
            {
                var cal = (ICalendar)calendarType.GetConstructors().FirstOrDefault()?.Invoke(null);
                await scheduler.AddCalendar(calendarType.Name, cal, false, false).ConfigureAwait(false);
            }
        }

        private async Task RunScheduler()
        {
            var scheduler = await _schedulerFactory.GetScheduler().ConfigureAwait(false);
            await scheduler.Start(_disposables.Token).ConfigureAwait(false);
        }

        private void LoadDynamicAdapters(AdapterMode adapterMode)
        {
            _log.Info($"Loading adapters in mode: {adapterMode}");

            if (adapterMode == AdapterMode.Compiled)
            {
                var result = _roslynCompilerService.CompileAssemblies(_resourceLocator.GetRepositoyLocation());
                var veryfy = Result.Combine(result.ToArray());
                if (veryfy.IsFailure) throw new Exception($"Error while compiling adapters: {veryfy.Error}");

                foreach (var adapter in result)
                {
                    Assembly.LoadFrom(adapter.Value);
                }
            }
            else
            {
                _log.Info($"Using only build in adapters");
            }
        }

        private void RegisterServices()
        {
            _container.RegisterSingleton(() => _container);

            RegisterBaseServices();

            RegisterNativeServices();

            GetServicesFromContainer();

            _container.Verify();
        }

        private void GetServicesFromContainer()
        {
            _log = _container.GetInstance<ILogService>().CreatePublisher(nameof(HomeCenterController));
            _confService = _container.GetInstance<IConfigurationService>();
            _resourceLocator = _container.GetInstance<IResourceLocatorService>();
            _roslynCompilerService = _container.GetInstance<IRoslynCompilerService>();
            _schedulerFactory = _container.GetInstance<ISchedulerFactory>();
            _httpServerService = _container.GetInstance<IHttpServerService>();
            _eventAggregator = _container.GetInstance<IEventAggregator>();
            _mapper = _container.GetInstance<IMapper>();
        }

        private void RegisterNativeServices()
        {
            if (_options.NativeServicesRegistration == null) throw new Exception("Missing native services registration");

            _options.NativeServicesRegistration?.Invoke(_container);
        }

        private void RegisterBaseServices()
        {
            (_options.BaseServicesRegistration ?? RegisterBaseServices).Invoke(_container);
        }

        private void RegisterBaseServices(IContainer container)
        {
            container.RegisterCollection(_options.Loggers);

            container.RegisterSingleton<IEventAggregator, EventAggregator>();
            container.RegisterSingleton<IConfigurationService, ConfigurationService>();
            container.RegisterSingleton<IResourceLocatorService, ResourceLocatorService>();
            container.RegisterSingleton<IAdapterServiceFactory, AdapterServiceFactory>();
            container.RegisterSingleton<IRoslynCompilerService, RoslynCompilerService>();

            container.RegisterService<ISerialMessagingService, SerialMessagingService>();
            container.RegisterService<II2CBusService, I2CBusService>();
            container.RegisterService<ILogService, LogService>();
            container.RegisterService<IHttpServerService, HttpServerService>(100);

            //Quartz
            container.RegisterSingleton<IJobFactory, SimpleInjectorJobFactory>();
            container.RegisterSingleton<ISchedulerFactory, SimpleInjectorSchedulerFactory>();
            container.RegisterSingleton(() => container.GetInstance<ISchedulerFactory>().GetScheduler().Result);

            //Auto mapper
            container.RegisterSingleton(GetMapper);
        }

        public IMapper GetMapper()
        {
            var mce = new MapperConfigurationExpression();
            mce.ConstructServicesUsing(_container.GetInstance);
            mce.AddProfile(new HomeCenterMappingProfile());

            return new Mapper(new MapperConfiguration(mce), t => _container.GetInstance(t));
        }

        private async Task InitializeConfiguration()
        {
            _homeConfiguration = _confService.ReadConfiguration(_options.AdapterMode);

            foreach (var adapter in _homeConfiguration.Adapters)
            {
                try
                {
                    await adapter.Initialize().ConfigureAwait(false);
                    _disposables.Add(adapter);
                }
                catch (Exception e)
                {
                    _log.Error(e, $"Exception while initialization of adapter {adapter.Uid}");
                }
            }

            foreach (var component in _homeConfiguration.Components)
            {
                try
                {
                    await component.Initialize().ConfigureAwait(false);
                    _disposables.Add(component);
                }
                catch (Exception e)
                {
                    _log.Error(e, $"Exception while initialization of component {component.Uid}");
                }
            }
        }

        private async Task InitializeServices()
        {
            var services = _container.GetSerives();

            while (services.Count > 0)
            {
                var service = services.Dequeue();
                try
                {
                    await service.Initialize().ConfigureAwait(false);
                    _disposables.Add(service);
                }
                catch (Exception exception)
                {
                    _log.Error(exception, $"Error while starting service '{service.GetType().Name}'. " + exception.Message);
                }
            }
        }

        protected override void LogException(Exception ex) => _log.Error(ex, $"Unhanded controller exception");

        protected Component GetComponentCommandHandler(Command command)
        {
            if (string.IsNullOrWhiteSpace(command.Uid)) throw new ArgumentException($"Command GetComponentCommand is missing destination uid");

            return _homeConfiguration.Components.FirstOrDefault(c => c.Uid == command.Uid);
        }
    }

    public class RestCommandHandler : IHttpContextPipelineHandler
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IMapper _mapper;

        public RestCommandHandler(IEventAggregator eventAggregator, IMapper mapper)
        {
            _eventAggregator = eventAggregator;
            _mapper = mapper;
        }

        public async Task ProcessRequestAsync(HttpContextPipelineHandlerContext context)
        {
            if (context.HttpContext.Request.Method.Equals(HttpMethod.Post.Method) && context.HttpContext.Request.Uri.Equals("/api"))
            {
                using (var reader = new StreamReader(context.HttpContext.Request.Body))
                {
                    var rawCommandString = await reader.ReadToEndAsync().ConfigureAwait(false);
                    var result = JsonConvert.DeserializeObject<CommandDTO>(rawCommandString);

                    var command = _mapper.Map<Command>(result);
                    await _eventAggregator.PublishDeviceCommnd(command).ConfigureAwait(false);
                }

                //context.HttpContext.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes(s.ToUpperInvariant()));
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK; // OK is also default

                context.BreakPipeline = true;
                return;
            }

            return;
        }

        public Task ProcessResponseAsync(HttpContextPipelineHandlerContext context)
        {
            return Task.FromResult(0);
        }
    }
}