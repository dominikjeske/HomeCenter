using HomeCenter.Model.Exceptions;
using HomeCenter.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace HomeCenter.Model.Core
{
    public class BaseObject
    {
        [Map] private Dictionary<string, string> _properties { get; set; } = new Dictionary<string, string>();
        public string Uid { get; protected set; } = "";
        public string Type { get; set; } = "";
        public List<string> Tags { get; private set; } = new List<string>();

        public BaseObject()
        {
        }

        public BaseObject(IDictionary<string, string> properties)
        {
            foreach (var property in properties)
            {
                SetProperty(property.Key, property.Value);
            }
        }

        public override string ToString() => $"Type: [{Type}] | Uid: [{Uid}] | Properties: [{GetProperties()?.ToFormatedString()}] | Tags: [{Tags.ToFormatedString()}]";

        public bool ContainsProperty(string propertyName) => _properties.ContainsKey(propertyName);

        public void SetProperty(string propertyName, string value) => _properties[propertyName] = value;

        public void SetProperty(string propertyName, DateTimeOffset value) => _properties[propertyName] = value.ToString(); //TODO check this

        public void SetProperty(string propertyName, TimeSpan value) => _properties[propertyName] = value.ToString(); //TODO check this

        public void SetProperty(string propertyName, int value) => _properties[propertyName] = value.ToString(); //TODO check this

        public void SetProperty(string propertyName, double value) => _properties[propertyName] = value.ToString(); //TODO check this

        public void SetProperty(string propertyName, bool value) => _properties[propertyName] = value.ToString(); //TODO check this

        public void SetProperty(string propertyName, byte[] value) => _properties[propertyName] = BitConverter.ToString(value); //TODO check this

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

            throw new WrongPropertyFormatException($"Property {propertyName} value {_properties[propertyName]} is not proper int value");
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

        public IList<string> AsList(string propertyName, IList<string> defaultValue = null)
        {
            if (!_properties.ContainsKey(propertyName)) return defaultValue ?? throw new MissingPropertyException(propertyName);

            return _properties[propertyName].Split(',').Select(x => x.Trim()).ToList();
        }

        public byte[] AsByteArray(string propertyName, byte[] defaultValue = null)
        {
            if (!_properties.ContainsKey(propertyName)) return defaultValue ?? throw new MissingPropertyException(propertyName);

            return Encoding.UTF8.GetBytes(_properties[propertyName]);
        }
    }
}