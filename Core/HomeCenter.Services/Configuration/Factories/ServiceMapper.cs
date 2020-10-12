using HomeCenter.Model.Actors;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages;
using HomeCenter.Services.Configuration.DTO;
using Proto;
using System;

namespace HomeCenter.Services.Actors
{
    internal class ServiceMapper : ITypeMapper<ServiceDTO>
    {
        private readonly ClassActivator _classActivator;

        public ServiceMapper(ClassActivator classActivator)
        {
            _classActivator = classActivator;
        }

        public IActor Create(ServiceDTO config, Type destinationType)
        {
            destinationType.MustDeriveFrom<IActor>();

            if (_classActivator.Create(destinationType) is not DeviceActor instance)
            {
                throw new InvalidCastException($"Type {destinationType} is not {typeof(DeviceActor).Name}");
            }

            instance.SetProperty(MessageProperties.Uid, config.Uid);
            instance.SetProperty(MessageProperties.Type, config.Type);
            instance.SetProperty(MessageProperties.IsEnabled, config.IsEnabled);
            instance.SetProperty(MessageProperties.Tags, config.Tags);

            foreach (var property in config.Properties)
            {
                instance.SetProperty(property.Key, property.Value.ToString());
            }

            return instance as IActor;
        }
    }

    //var properties = destinationType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
    //                                .Where(prop => prop.IsDefined(typeof(MapAttribute), false));

    //try
    //{
    //    foreach (var property in properties)
    //    {
    //        var sourceProperty = config.GetType().GetProperty(property.Name);
    //        var sourceValue = sourceProperty.GetValue(config);

    //        if (sourceValue is IList source)
    //        {
    //            foreach (object el in source)
    //            {
    //            }
    //        }
    //        else
    //        {
    //            try
    //            {
    //                property.SetValue(instance, sourceValue);
    //            }
    //            catch (Exception fff)
    //            {
    //                throw;
    //            }

    //        }
    //    }
    //}
    //catch (Exception eee)
    //{
    //}
}