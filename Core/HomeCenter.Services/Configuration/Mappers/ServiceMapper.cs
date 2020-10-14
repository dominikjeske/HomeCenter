using HomeCenter.Model.Actors;
using HomeCenter.Services.Configuration.DTO;
using Proto;
using System;
using System.Linq;

namespace HomeCenter.Services.Actors
{
    internal class ServiceMapper : ITypeMapper<ServiceDTO>
    {
        private readonly ITypeMapper<DeviceActorDTO> _actorMapper;

        public ServiceMapper(ITypeMapper<DeviceActorDTO> actorMapper)
        {
            _actorMapper = actorMapper;
        }

        public IActor Map(ServiceDTO config, Type destinationType)
        {
            if (_actorMapper.Map(config, destinationType) is not Service actor)
            {
                throw new ArgumentException($"{nameof(destinationType)} should be '{typeof(Service).Name}' type");
            }

            actor.AreasAttachedProperties = config.AreasAttachedProperties.Select(p => new AttachedProperty
            {
                AttachedActor = p.AttachedActor,
                AttachedArea = p.AttachedArea,
                Service = p.Service,
                Type = p.Type,
                Uid = p.Uid
            }).ToList();

            actor.ComponentsAttachedProperties = config.ComponentsAttachedProperties.Select(p => new AttachedProperty
            {
                AttachedActor = p.AttachedActor,
                AttachedArea = p.AttachedArea,
                Service = p.Service,
                Type = p.Type,
                Uid = p.Uid
            }).ToList();

            return actor;
        }
    }
}