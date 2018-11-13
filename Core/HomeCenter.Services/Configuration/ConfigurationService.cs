using AutoMapper;
using CSharpFunctionalExtensions;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Areas;
using HomeCenter.Model.Components;
using HomeCenter.Model.Core;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events.Service;
using HomeCenter.Services.Configuration.DTO;
using HomeCenter.Services.Roslyn;
using HomeCenter.Utils;
using HomeCenter.Utils.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Proto;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HomeCenter.Services.Configuration
{
    [ProxyCodeGenerator]
    public abstract class ConfigurationService : Service
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ConfigurationService> _logger;
        private readonly IResourceLocatorService _resourceLocatorService;
        private readonly IActorFactory _actorFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRoslynCompilerService _roslynCompilerService;

        public ConfigurationService(IMapper mapper, ILogger<ConfigurationService> logger, IResourceLocatorService resourceLocatorService, IActorFactory actorFactory,
                                    IServiceProvider serviceProvider, IRoslynCompilerService roslynCompilerService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _resourceLocatorService = resourceLocatorService;
            _logger = logger;
            _actorFactory = actorFactory;
            _serviceProvider = serviceProvider;
            _roslynCompilerService = roslynCompilerService;
        }

        protected async Task Handle(StartSystemCommand startFromConfigCommand)
        {
            var configPath = _resourceLocatorService.GetConfigurationPath();

            var rawConfig = File.ReadAllText(configPath);

            var result = JsonConvert.DeserializeObject<HomeCenterConfigDTO>(rawConfig);

            await LoadCalendars().ConfigureAwait(false);

            LoadDynamicAdapters(startFromConfigCommand.AdapterMode);

            CheckForDuplicateUid(result);

            var types = RegisterTypesInAutomapper(startFromConfigCommand.AdapterMode);

            var services = CreataActors<ServiceDTO, Service>(result.HomeCenter.Services, types[typeof(ServiceDTO)]);
            var adapters = CreataActors<AdapterDTO, Adapter>(result.HomeCenter.Adapters, types[typeof(AdapterDTO)]);
            var components = MapComponents(result);
            var areas = MapAreas(result, components);

            MessageBroker.Send(SystemStartedEvent.Default, "Controller");
        }

        private void CheckForDuplicateUid(HomeCenterConfigDTO configuration)
        {
            var allUids = configuration.HomeCenter.Adapters.Select(a => a.Uid).ToList();
            allUids.AddRange(configuration.HomeCenter.Components.Select(c => c.Uid));
            allUids.AddRange(configuration.HomeCenter.Services.Select(c => c.Uid));

            var duplicateKeys = allUids.GroupBy(x => x)
                                       .Where(group => group.Count() > 1)
                                       .Select(group => group.Key);
            if (duplicateKeys?.Count() > 0)
            {
                throw new ConfigurationException($"Duplicate UID's found in config file: {string.Join(", ", duplicateKeys)}");
            }
        }

        private IList<Area> MapAreas(HomeCenterConfigDTO result, IDictionary<string, PID> components)
        {
            var areas = _mapper.Map<IList<AreaDTO>, IList<Area>>(result.HomeCenter.Areas);
            MapComponentsToArea(result.HomeCenter.Areas, components, areas);

            return areas;
        }

        private void MapComponentsToArea(IList<AreaDTO> areasFromConfig, IDictionary<string, PID> components, IList<Area> areas)
        {
            var configAreas = areasFromConfig.Expand(a => a.Areas);
            foreach (var area in areas.Expand(a => a.Areas))
            {
                var areInConfig = configAreas.FirstOrDefault(a => a.Uid == area.Uid);
                if (areInConfig?.Components != null)
                {
                    foreach (var component in areInConfig?.Components)
                    {
                        //area.AddComponent(component.Uid, components[component.Uid]);
                    }
                }
            }
        }

        private IDictionary<string, PID> MapComponents(HomeCenterConfigDTO result)
        {
            var components = new Dictionary<string, PID>();
            List<ComponentProxy> comp = new List<ComponentProxy>();

            foreach (var componentConfig in result.HomeCenter.Components)
            {
                var localConfigCopy = componentConfig; // prevents override of componentConfig when executed in multi thread

                var component = _actorFactory.GetActor(() => _mapper.Map<ComponentProxy>(localConfigCopy), localConfigCopy.Uid);
                components.Add(localConfigCopy.Uid, component);
            }

            return components;
        }

        private Dictionary<string, PID> CreataActors<T, Q>(IEnumerable<T> config, List<Type> types) where T : BaseDTO
                                                                                                    where Q : IActor
        {
            Dictionary<string, PID> actors = new Dictionary<string, PID>();
            foreach (var actorConfig in config)
            {
                try
                {
                    var routing = GetRouting(actorConfig);

                    var actorType = types.Find(t => t.Name == $"{actorConfig.Type}Proxy");
                    if (actorType == null) throw new MissingTypeException($"Could not find type for actor {actorType}");
                    var actor = _actorFactory.GetActor(() => (Q)Mapper.Map(actorConfig, typeof(T), actorType), actorConfig.Uid, routing: routing);

                    actors.Add(actorConfig.Uid, actor);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception {ex.Message} while initializing actor {actorConfig.Type}");
                }
            }
            return actors;
        }

        private int GetRouting<T>(T actorConfig) where T : BaseDTO
        {
            if (actorConfig?.Properties?.ContainsKey("Routing") ?? false)
            {
                return int.Parse(actorConfig.Properties["Routing"]);
            }

            return 0;
        }

        private Dictionary<Type, List<Type>> RegisterTypesInAutomapper(string adapterMode)
        {
            Dictionary<Type, List<Type>> types = new Dictionary<Type, List<Type>>();

            // force to load HomeCenter.ActorsContainer into memory
            if (adapterMode == "Embedded")
            {
                var testAdapter = typeof(Actors.ForceAssemblyLoadType);
            }

            types[typeof(AdapterDTO)] = new List<Type>(AssemblyHelper.GetAllTypes<Adapter>(true));
            types[typeof(ServiceDTO)] = new List<Type>(AssemblyHelper.GetAllTypes<Service>(true));

            Mapper.Initialize(p =>
            {
                foreach (var type in types.Keys)
                {
                    foreach (var actorType in types[type])
                    {
                        p.CreateMap(type, actorType).ConstructUsingServiceLocator();
                    }
                }

                p.ShouldMapProperty = propInfo => (propInfo.CanWrite && propInfo.GetGetMethod(true).IsPublic) || propInfo.IsDefined(typeof(MapAttribute), false);
                p.ConstructServicesUsing(_serviceProvider.GetService);
            });
            return types;
        }

        //TODO move to separate service?
        private void LoadDynamicAdapters(string adapterMode)
        {
            Logger.LogInformation($"Loading adapters in mode: {adapterMode}");

            if (adapterMode == "Compiled")
            {
                var result = _roslynCompilerService.CompileAssemblies(_resourceLocatorService.GetRepositoyLocation());
                var veryfy = Result.Combine(result.ToArray());
                if (veryfy.IsFailure) throw new CompilationException($"Error while compiling adapters: {veryfy.Error}");

                foreach (var adapter in result)
                {
                    Assembly.LoadFrom(adapter.Value);
                }
            }
            else
            {
                Logger.LogInformation($"Using only build in adapters");
            }
        }

        private async Task LoadCalendars()
        {
            foreach (var calendarType in AssemblyHelper.GetAllTypes<ICalendar>())
            {
                var cal = calendarType.CreateInstance<ICalendar>();
                await Scheduler.AddCalendar(calendarType.Name, cal, false, false).ConfigureAwait(false);
            }
        }
    }
}