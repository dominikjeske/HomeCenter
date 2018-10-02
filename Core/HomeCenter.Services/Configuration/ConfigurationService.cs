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

            var adapters = MapAdapters(result.HomeCenter.Adapters, adapterMode);
            var components = MapComponents(result);
            var areas = MapAreas(result, components);

            var configuration = new HomeCenterConfiguration
            {
                Adapters = adapters,
                Components = components,
                Areas = areas
            };

            //TODO check before map
            //CheckForDuplicateUid(configuration);

            return configuration;
        }

        //private void CheckForDuplicateUid(HomeCenterConfiguration configuration)
        //{
        //    var allUids = configuration.Adapters.Select(a => a.Uid).ToList();
        //    allUids.AddRange(configuration.Components.Select(c => c.Uid));

        //    var duplicateKeys = allUids.GroupBy(x => x)
        //                               .Where(group => group.Count() > 1)
        //                               .Select(group => group.Key);
        //    if (duplicateKeys?.Count() > 0)
        //    {
        //        throw new ConfigurationException($"Duplicate UID's found in config file: {string.Join(", ", duplicateKeys)}");
        //    }
        //}

        private IList<Area> MapAreas(HomeCenterConfigDTO result, IList<Component> components)
        {
            var areas = _mapper.Map<IList<AreaDTO>, IList<Area>>(result.HomeCenter.Areas);
            MapComponentsToArea(result.HomeCenter.Areas, components, areas);

            return areas;
        }

        private void MapComponentsToArea(IList<AreaDTO> areasFromConfig, IList<Component> components, IList<Area> areas)
        {
            var configAreas = areasFromConfig.Expand(a => a.Areas);
            foreach (var area in areas.Expand(a => a.Areas))
            {
                var areInConfig = configAreas.FirstOrDefault(a => a.Uid == area.Uid);
                if (areInConfig?.Components != null)
                {
                    foreach (var component in areInConfig?.Components)
                    {
                        area.AddComponent(components.FirstOrDefault(c => c.Uid == component.Uid));
                    }
                }
            }
        }

        private IList<Component> MapComponents(HomeCenterConfigDTO result)
        {
            return _mapper.Map<IList<ComponentDTO>, IList<Component>>(result.HomeCenter.Components);
        }

        private IList<PID> MapAdapters(IList<AdapterDTO> adapterConfigs, AdapterMode adapterMode)
        {
            var adapters = new List<PID>();

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

                    adapters.Add(adapter);
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