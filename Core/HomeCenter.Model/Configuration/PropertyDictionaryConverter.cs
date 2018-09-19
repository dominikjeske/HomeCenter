using HomeCenter.ComponentModel;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Model.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace HomeCenter.Core.ComponentModel.Configuration
{
    public class PropertyDictionaryConverter : JsonConverter
    {
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            IDictionary<string, Property> result = new Dictionary<string, Property>();

            foreach (var property in JObject.Load(reader))
            {
                IValue value;

                if (property.Value.Type == JTokenType.Float)
                {
                    value = new DoubleValue(property.Value.Value<double>());
                }
                else if (property.Value.Type == JTokenType.Integer)
                {
                    value = new IntValue(property.Value.Value<int>());
                }
                else if (property.Value.Type == JTokenType.Boolean)
                {
                    value = new BooleanValue(property.Value.Value<bool>());
                }
                else if (property.Value.Type == JTokenType.Array)
                {
                    value = new StringListValue(property.Value.Values<string>());
                }
                else if (property.Value.Type == JTokenType.Date)
                {
                    value = new DateTimeValue(property.Value.Value<DateTime>());
                }
                else if (property.Value.Type == JTokenType.TimeSpan)
                {
                    value = new TimeSpanValue(property.Value.Value<TimeSpan>());
                }
                else
                {
                    var stringValue = property.Value.Value<string>();
                    if (TimeSpan.TryParse(stringValue, out var time))
                    {
                        value = new TimeSpanValue(time);
                    }
                    else if (DateTime.TryParse(stringValue, out var date))
                    {
                        value = new DateTimeValue(date);
                    }
                    else
                    {
                        value = new StringValue(stringValue);
                    }
                }

                var newProperty = new Property
                {
                    Key = property.Key,
                    Value = value
                };

                result.Add(newProperty.Key, newProperty);
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

        public override bool CanConvert(Type objectType) => typeof(IDictionary<string, Property>).IsAssignableFrom(objectType);

        public override bool CanWrite => false;
    }
}