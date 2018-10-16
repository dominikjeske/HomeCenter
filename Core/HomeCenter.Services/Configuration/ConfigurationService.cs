using AutoMapper;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Areas;
using HomeCenter.Model.Components;
using HomeCenter.Model.Core;
using HomeCenter.Model.Exceptions;
using HomeCenter.Services.Configuration.DTO;
using HomeCenter.Utils;
using HomeCenter.Utils.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Proto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HomeCenter.Services.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ConfigurationService> _logger;
        private readonly IResourceLocatorService _resourceLocatorService;
        private readonly IActorFactory _actorFactory;
        private readonly IServiceProvider _serviceProvider;

        public ConfigurationService(IMapper mapper, ILogger<ConfigurationService> logger, IResourceLocatorService resourceLocatorService, IActorFactory actorFactory, IServiceProvider serviceProvider)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _resourceLocatorService = resourceLocatorService;
            _logger = logger;
            _actorFactory = actorFactory;
            _serviceProvider = serviceProvider;
        }

        public HomeCenterConfiguration ReadConfiguration(AdapterMode adapterMode)
        {
            var configPath = _resourceLocatorService.GetConfigurationPath();

            var rawConfig = File.ReadAllText(configPath);

            var result = JsonConvert.DeserializeObject<HomeCenterConfigDTO>(rawConfig);

            CheckForDuplicateUid(result);

            var types = RegisterTypesInAutomapper(adapterMode);

            var adapters = CreataActors<AdapterDTO, Adapter>(result.HomeCenter.Adapters, types[typeof(AdapterDTO)]);
            var components = MapComponents(result);
            var areas = MapAreas(result, components);
            var services = CreataActors<ServiceDTO, Service>(result.HomeCenter.Services, types[typeof(ServiceDTO)]);

            var configuration = new HomeCenterConfiguration
            {
                Adapters = adapters,
                Components = components,
                Areas = areas,
                Services = services
            };

            return configuration;
        }

        private void CheckForDuplicateUid(HomeCenterConfigDTO configuration)
        {
            var allUids = configuration.HomeCenter.Adapters.Select(a => a.Uid).ToList();
            allUids.AddRange(configuration.HomeCenter.Components.Select(c => c.Uid));

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
                    var actorType = types.Find(t => t.Name == $"{actorConfig.Type}Proxy");
                    if (actorType == null) throw new MissingTypeException($"Could not find adapter {actorType}");
                    var adapter = _actorFactory.GetActor(() => (Q)Mapper.Map(actorConfig, typeof(T), actorType), actorConfig.Uid);

                    actors.Add(actorConfig.Uid, adapter);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception {ex.Message} while initializing adapter {actorConfig.Type}");
                }
            }
            return actors;
        }

        private Dictionary<Type, List<Type>> RegisterTypesInAutomapper(AdapterMode adapterMode)
        {
            Dictionary<Type, List<Type>> types = new Dictionary<Type, List<Type>>();

            // force to load HomeCenter.ActorsContainer into memory
            if (adapterMode == AdapterMode.Embedded)
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
    }
}