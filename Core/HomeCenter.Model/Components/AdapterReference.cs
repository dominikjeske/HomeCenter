using HomeCenter.Broker;
using HomeCenter.Model.Core;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Events.Device;
using System.Collections.Generic;

namespace HomeCenter.Model.Components
{
    public class AdapterReference : BaseObject
    {
        public RoutingFilter GetRoutingFilter(IList<string> requierdProperties)
        {
            var routerAttributes = new Dictionary<string, string>();
            foreach (var adapterProperty in requierdProperties)
            {
                if (!ContainsProperty(adapterProperty)) throw new ConfigurationException($"Adapter {Uid} in component {Uid} missing configuration property {adapterProperty}");
                routerAttributes.Add(adapterProperty, this[adapterProperty]);
            }
            routerAttributes.Add(MessageProperties.MessageSource, Uid);
            routerAttributes.Add(MessageProperties.EventType, nameof(PropertyChangedEvent));

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