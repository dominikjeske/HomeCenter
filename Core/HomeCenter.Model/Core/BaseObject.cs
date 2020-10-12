using HomeCenter.Broker;
using HomeCenter.Model.Messages;
using HomeCenter.Utils.Extensions;
using System.Collections.Generic;

namespace HomeCenter.Model.Core
{
    public abstract class BaseObject : IPropertySource, IBaseObject
    {
        [Map] private Dictionary<string, string> _properties { get; set; } = new Dictionary<string, string>();

        [Map]
        public string Uid
        {
            get => this.AsString(MessageProperties.Uid, GetType().Name);
            init => this.SetProperty(MessageProperties.Uid, value);
        }

        [Map]
        public string Type
        {
            get => this.AsString(MessageProperties.Type);
            init => this.SetProperty(MessageProperties.Type, value);
        }

        public BaseObject()
        {
            Type = GetType().Name;
        }

        public string this[string propertyName]
        {
            get
            {
                if (!ContainsProperty(propertyName)) throw new KeyNotFoundException($"Property {propertyName} not found on component {Uid}");
                return _properties[propertyName];
            }
            set 
            {
                _properties[propertyName] = value; 
            }
        }

        public override string ToString() => GetProperties()?.ToFormatedString() ?? string.Empty;

        public bool ContainsProperty(string propertyName) => _properties.ContainsKey(propertyName);

        public IReadOnlyDictionary<string, string> GetProperties() => _properties.AsReadOnly();
    }
}