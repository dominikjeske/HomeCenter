using CSharpFunctionalExtensions;
using FastDeepCloner;
using HomeCenter.Broker;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Contracts;
using HomeCenter.Model.Core;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events.Service;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Services.Actors;
using HomeCenter.Services.Configuration.DTO;
using HomeCenter.Utils.Extensions;
using Newtonsoft.Json;
using Proto;
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
        private readonly IActorFactory _actorFactory;
        private readonly ITypeLoader _typeLoader;

        private IDictionary<string, PID> _services;
        private IDictionary<string, PID> _adapters;
        private PID _mainArea;
        private readonly IMessageBroker _messageBroker;

        protected ConfigurationService(IActorFactory actorFactory, ITypeLoader typeLoader, IMessageBroker messageBroker)
        {
            _actorFactory = actorFactory;
            _typeLoader = typeLoader;
            _messageBroker = messageBroker;
        }

        protected async Task Handle(StartSystemCommand startFromConfigCommand)
        {
            var configPath = startFromConfigCommand.Configuration;

            if (!File.Exists(configPath)) throw new ConfigurationException($"Configuration file not found at {configPath}");

            var rawConfig = File.ReadAllText(configPath);

            var result = JsonConvert.DeserializeObject<HomeCenterConfigDTO>(rawConfig);

            await LoadTypes();

            ResolveTemplates(result);

            ResolveAttachedProperties(result);

            CheckForDuplicateUid(result);

            await LoadActors(result);

            await MessageBroker.Publish(SystemStartedEvent.Default).ConfigureAwait(false);
        }

        private async Task LoadTypes()
        {
            await _typeLoader.LoadTypes();
        }

        private async Task LoadActors(HomeCenterConfigDTO result)
        {
            _services = CreataActors(result.HomeCenter.Services);
            _adapters = CreataActors(result.HomeCenter.SharedAdapters);
            _mainArea = await CreateAreas(result.HomeCenter.MainArea, null);
        }

        private Dictionary<string, PID> CreataActors<T>(IEnumerable<T> config) where T : IBaseObject, IPropertySource
        {
            var actors = new Dictionary<string, PID>();
            foreach (var actorConfig in config)
            {
                var actor = _actorFactory.CreateActor(actorConfig);
                actors.Add(actorConfig.Uid, actor);
            }
            return actors;
        }

        private async Task<PID> CreateAreas(AreaDTO area, IContext parent)
        {
            var areaActor = _actorFactory.CreateActor(area, parent);

            var actorContext = await _messageBroker.Request<ActorContextQuery, IContext>(ActorContextQuery.Default, areaActor);

            foreach (var component in area.Components)
            {
                var componentActor = _actorFactory.CreateActor(component.Clone(), actorContext); // clone to prevents override of componentConfig when executed in multi thread
            }

            foreach (var subArea in area.Areas)
            {
                await CreateAreas(subArea.Clone(), actorContext); // clone to prevents override of componentConfig when executed in multi thread
            }

            return areaActor;
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
            foreach (var component in rootArea.Components)
            {
                yield return (component, rootArea);
            }
            foreach (var area in rootArea.Areas)
            {
                foreach (var component in GetFlatComponentList(area))
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
    }
}