using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Wirehome.ComponentModel;
using Wirehome.ComponentModel.ValueTypes;

namespace Wirehome.Core.ComponentModel.Configuration
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
                else
                {
                    value = new StringValue(property.Value.Value<string>());
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