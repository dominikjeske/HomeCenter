using AutoMapper;
using CSharpFunctionalExtensions;
using FastDeepCloner;
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
        private PID _mainArea;

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
            _adapters = CreataActors<AdapterDTO, Adapter>(result.HomeCenter.SharedAdapters, types[typeof(AdapterDTO)]);
            _mainArea = MapAreas(result);

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

            await _mainArea.StopAsync();

            return true;
        }

        private IEnumerable<(ComponentDTO Component, AreaDTO Area)> GetFlatComponentList(AreaDTO rootArea)
        {
            foreach(var component in rootArea.Components)
            {
                yield return (component, rootArea);
            }
            foreach(var area in rootArea.Areas)
            {
                foreach(var component in GetFlatComponentList(area))
                {
                    yield return component;
                }
            }
        }

        private void ResolveTemplates(HomeCenterConfigDTO result)
        {
            foreach (var component in GetFlatComponentList(result.HomeCenter.MainArea).Where(c => !string.IsNullOrWhiteSpace(c.Component.Template)).ToList())
            {
                var template = result.HomeCenter.Templates.Single(t => t.Uid == component.Component.Template);
                var templateCopy = template.Clone();
                templateCopy.Uid = component.Component.Uid;

                foreach (var adapter in templateCopy.Adapters)
                {
                    adapter.Uid = GetTemplateValueOrDefault(adapter.Uid, component.Component.TemplateProperties);

                    foreach (var property in adapter.Properties.Keys.ToList())
                    {
                        var propvalue = adapter.Properties[property];
                        if (propvalue.IndexOf("#") > -1)
                        {
                            if (!component.Component.TemplateProperties.ContainsKey(propvalue)) throw new ConfigurationException($"Property '{propvalue}' was not found in component '{component.Component.Uid}'");
                            adapter.Properties[property] = component.Component.TemplateProperties[propvalue];
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
                            if (!component.Component.TemplateProperties.ContainsKey(propvalue)) throw new ConfigurationException($"Property '{propvalue}' was not found in component '{component.Component.Uid}'");
                            attachedProperty.Properties[property] = component.Component.TemplateProperties[propvalue];
                        }
                    }
                }

                component.Area.Components.Remove(component.Component);
                component.Area.Components.Add(templateCopy);
            }
        }

        private void ResolveAttachedProperties(HomeCenterConfigDTO result)
        {
            foreach (var component in GetFlatComponentList(result.HomeCenter.MainArea).Where(c => c.Component.AttachedProperties?.Count > 0))
            {
                foreach (var property in component.Component.AttachedProperties)
                {
                    var propertyCopy = property.Clone();

                    var serviceDto = result.HomeCenter.Services.FirstOrDefault(s => s.Uid == propertyCopy.Service);
                    if (serviceDto == null) throw new MissingMemberException($"Service {propertyCopy.Service} was not found in configuration");

                    propertyCopy.AttachedActor = component.Component.Uid;
                    propertyCopy.AttachedArea = component.Area.Uid;

                    serviceDto.ComponentsAttachedProperties.Add(propertyCopy);
                }
            }

            var areas = result.HomeCenter.MainArea.Areas.Flatten(a => a.Areas).Where(c => c.AttachedProperties?.Count > 0);

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

      

        private string GetTemplateValueOrDefault(string varible, IDictionary<string, string> templateValues)
        {
            if (templateValues.ContainsKey(varible))
            {
                return templateValues[varible];
            }
            return varible;
        }

        private void CheckForDuplicateUid(HomeCenterConfigDTO configuration)
        {
            var allUids = configuration.HomeCenter?.SharedAdapters?.Select(a => a.Uid).ToList();
            allUids.AddRange(GetFlatComponentList(configuration.HomeCenter.MainArea).Select(c => c.Component.Uid));
            allUids.AddRange(configuration.HomeCenter?.Services?.Select(c => c.Uid));

            var duplicateKeys = allUids.GroupBy(x => x)
                                       .Where(group => group.Count() > 1)
                                       .Select(group => group.Key);
            if (duplicateKeys?.Count() > 0)
            {
                throw new ConfigurationException($"Duplicate UID's found in config file: {string.Join(", ", duplicateKeys)}");
            }
        }

        private PID MapAreas(HomeCenterConfigDTO result) => CreateArea(result.HomeCenter.MainArea, null);

        private PID CreateArea(AreaDTO area, PID parent)
        {
            var context = new ActorContext(null, parent);

            var mainArea = _actorFactory.CreateActor(() =>
            {
                var mapped = _mapper.Map<Area>(area);
                return mapped;
            }, area.Uid, parent: context);

            foreach(var component in area.Components)
            {
                var localConfigCopy = component; // prevents override of componentConfig when executed in multi thread
                _actorFactory.CreateActor(() =>
                {
                    var mapped = _mapper.Map<ComponentProxy>(localConfigCopy);
                    return mapped;
                }, localConfigCopy.Uid);
            }

            foreach (var subArea in area.Areas)
            {
                var subAreaConfig = subArea; // prevents override of componentConfig when executed in multi thread
                CreateArea(subAreaConfig, mainArea);
            }

            return mainArea;
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
                    var actor = _actorFactory.CreateActor(() =>
                    {
                        var mapped = (Q)Mapper.Map(actorConfig, typeof(T), actorType);
                        return mapped;
                    }, actorConfig.Uid, routing: routing);

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