using HomeCenter.Broker;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Messages;
using HomeCenter.Utils.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HomeCenter.Model.Core
{
    public class BaseObject : IPropertiesSource
    {
        [Map] private Dictionary<string, string> _properties { get; set; } = new Dictionary<string, string>();

        public string Uid
        {
            get => AsString(MessageProperties.Uid, GetType().Name);
            set => SetProperty(MessageProperties.Uid, value);
        }

        public string Type
        {
            get => AsString(MessageProperties.Type);
            set => SetProperty(MessageProperties.Type, value);
        }
        
        public BaseObject()
        {
            Type = GetType().Name;
        }

        public BaseObject(IDictionary<string, string> properties)
        {
            foreach (var property in properties)
            {
                SetProperty(property.Key, property.Value);
            }
        }

        public override string ToString() => GetProperties()?.ToFormatedString();
  
        public bool ContainsProperty(string propertyName) => _properties.ContainsKey(propertyName);

        public void SetProperty(string propertyName, string value) => _properties[propertyName] = value;

        public void SetProperty(string propertyName, DateTimeOffset value) => _properties[propertyName] = value.ToString();

        public void SetProperty(string propertyName, TimeSpan value) => _properties[propertyName] = value.ToString();

        public void SetProperty(string propertyName, int value) => _properties[propertyName] = value.ToString();

        public void SetProperty(string propertyName, double value) => _properties[propertyName] = value.ToString();

        public void SetProperty(string propertyName, bool value) => _properties[propertyName] = value.ToString();

        public void SetProperty(string propertyName, byte[] value) => _properties[propertyName] = value.ToHex();

        public void SetProperty(string propertyName, IDictionary<string, string> value) => _properties[propertyName] = JsonConvert.SerializeObject(value);

        public void SetPropertyList(string propertyName, params string[] values) => _properties[propertyName] = string.Join(", ", values);

        public IReadOnlyDictionary<string, string> GetProperties() => _properties.AsReadOnly();

        public IEnumerable<string> GetPropetiesKeys() => _properties.Keys;

        public string this[string propertyName]
        {
            get
            {
                if (!ContainsProperty(propertyName)) throw new KeyNotFoundException($"Property {propertyName} not found on component {Uid}");
                return _properties[propertyName];
            }
            set { SetProperty(propertyName, value); }
        }

        public void SetEmptyProperty(string propertyName)
        {
            if (_properties.ContainsKey(propertyName)) return;
            _properties[propertyName] = string.Empty;
        }

        public bool AsBool(string propertyName, bool? defaultValue = null)
        {
            if (!_properties.ContainsKey(propertyName)) return defaultValue ?? throw new MissingPropertyException(propertyName);

            if (bool.TryParse(_properties[propertyName], out bool value))
            {
                return value;
            }

            throw new WrongPropertyFormatException($"Property {propertyName} value {_properties[propertyName]} is not proper bool value");
        }

        public int AsInt(string propertyName, int? defaultValue = null)
        {
            if (!_properties.ContainsKey(propertyName)) return defaultValue ?? throw new MissingPropertyException(propertyName);

            if (int.TryParse(_properties[propertyName], out int value))
            {
                return value;
            }

            throw new WrongPropertyFormatException($"Property {propertyName} value {_properties[propertyName]} is not proper int value");
        }

        public byte AsByte(string propertyName, byte? defaultValue = null)
        {
            if (!_properties.ContainsKey(propertyName)) return defaultValue ?? throw new MissingPropertyException(propertyName);

            if (byte.TryParse(_properties[propertyName], out byte value))
            {
                return value;
            }

            throw new WrongPropertyFormatException($"Property {propertyName} value {_properties[propertyName]} is not proper byte value");
        }

        public DateTime AsDate(string propertyName, DateTime? defaultValue = null)
        {
            if (!_properties.ContainsKey(propertyName)) return defaultValue ?? throw new MissingPropertyException(propertyName);

            if (DateTime.TryParse(_properties[propertyName], out DateTime value))
            {
                return value;
            }

            throw new WrongPropertyFormatException($"Property {propertyName} value {_properties[propertyName]} is not proper date value");
        }

        public TimeSpan AsTime(string propertyName, TimeSpan? defaultValue = null)
        {
            if (!_properties.ContainsKey(propertyName)) return defaultValue ?? throw new MissingPropertyException(propertyName);

            if (TimeSpan.TryParse(_properties[propertyName], out TimeSpan value))
            {
                return value;
            }

            throw new WrongPropertyFormatException($"Property {propertyName} value {_properties[propertyName]} is not proper time value");
        }

        public TimeSpan AsIntTime(string propertyName, int? defaultValue = null)
        {
            if (!_properties.ContainsKey(propertyName))
            {
                if (defaultValue.HasValue)
                {
                    return TimeSpan.FromMilliseconds(defaultValue.Value);
                }
                throw new MissingPropertyException(propertyName);
            }

            if (int.TryParse(_properties[propertyName], out int value))
            {
                return TimeSpan.FromMilliseconds(value);
            }

            throw new WrongPropertyFormatException($"Property {propertyName} value {_properties[propertyName]} is not proper time value");
        }

        public double AsDouble(string propertyName, double? defaultValue = null)
        {
            if (!_properties.ContainsKey(propertyName)) return defaultValue ?? throw new MissingPropertyException(propertyName);

            if (double.TryParse(_properties[propertyName], out double value))
            {
                return value;
            }

            throw new WrongPropertyFormatException($"Property {propertyName} value {_properties[propertyName]} is not proper double value");
        }

        public uint AsUint(string propertyName, uint? defaultValue = null)
        {
            if (!_properties.ContainsKey(propertyName)) return defaultValue ?? throw new MissingPropertyException(propertyName);

            if (uint.TryParse(_properties[propertyName], out uint value))
            {
                return value;
            }

            throw new WrongPropertyFormatException($"Property {propertyName} value {_properties[propertyName]} is not proper uint value");
        }

        public string AsString(string propertyName, string defaultValue = null)
        {
            if (!_properties.ContainsKey(propertyName)) return defaultValue ?? throw new MissingPropertyException(propertyName);

            return _properties[propertyName];
        }

        public IList<string> AsList(string propertyName, IEnumerable<string> defaultValue = null)
        {
            if (!_properties.ContainsKey(propertyName)) return (IList<string>)defaultValue ?? throw new MissingPropertyException(propertyName);

            return _properties[propertyName].Split(',').Select(x => x.Trim()).ToList();
        }

        public byte[] AsByteArray(string propertyName, byte[] defaultValue = null)
        {
            if (!_properties.ContainsKey(propertyName)) return defaultValue ?? throw new MissingPropertyException(propertyName);

            return _properties[propertyName].ToBytes();
        }

        public IDictionary<string, string> AsDictionary(string propertyName, IDictionary<string, string> defaultValue = null)
        {
            if (!_properties.ContainsKey(propertyName)) return defaultValue ?? throw new MissingPropertyException(propertyName);

            return JsonConvert.DeserializeObject<IDictionary<string, string>>(_properties[propertyName]);
        }
    }
}