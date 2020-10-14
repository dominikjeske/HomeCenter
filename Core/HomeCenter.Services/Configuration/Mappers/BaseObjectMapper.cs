using HomeCenter.Model.Core;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages;
using HomeCenter.Services.Configuration.DTO;
using System;
using System.Text.Json;

namespace HomeCenter.Services.Actors
{
    internal class BaseObjectMapper
    {
        private readonly ClassActivator _classActivator;

        public BaseObjectMapper(ClassActivator classActivator)
        {
            _classActivator = classActivator;
        }

        public T Map<T>(BaseDTO config) where T : class
        {
            if (Map(config, typeof(T)) is not T result)
            {
                throw new InvalidOperationException($"Cannot create type {typeof(T).Name} from {config.GetType().Name}");
            }

            return result;
        }

        public BaseObject Map(BaseDTO config, BaseObject instance)
        {
            instance.SetProperty(MessageProperties.Uid, config.Uid);
            instance.SetProperty(MessageProperties.Type, config.Type);
            SetProperties(config, instance);

            return instance;
        }

        public BaseObject Map(BaseDTO config, Type destinationType)
        {
            destinationType.MustDeriveFrom<BaseObject>();

            if (_classActivator.Create(destinationType) is not BaseObject instance)
            {
                throw new InvalidCastException($"Type {destinationType} is not {typeof(BaseObject).Name}");
            }

            return Map(config, instance);
        }

        private static void SetProperties(BaseDTO config, BaseObject instance)
        {
            foreach (var property in config.Properties)
            {
                if (property.Value is JsonElement element)
                {
                    if (element.ValueKind == JsonValueKind.Number)
                    {
                        if (element.TryGetInt32(out int intValue))
                        {
                            instance.SetProperty(property.Key, intValue);
                        }
                        if (element.TryGetDouble(out double douleValue))
                        {
                            instance.SetProperty(property.Key, douleValue);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else if (element.ValueKind == JsonValueKind.True)
                    {
                        instance.SetProperty(property.Key, true);
                    }
                    else if (element.ValueKind == JsonValueKind.False)
                    {
                        instance.SetProperty(property.Key, false);
                    }
                    else if (element.ValueKind == JsonValueKind.String)
                    {
                        var stringValue = element.GetString();
                        if (TimeSpan.TryParse(stringValue, out TimeSpan timeValue))
                        {
                            instance.SetProperty(property.Key, timeValue);
                        }
                        else if (DateTime.TryParse(stringValue, out DateTime dateValue))
                        {
                            instance.SetProperty(property.Key, timeValue);
                        }
                        else if (Guid.TryParse(stringValue, out Guid guidValue))
                        {
                            instance.SetProperty(property.Key, timeValue);
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }

}