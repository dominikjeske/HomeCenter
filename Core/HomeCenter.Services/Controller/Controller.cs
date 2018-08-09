using AutoMapper;
using CSharpFunctionalExtensions;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.Components;
using HomeCenter.ComponentModel.Configuration;
using HomeCenter.Core;
using HomeCenter.Core.ComponentModel.Configuration;
using HomeCenter.Core.EventAggregator;
using HomeCenter.Core.Services.DependencyInjection;
using HomeCenter.Core.Services.Roslyn;
using HomeCenter.Core.Utils;
using HomeCenter.Model.Extensions;
using HomeCenter.Services.Networking;
using HTTPnet.Core.Pipeline;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace HomeCenter.Model.Core
{
    public class Controller : Actor
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IScheduler _scheduler;
        private readonly IRoslynCompilerService _roslynCompilerService;
        private readonly IControllerOptions _controllerOptions;
        private readonly ILogger<Controller> _logger;
        private readonly IConfigurationService _configurationService;
        private readonly IResourceLocatorService _resourceLocatorService;
        private readonly IMapper _mapper;
        private readonly IHttpServerService _httpServerService;

        private HomeCenterConfiguration _homeConfiguration = null;
        private readonly IEnumerable<IService> _services;

        public Controller(IEventAggregator eventAggregator, IMapper mapper, IHttpServerService httpServerService, IScheduler scheduler, IRoslynCompilerService roslynCompilerService,
            IResourceLocatorService resourceLocatorService, IConfigurationService configurationService, ILogger<Controller> logger, IControllerOptions controllerOptions, IEnumerable<IService> services)
        {
            _eventAggregator = eventAggregator;
            _scheduler = scheduler;
            _roslynCompilerService = roslynCompilerService;
            _controllerOptions = controllerOptions;
            _logger = logger;
            _configurationService = configurationService;
            _resourceLocatorService = resourceLocatorService;
            _mapper = mapper;
            _httpServerService = httpServerService;
            _services = services;
        }

        public override async Task Initialize()
        {
            try
            {
                await base.Initialize().ConfigureAwait(false);

                RegisterRestCommandHanler();
                LoadDynamicAdapters(_controllerOptions.AdapterMode);

                await LoadCalendars().ConfigureAwait(false);
                await InitializeConfiguration().ConfigureAwait(false);
                await InitializeServices().ConfigureAwait(false);
                await RunScheduler().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhanded exception while application startup");
                throw;
            }
        }

        private void RegisterRestCommandHanler()
        {
            _httpServerService.AddRequestHandler(new RestCommandHandler(_eventAggregator, _mapper));

            if (_controllerOptions.HttpServerPort.HasValue)
            {
                _httpServerService.UpdateServerPort(_controllerOptions.HttpServerPort.Value);
            }
        }

        private async Task LoadCalendars()
        {
            foreach (var calendarType in AssemblyHelper.GetAllTypes<ICalendar>())
            {
                var cal = calendarType.CreateInstance<ICalendar>();
                await _scheduler.AddCalendar(calendarType.Name, cal, false, false).ConfigureAwait(false);
            }
        }

        private Task RunScheduler() => _scheduler.Start(_disposables.Token);

        private void LoadDynamicAdapters(AdapterMode adapterMode)
        {
            _logger.LogInformation($"Loading adapters in mode: {adapterMode}");

            if (adapterMode == AdapterMode.Compiled)
            {
                var result = _roslynCompilerService.CompileAssemblies(_resourceLocatorService.GetRepositoyLocation());
                var veryfy = Result.Combine(result.ToArray());
                if (veryfy.IsFailure) throw new Exception($"Error while compiling adapters: {veryfy.Error}");

                foreach (var adapter in result)
                {
                    Assembly.LoadFrom(adapter.Value);
                }
            }
            else
            {
                _logger.LogInformation($"Using only build in adapters");
            }
        }

        private async Task InitializeServices()
        {
            foreach (var service in _services)
            {
                try
                {
                    await service.Initialize().ConfigureAwait(false);
                    _disposables.Add(service);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, $"Error while starting service '{service.GetType().Name}'. " + exception.Message);
                }
            }
        }

        private async Task InitializeConfiguration()
        {
            _homeConfiguration = _configurationService.ReadConfiguration(_controllerOptions.AdapterMode);

            foreach (var adapter in _homeConfiguration.Adapters)
            {
                try
                {
                    await adapter.Initialize().ConfigureAwait(false);
                    _disposables.Add(adapter);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Exception while initialization of adapter {adapter.Uid}");
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
                    _logger.LogError(e, $"Exception while initialization of component {component.Uid}");
                }
            }
        }

        protected override void LogException(Exception ex) => _logger.LogError(ex, $"Unhanded controller exception");

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