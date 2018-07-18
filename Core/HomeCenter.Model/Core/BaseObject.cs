using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using HomeCenter.ComponentModel.Events;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Core.Extensions;
using HomeCenter.Core.Services.DependencyInjection;

namespace HomeCenter.ComponentModel
{
    public class BaseObject
    {
        private readonly Subject<Event> _events = new Subject<Event>();
        [Map] private Dictionary<string, Property> _properties { get; set; } = new Dictionary<string, Property>();
        protected bool SupressPropertyChangeEvent { get; set; }
        public string Uid { get; protected set; }
        public string Type { get; set; }
        public List<string> Tags { get; private set; } = new List<string>();
        public IObservable<Event> Events => _events.AsObservable();

        public IReadOnlyDictionary<string, Property> ToProperiesList() => _properties.AsReadOnly();

        public BaseObject()
        {
        }

        public BaseObject(params Property[] properties)
        {
            foreach (var property in properties)
            {
                SetPropertyValue(property.Key, property.Value);
            }
        }

        public IValue this[string propertyName]
        {
            get
            {
                var value = GetPropertyValue(propertyName);
                if (!value.HasValue) throw new KeyNotFoundException($"Property {propertyName} not found on component {Uid}");
                return value;
            }
            set { SetPropertyValue(propertyName, value); }
        }

        public bool ContainsProperty(string propertyName) => _properties.ContainsKey(propertyName);

        public virtual IValue GetPropertyValue(string propertyName, IValue defaultValue = null)
        {
            if (!_properties.ContainsKey(propertyName))
            {
                return defaultValue ?? NullValue.Value;
            }
            else
            {
                return _properties[propertyName].Value;
            }
        }

        public virtual void SetPropertyValue(string propertyName, IValue value)
        {
            Property property;
            if (!_properties.ContainsKey(propertyName))
            {
                property = new Property
                {
                    Key = propertyName
                };
                _properties.Add(propertyName, property);
            }
            else
            {
                property = _properties[propertyName];
            }
            var oldValue = property.Value;
            property.Value = value;

            if (SupressPropertyChangeEvent || value.Equals(oldValue)) return;

            _events.OnNext(new PropertyChangedEvent(Uid, property.Key, oldValue, value));
        }

        public IDictionary<string, string> GetPropertiesStrings() => _properties.Values.ToDictionary(k => k.Key, v => v.Value?.ToString());
    }
}