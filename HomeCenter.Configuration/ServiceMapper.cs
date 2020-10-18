using HomeCenter.Model.Actors;
using HomeCenter.Services.Configuration.DTO;
using Proto;
using System;
using System.Linq;

namespace HomeCenter.Services.Actors
{
    public class ServiceMapper : ITypeMapper<ServiceDTO>
    {
        private readonly DeviceActorMapper _actorMapper;
        private readonly BaseObjectMapper _baseObjectMapper;

        public ServiceMapper(DeviceActorMapper actorMapper, BaseObjectMapper baseObjectMapper)
        {
            _actorMapper = actorMapper;
            _baseObjectMapper = baseObjectMapper;
        }

        public IActor Map(ServiceDTO config, Type destinationType)
        {
            if (_actorMapper.Map(config, destinationType) is not Service actor)
            {
                throw new ArgumentException($"{nameof(destinationType)} should be '{typeof(Service).Name}' type");
            }

            actor.AreasAttachedProperties = config.AreasAttachedProperties.Select(p => new
            {
                Source = p,
                Destination = new AttachedProperty(p.Service, p.AttachedActor, p.AttachedArea)
            }).Select(x => _baseObjectMapper.Map(x.Source, x.Destination)).ToList();

            actor.ComponentsAttachedProperties = config.ComponentsAttachedProperties.Select(p => new
            {
                Source = p,
                Destination = new AttachedProperty(p.Service, p.AttachedActor, p.AttachedArea)
            }).Select(x => _baseObjectMapper.Map(x.Source, x.Destination)).ToList();

            return actor;
        }
    }
}