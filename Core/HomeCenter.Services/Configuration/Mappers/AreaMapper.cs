using HomeCenter.Model.Areas;
using HomeCenter.Services.Configuration.DTO;
using Proto;
using System;

namespace HomeCenter.Services.Actors
{
    internal class AreaMapper : ITypeMapper<AreaDTO>
    {
        private readonly ITypeMapper<DeviceActorDTO> _actorMapper;

        public AreaMapper(ITypeMapper<DeviceActorDTO> actorMapper)
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