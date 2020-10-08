using HomeCenter.Model.Components;
using HomeCenter.Model.Core;
using HomeCenter.Model.Exceptions;
using HomeCenter.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration
{
    //TODO DNF
    //public class ValueConverter : JsonConverter
    //{
    //    public override object ReadJson(
    //        JsonReader reader,
    //        Type objectType,
    //        object existingValue,
    //        JsonSerializer serializer)
    //    {
    //        IDictionary<string, IValueConverter> result = new Dictionary<string, IValueConverter>();

    //        var types = AssemblyHelper.GetAllTypes<IValueConverter>();

    //        foreach (var property in JObject.Load(reader))
    //        {
    //            var converter = property.Value.Value<string>();
    //            var converterType = types.FirstOrDefault(t => t.Name == converter);

    //            if (converterType == null) throw new ConfigurationException($"Could not find converter {converter}");

    //            result.Add(property.Key, (IValueConverter)Activator.CreateInstance(converterType));
    //        }

    //        return result;
    //    }

    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

    //    public override bool CanConvert(Type objectType) => typeof(IDictionary<string, string>).IsAssignableFrom(objectType);

    //    public override bool CanWrite => false;
    //}
}