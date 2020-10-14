using HomeCenter.Model.Adapters;
using HomeCenter.Services.Configuration.DTO;
using Proto;
using System;

namespace HomeCenter.Services.Actors
{
    internal class AdapterMapper : ITypeMapper<AdapterDTO>
    {
        private readonly DeviceActorMapper _actorMapper;

        public AdapterMapper(DeviceActorMapper actorMapper)
        {
            _actorMapper = actorMapper;
        }

        public IActor Map(AdapterDTO config, Type destinationType)
        {
            if (_actorMapper.Map(config, destinationType) is not Adapter adapter)
            {
                throw new ArgumentException($"{nameof(destinationType)} should be '{typeof(Adapter).Name}' type");
            }

            return adapter;
        }
    }
}