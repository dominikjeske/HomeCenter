using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HomeCenter.ComponentModel.Adapters;
using HomeCenter.ComponentModel.Adapters.Kodi;
using HomeCenter.ComponentModel.Components;
using HomeCenter.Core.ComponentModel.Areas;
using HomeCenter.Core.ComponentModel.Configuration;
using HomeCenter.Core.Extensions;
using HomeCenter.Core.Services.DependencyInjection;
using HomeCenter.Core.Services.Logging;
using HomeCenter.Core.Utils;

namespace HomeCenter.ComponentModel.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IMapper _mapper;
        private readonly IAdapterServiceFactory _adapterServiceFactory;
        private readonly ILogger _logger;
        private readonly IResourceLocatorService _resourceLocatorService;
        private readonly IContainer _container;

        public ConfigurationService(IMapper mapper, IAdapterServiceFactory adapterServiceFactory, ILogService logService,
            IResourceLocatorService resourceLocatorService, IContainer container)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _adapterServiceFactory = adapterServiceFactory ?? throw new ArgumentNullException(nameof(adapterServiceFactory));
            _logger = logService.CreatePublisher(nameof(ConfigurationService));
            _resourceLocatorService = resourceLocatorService;
            _container = container;
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

            CheckForDuplicateUid(configuration);

            return configuration;
        }

        private void CheckForDuplicateUid(HomeCenterConfiguration configuration)
        {
            var allUids = configuration.Adapters.Select(a => a.Uid).ToList();
            allUids.AddRange(configuration.Components.Select(c => c.Uid));

            var duplicateKeys = allUids.GroupBy(x => x)
                                       .Where(group => group.Count() > 1)
                                       .Select(group => group.Key);
            if (duplicateKeys?.Count() > 0)
            {
                throw new Exception($"Duplicate UID's found in config file: {string.Join(", ", duplicateKeys)}");
            }
        }

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

        private IList<Adapter> MapAdapters(IList<AdapterDTO> adapterConfigs, AdapterMode adapterMode)
        {
            var adapters = new List<Adapter>();

            // force to load HomeCenter.AdaptersContainer into memory
            if (adapterMode == AdapterMode.Embedded)
            {
                var testAdapter = typeof(KodiAdapter);
            }

            var types = new List<Type>(AssemblyHelper.GetAllInherited<Adapter>());

            Mapper.Initialize(p =>
            {
                foreach (var adapterType in types)
                {
                    p.CreateMap(typeof(AdapterDTO), adapterType).ConstructUsingServiceLocator();
                }

                p.ShouldMapProperty = propInfo => (propInfo.CanWrite && propInfo.GetGetMethod(true).IsPublic) || propInfo.IsDefined(typeof(MapAttribute), false);
                p.ConstructServicesUsing(_container.GetInstance);
            });

            foreach (var adapterConfig in adapterConfigs)
            {
                try
                {
                    var adapterType = types.Find(t => t.Name == adapterConfig.Type);
                    if (adapterType == null) throw new Exception($"Could not find adapter {adapterType}");
                    var adapter = (Adapter)Mapper.Map(adapterConfig, typeof(AdapterDTO), adapterType);

                    adapters.Add(adapter);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Exception {ex.Message} while initializing adapter {adapterConfig.Type}");
                }
            }

            return adapters;
        }
    }
}