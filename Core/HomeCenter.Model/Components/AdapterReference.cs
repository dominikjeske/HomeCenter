using HomeCenter.Broker;
using HomeCenter.Model.Core;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Events;
using Proto;
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
                routerAttributes.Add(adapterProperty, this[adapterProperty].ToString());
            }
            routerAttributes.Add(MessageProperties.MessageSource, Uid);

            var routingKey = routerAttributes[MessageProperties.MessageSource];
            routerAttributes.Add(EventProperties.EventType, EventType.PropertyChanged);

            return new RoutingFilter(routingKey, routerAttributes);
        }

        public Command GetDeviceCommand(Command baseCommand)
        {
            var command = new Command(baseCommand.Type, Uid);

            // copy properties from base command
            foreach (var prop in baseCommand.ToProperiesList())
            {
                command.SetPropertyValue(prop.Key, prop.Value.Value);
            }

            // add properties from adapter reference
            foreach (var prop in ToProperiesList())
            {
                command.SetPropertyValue(prop.Key, prop.Value.Value);
            }

            return command;
        }
    }
}