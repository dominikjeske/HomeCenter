using System.Collections.Generic;
using HomeCenter.Abstractions;
using HomeCenter.EventAggregator;
using HomeCenter.Messages.Events.Device;

namespace HomeCenter.Model.Components
{
    public class AdapterReference : BaseObject
    {
        public List<string> RequierdProperties { get; } = new List<string>();

        public bool IsMainAdapter { get; set; }

        public void AddRequierdProperties(IList<string> requierdProperties)
        {
            RequierdProperties.AddRange(requierdProperties);
        }

        public RoutingFilter GetRoutingFilter()
        {
            var routerAttributes = new Dictionary<string, object>();
            foreach (var adapterProperty in RequierdProperties)
            {
                if (!ContainsProperty(adapterProperty)) throw new ConfigurationException($"Adapter {Uid} in component {Uid} missing configuration property {adapterProperty}");
                routerAttributes.Add(adapterProperty, this[adapterProperty]);
            }
            routerAttributes.Add(MessageProperties.MessageSource, Uid);
            routerAttributes.Add(MessageProperties.Type, nameof(PropertyChangedEvent)); // TODO consider Event

            return new RoutingFilter(Uid, routerAttributes);
        }

        public Command GetDeviceCommand(Command command)
        {
            // add properties from adapter reference
            foreach (var prop in GetProperties())
            {
                if (!command.ContainsProperty(prop.Key))   // If property already exists we leave it. This allow to override by sender
                {
                    command.SetProperty(prop.Key, prop.Value);
                }
            }

            return command;
        }
    }
}