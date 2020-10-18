using HomeCenter.Model.Actors;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages;
using HomeCenter.Services.Configuration.DTO;
using Proto;
using System;

namespace HomeCenter.Services.Actors
{
    public class DeviceActorMapper
    {
        private readonly BaseObjectMapper _baseObjectMapper;

        public DeviceActorMapper(BaseObjectMapper baseObjectMapper)
        {
            _baseObjectMapper = baseObjectMapper;
        }

        public DeviceActor Map(DeviceActorDTO config, Type destinationType)
        {
            destinationType.MustDeriveFrom<IActor>();

            if (_baseObjectMapper.Map(config, destinationType) is not DeviceActor instance)
            {
                throw new InvalidCastException($"Type {destinationType} is not {typeof(DeviceActor).Name}");
            }

            return Map(config, instance);
        }

        public DeviceActor Map(DeviceActorDTO config, DeviceActor instance)
        {
            instance.SetProperty(MessageProperties.IsEnabled, config.IsEnabled);
            instance.SetProperty(MessageProperties.Tags, config.Tags);

            return instance;
        }
    }
}