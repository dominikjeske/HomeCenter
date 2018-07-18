using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Wirehome.ComponentModel;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Components;
using Wirehome.Core.Utils;

namespace Wirehome.Core.ComponentModel.Configuration
{
    public class ValueConverter : JsonConverter
    {
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            IDictionary<string, IValueConverter> result = new Dictionary<string, IValueConverter>();

            var types = AssemblyHelper.GetProjectAssemblies()
                                      .SelectMany(s => s.GetTypes())
                                      .Where(p => typeof(IValueConverter).IsAssignableFrom(p));

            foreach (var property in JObject.Load(reader))
            {
                var converter = property.Value.Value<string>();
                var converterType = types.FirstOrDefault(t => t.Name == converter);

                if (converterType == null) throw new Exception($"Could not find converter {converter}");

                result.Add(property.Key, (IValueConverter)Activator.CreateInstance(converterType));
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
        public override bool CanConvert(Type objectType) => typeof(IDictionary<string, Property>).IsAssignableFrom(objectType);
        public override bool CanWrite => false;
    }
}
