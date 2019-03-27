using AutoMapper;
using CSharpFunctionalExtensions;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Areas;
using HomeCenter.Model.Calendars;
using HomeCenter.Model.Components;
using HomeCenter.Model.Contracts;
using HomeCenter.Model.Core;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events.Service;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Model.Triggers.Calendars;
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
using System.Threading.Tasks;

namespace HomeCenter.Services.Configuration
{
    [ProxyCodeGenerator]
    public abstract class ConfigurationService : Service
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ConfigurationService> _logger;
        private readonly IActorFactory _actorFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRoslynCompilerService _roslynCompilerService;

        private IDictionary<string, PID> _services;
        private IDictionary<string, PID> _adapters;
        private IDictionary<string, PID> _components;
        private readonly IScheduler _scheduler;

        protected ConfigurationService(IMapper mapper, ILogger<ConfigurationService> logger, IActorFactory actorFactory,
                                       IServiceProvider serviceProvider, IRoslynCompilerService roslynCompilerService, IScheduler scheduler)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger;
            _actorFactory = actorFactory;
            _serviceProvider = serviceProvider;
            _roslynCompilerService = roslynCompilerService;
            _scheduler = scheduler;
        }

        protected async Task Handle(StartSystemCommand startFromConfigCommand)
        {
            var configPath = startFromConfigCommand.Configuration;

            if (!File.Exists(configPath))
            {
                throw new ConfigurationException($"Configuration file not found at {configPath}");
            }

            var rawConfig = File.ReadAllText(configPath);

            var result = JsonConvert.DeserializeObject<HomeCenterConfigDTO>(rawConfig);

            LoadDynamicAdapters(startFromConfigCommand.AdapterMode);

            ResolveTemplates(result);

            ResolveInlineAdapters(result);

            ResolveAttachedProperties(result);

            CheckForDuplicateUid(result);

            // force to load HomeCenter.ActorsContainer into memory
            if (startFromConfigCommand.AdapterMode == "Embedded")
            {
                var testAdapter = typeof(Actors.ForceAssemblyLoadType);
            }

            await LoadCalendars().ConfigureAwait(false);

            var types = RegisterTypesInAutomapper();

            _services = CreataActors<ServiceDTO, Service>(result.HomeCenter.Services, types[typeof(ServiceDTO)]);
            _adapters = CreataActors<AdapterDTO, Adapter>(result.HomeCenter.Adapters, types[typeof(AdapterDTO)]);
            _components = MapComponents(result);
            var areas = MapAreas(result, _components);

            await MessageBroker.Publish(SystemStartedEvent.Default).ConfigureAwait(false);
        }

        protected async Task<bool> Handle(StopSystemQuery stopSystemCommand)
        {
            foreach (var service in _services.Values)
            {
                await service.StopAsync();
            }

            foreach (var adapter in _adapters.Values)
            {
                await adapter.StopAsync();
            }

            foreach (var component in _components.Values)
            {
                await component.StopAsync();
            }

            return true;
        }

        private void ResolveAttachedProperties(HomeCenterConfigDTO result)
        {
            var components = result.HomeCenter.Components.Where(c => c.AttachedProperties?.Count > 0);
            foreach (var component in components)
            {
                foreach (var property in component.AttachedProperties)
                {
                    var serviceDto = result.HomeCenter.Services.FirstOrDefault(s => s.Uid == property.Service);
                    if (serviceDto == null) throw new MissingMemberException($"Service {property.Service} was not found in configuration");

                    var area = result.HomeCenter.Areas.Flatten(a => a.Areas).FirstOrDefault(a => a.ComponentsRefs?.Any(c => c.Uid.InvariantEquals(component.Uid)) ?? false);
                    if (area == null) throw new MissingMemberException($"Component {component.Uid} was not found in any area");

                    property.AttachedActor = component.Uid;
                    property.AttachedArea = area.Uid;

                    serviceDto.ComponentsAttachedProperties.Add(property);
                }
            }

            var areas = result.HomeCenter.Areas.Flatten(a => a.Areas).Where(c => c.AttachedProperties?.Count > 0);

            foreach (var area in areas)
            {
                foreach (var property in area.AttachedProperties)
                {
                    var serviceDto = result.HomeCenter.Services.FirstOrDefault(s => s.Uid == property.Service);
                    if (serviceDto == null) throw new MissingMemberException($"Service {property.Service} was not found in configuration");

                    property.AttachedActor = area.Uid;
                    serviceDto.AreasAttachedProperties.Add(property);
                }
            }
        }

        private void ResolveTemplates(HomeCenterConfigDTO result)
        {
            var templatedComponents = result.HomeCenter.Components.Where(c => !string.IsNullOrWhiteSpace(c.Template));
            var resolved = new Dictionary<ComponentDTO, ComponentDTO>();

            foreach (var component in templatedComponents)
            {
                var template = result.HomeCenter.Templates.Single(t => t.Uid == component.Template);
                var templateCopy = _mapper.Map<ComponentDTO>(template);
                templateCopy.Uid = component.Uid;

                foreach (var adapter in templateCopy.Adapters)
                {
                    adapter.Uid = GetTemplateValueOrDefault(adapter.Uid, component.TemplateProperties);

                    foreach (var property in adapter.Properties.Keys.ToList())
                    {
                        var propvalue = adapter.Properties[property];
                        if (propvalue.IndexOf("#") > -1)
                        {
                            if (!component.TemplateProperties.ContainsKey(propvalue)) throw new ConfigurationException($"Property '{propvalue}' was not found in component '{component.Uid}'");
                            adapter.Properties[property] = component.TemplateProperties[propvalue];
                        }
                    }
                }

                foreach (var attachedProperty in templateCopy.AttachedProperties)
                {
                    foreach (var property in attachedProperty.Properties.Keys.ToList())
                    {
                        var propvalue = attachedProperty.Properties[property];
                        if (propvalue.IndexOf("#") > -1)
                        {
                            if (!component.TemplateProperties.ContainsKey(propvalue)) throw new ConfigurationException($"Property '{propvalue}' was not found in component '{component.Uid}'");
                            attachedProperty.Properties[property] = component.TemplateProperties[propvalue];
                        }
                    }
                }

                resolved.Add(component, templateCopy);
            }

            foreach (var comp in resolved.Keys)
            {
                result.HomeCenter.Components.Remove(comp);
                result.HomeCenter.Components.Add(resolved[comp]);
            }
        }

        private string GetTemplateValueOrDefault(string varible, IDictionary<string, string> templateValues)
        {
            if (templateValues.ContainsKey(varible))
            {
                return templateValues[varible];
            }
            return varible;
        }

        private void ResolveInlineAdapters(HomeCenterConfigDTO result)
        {
            foreach (var component in result.HomeCenter.Components.Where(c => c.Adapter != null).Select(c => c))
            {
                result.HomeCenter.Adapters.Add(component.Adapter);
                component.Adapters = new List<AdapterReferenceDTO>() { new AdapterReferenceDTO { Uid = component.Adapter.Uid, Type = component.Adapter.Type } };
            }
        }

        private void CheckForDuplicateUid(HomeCenterConfigDTO configuration)
        {
            var allUids = configuration.HomeCenter?.Adapters?.Select(a => a.Uid).ToList();
            allUids.AddRange(configuration.HomeCenter?.Components?.Select(c => c.Uid));
            allUids.AddRange(configuration.HomeCenter?.Services?.Select(c => c.Uid));

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
                if (areInConfig?.ComponentsRefs != null)
                {
                    foreach (var component in areInConfig?.ComponentsRefs)
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

                var component = _actorFactory.CreateActor(() => _mapper.Map<ComponentProxy>(localConfigCopy), localConfigCopy.Uid);
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
                    if (actorType == null) throw new ConfigurationException($"Could not find type for actor {actorType}");
                    var actor = _actorFactory.CreateActor(() => (Q)Mapper.Map(actorConfig, typeof(T), actorType), actorConfig.Uid, routing: routing);

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

        private Dictionary<Type, List<Type>> RegisterTypesInAutomapper()
        {
            Dictionary<Type, List<Type>> types = new Dictionary<Type, List<Type>>();

            types[typeof(AdapterDTO)] = new List<Type>(AssemblyHelper.GetAllTypes<Adapter>(false));
            types[typeof(ServiceDTO)] = new List<Type>(AssemblyHelper.GetAllTypes<Service>(false));

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
                //var result = _roslynCompilerService.CompileAssemblies(_controllerOptions.AdapterRepoName);
                //var veryfy = Result.Combine(result.ToArray());
                //if (veryfy.IsFailure) throw new CompilationException($"Error while compiling adapters: {veryfy.Error}");

                //foreach (var adapter in result)
                //{
                //    Assembly.LoadFrom(adapter.Value);
                //}
            }
            else
            {
                Logger.LogInformation($"Using only build in adapters");
            }
        }

        private async Task LoadCalendars()
        {
            foreach (var calendarType in AssemblyHelper.GetAllTypes<IDayOffProvider>())
            {
                var dayOffProvider = calendarType.CreateInstance<IDayOffProvider>();
                var calendar = new QuartzCalendar(dayOffProvider);

                await _scheduler.AddCalendar(dayOffProvider.Name, calendar, false, false).ConfigureAwait(false);
            }
        }
    }
}