using HomeCenter.Model.Core;
using HomeCenter.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace HomeCenter
{
    public  static class BaseObjectAsExtensions
    {
        public static bool AsBool(this BaseObject baseObject, string propertyName, bool? defaultValue = null)
        {
            if (!baseObject.ContainsProperty(propertyName)) return defaultValue ?? throw new ArgumentException(propertyName);

            if (bool.TryParse(baseObject[propertyName], out bool value))
            {
                return value;
            }

            throw new FormatException($"Property {propertyName} value {baseObject[propertyName]} is not proper bool value");
        }

        public static int AsInt(this BaseObject baseObject, string propertyName, int? defaultValue = null)
        {
            if (!baseObject.ContainsProperty(propertyName)) return defaultValue ?? throw new ArgumentException(propertyName);

            if (int.TryParse(baseObject[propertyName], out int value))
            {
                return value;
            }

            throw new FormatException($"Property {propertyName} value {baseObject[propertyName]} is not proper int value");
        }

        public static byte AsByte(this BaseObject baseObject, string propertyName, byte? defaultValue = null)
        {
            if (!baseObject.ContainsProperty(propertyName)) return defaultValue ?? throw new ArgumentException(propertyName);

            if (byte.TryParse(baseObject[propertyName], out byte value))
            {
                return value;
            }

            throw new FormatException($"Property {propertyName} value {baseObject[propertyName]} is not proper byte value");
        }

        public static DateTime AsDate(this BaseObject baseObject, string propertyName, DateTime? defaultValue = null)
        {
            if (!baseObject.ContainsProperty(propertyName)) return defaultValue ?? throw new ArgumentException(propertyName);

            if (DateTime.TryParse(baseObject[propertyName], out DateTime value))
            {
                return value;
            }

            throw new FormatException($"Property {propertyName} value {baseObject[propertyName]} is not proper date value");
        }

        public static TimeSpan AsTime(this BaseObject baseObject, string propertyName, TimeSpan? defaultValue = null)
        {
            if (!baseObject.ContainsProperty(propertyName)) return defaultValue ?? throw new ArgumentException(propertyName);

            if (TimeSpan.TryParse(baseObject[propertyName], out TimeSpan value))
            {
                return value;
            }

            throw new FormatException($"Property {propertyName} value {baseObject[propertyName]} is not proper time value");
        }

        public static TimeSpan AsIntTime(this BaseObject baseObject, string propertyName, int? defaultValue = null)
        {
            if (!baseObject.ContainsProperty(propertyName))
            {
                if (defaultValue.HasValue)
                {
                    return TimeSpan.FromMilliseconds(defaultValue.Value);
                }
                throw new ArgumentException(propertyName);
            }

            if (int.TryParse(baseObject[propertyName], out int value))
            {
                return TimeSpan.FromMilliseconds(value);
            }

            throw new FormatException($"Property {propertyName} value {baseObject[propertyName]} is not proper time value");
        }

        public static double AsDouble(this BaseObject baseObject, string propertyName, double defaultValue = double.MinValue)
        {
            if (!baseObject.ContainsProperty(propertyName))
            {
                if (defaultValue != double.MinValue) return defaultValue;
                throw new ArgumentException(propertyName);
            }
            return AsDoubleInner(baseObject, propertyName);
        }

        public static double? AsNullableDouble(this BaseObject baseObject, string propertyName)
        {
            if (!baseObject.ContainsProperty(propertyName)) return null;

            return AsDoubleInner(baseObject, propertyName);
        }

        private static double AsDoubleInner(BaseObject baseObject, string propertyName)
        {
            if (baseObject[propertyName].ParseAsDouble(out double value))
            {
                return value;
            }

            throw new FormatException($"Property {propertyName} value {baseObject[propertyName]} is not proper double value");
        }

        public static uint AsUint(this BaseObject baseObject, string propertyName, uint? defaultValue = null)
        {
            if (!baseObject.ContainsProperty(propertyName)) return defaultValue ?? throw new ArgumentException(propertyName);

            if (uint.TryParse(baseObject[propertyName], out uint value))
            {
                return value;
            }

            throw new FormatException($"Property {propertyName} value {baseObject[propertyName]} is not proper uint value");
        }

        public static string AsString(this BaseObject baseObject, string propertyName, string defaultValue = null)
        {
            if (!baseObject.ContainsProperty(propertyName)) return defaultValue ?? throw new ArgumentException(propertyName);

            return baseObject[propertyName];
        }

        public static IList<string> AsList(this BaseObject baseObject, string propertyName, IList<string> defaultValue = null)
        {
            if (!baseObject.ContainsProperty(propertyName)) return (IList<string>)defaultValue ?? throw new ArgumentException(propertyName);

            return baseObject[propertyName].Split(',').Select(x => x.Trim()).ToList();
        }

        public static byte[] AsByteArray(this BaseObject baseObject, string propertyName, byte[] defaultValue = null)
        {
            if (!baseObject.ContainsProperty(propertyName)) return defaultValue ?? throw new ArgumentException(propertyName);

            return baseObject[propertyName].ToBytes();
        }

        public static IDictionary<string, string> AsDictionary(this BaseObject baseObject, string propertyName, IDictionary<string, string> defaultValue = null)
        {
            if (!baseObject.ContainsProperty(propertyName)) return defaultValue ?? throw new ArgumentException(propertyName);

            return JsonSerializer.Deserialize<IDictionary<string, string>>(baseObject[propertyName]);
        }
    }
}