﻿using HomeCenter.ComponentModel.Adapters;
using HomeCenter.ComponentModel.Commands;

namespace HomeCenter.ComponentModel.Components
{
    public class AdapterReference : BaseObject
    {
        public Adapter Adapter { get; private set; }

        public void InitializeAdapter(Adapter adapter)
        {
            Adapter = adapter;
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