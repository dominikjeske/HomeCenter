using AutoMapper;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Areas;
using HomeCenter.Model.Components;
using HomeCenter.Model.Core;
using HomeCenter.Model.Exceptions;
using HomeCenter.Services.Configuration.DTO;
using HomeCenter.Services.DI;
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
        private readonly IAdapterServiceFactory _adapterServiceFactory;
        private readonly ILogger<ConfigurationService> _logger;
        private readonly IResourceLocatorService _resourceLocatorService;
        private readonly IActorFactory _actorFactory;
        private readonly IServiceProvider _serviceProvider;

        public ConfigurationService(IMapper mapper, IAdapterServiceFactory adapterServiceFactory, ILogger<ConfigurationService> logger,
            IResourceLocatorService resourceLocatorService, IActorFactory actorFactory, IServiceProvider serviceProvider)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _adapterServiceFactory = adapterServiceFactory ?? throw new ArgumentNullException(nameof(adapterServiceFactory));
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

            var adapters = MapAdapters(result.HomeCenter.Adapters, adapterMode);
            var components = MapComponents(result, adapters);
            var areas = MapAreas(result, components);

            var configuration = new HomeCenterConfiguration
            {
                Adapters = adapters,
                Components = components,
                Areas = areas
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
                        area.AddComponent(component.Uid, components[component.Uid]);
                    }
                }
            }
        }

        private IDictionary<string, PID> MapComponents(HomeCenterConfigDTO result, IDictionary<string, PID> adapters)
        {
            var components = new Dictionary<string, PID>();

            foreach (var componentConfig in result.HomeCenter.Components)
            {
               // fill adapter actor reference
               componentConfig.Adapters.ForEach(a => a.ID = adapters[a.Uid]);

               var component = _actorFactory.GetActor(() => (Component)_mapper.Map(componentConfig, typeof(ComponentDTO), typeof(Component)), componentConfig.Uid);
                components.Add(componentConfig.Uid, component);
            }

            return components;
        }

        private IDictionary<string, PID> MapAdapters(IList<AdapterDTO> adapterConfigs, AdapterMode adapterMode)
        {
            var adapters = new Dictionary<string, PID>();

            // force to load HomeCenter.AdaptersContainer into memory
            if (adapterMode == AdapterMode.Embedded)
            {
                var testAdapter = typeof(AdaptersContainer.ForceAssemblyLoadType);
            }

            var types = new List<Type>(AssemblyHelper.GetAllTypes<Adapter>(true));

            Mapper.Initialize(p =>
            {
                foreach (var adapterType in types)
                {
                    p.CreateMap(typeof(AdapterDTO), adapterType).ConstructUsingServiceLocator();
                }

                p.ShouldMapProperty = propInfo => (propInfo.CanWrite && propInfo.GetGetMethod(true).IsPublic) || propInfo.IsDefined(typeof(MapAttribute), false);
                p.ConstructServicesUsing(_serviceProvider.GetService);
            });

            foreach (var adapterConfig in adapterConfigs)
            {
                try
                {
                    var adapterType = types.Find(t => t.Name == $"{adapterConfig.Type}Proxy");
                    if (adapterType == null) throw new MissingAdapterException($"Could not find adapter {adapterType}");
                    var adapter = _actorFactory.GetActor(() => (Adapter)Mapper.Map(adapterConfig, typeof(AdapterDTO), adapterType), adapterConfig.Uid);

                    adapters.Add(adapterConfig.Uid, adapter);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception {ex.Message} while initializing adapter {adapterConfig.Type}");
                }
            }

            return adapters;
        }
    }


}