using Serilog.Debugging;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HomeCenter.Storage.RavenDB
{
    public static class RavenPropertyFormatter
    {
        private static readonly HashSet<Type> RavenSpecialScalars = new()
        {
            typeof(bool), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), 
            typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(byte[])
        };

        public static object? Simplify(LogEventPropertyValue value)
        {
            if (value is ScalarValue scalar)
            {
                return SimplifyScalar(scalar.Value);
            }

            if(value is DictionaryValue dict)
            {
                var result = new Dictionary<object, object?>();
                foreach (var element in dict.Elements)
                {
                    var key = SimplifyScalar(element.Key.Value);
                    if (key is null) continue;

                    if (result.ContainsKey(key))
                    {
                        SelfLog.WriteLine("The key {0} is not unique in the provided dictionary after simplification to {1}.", element.Key, key);
                        return dict.Elements.Select(e => new Dictionary<string, object?>
                        {
                            { "Key", SimplifyScalar(e.Key.Value) },
                            { "Value", Simplify(e.Value) }
                        })
                        .ToArray();
                    }

                    result.Add(key, Simplify(element.Value));
                }
                return result;
            }

            if (value is SequenceValue seq)
            {
                return seq.Elements.Select(Simplify).ToArray();
            }

            if (value is StructureValue str)
            {
                var props = str.Properties.ToDictionary(p => p.Name, p => Simplify(p.Value));
                if (str.TypeTag != null)
                {
                    props["$typeTag"] = str.TypeTag;
                }
                return props;
            }

            return null;
        }

        private static object? SimplifyScalar(object? value)
        {
            if (value == null) return null;

            if (RavenSpecialScalars.Contains(value.GetType())) return value;

            return value.ToString();
        }
    }
}