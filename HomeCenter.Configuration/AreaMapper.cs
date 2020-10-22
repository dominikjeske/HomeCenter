using HomeCenter.Abstractions;
using HomeCenter.Services.Configuration.DTO;
using Proto;
using System;
using HomeCenter.Model.Areas;

namespace HomeCenter.Services.Actors
{
    internal class AreaMapper : ITypeMapper<AreaDTO>
    {
        private readonly DeviceActorMapper _actorMapper;

        public AreaMapper(DeviceActorMapper actorMapper)
        {
            _actorMapper = actorMapper;
        }

        public IActor Map(AreaDTO config, Type destinationType)
        {
            if (_actorMapper.Map(config, destinationType) is not Area actor)
            {
                throw new ArgumentException($"{nameof(destinationType)} should be '{typeof(Area).Name}' type");
            }

            return actor;
        }
    }
}