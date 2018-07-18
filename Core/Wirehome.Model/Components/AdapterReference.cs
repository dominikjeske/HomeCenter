using Wirehome.ComponentModel.Commands;

namespace Wirehome.ComponentModel.Components
{
    public class AdapterReference : BaseObject
    {
        public DeviceCommand GetDeviceCommand(Command baseCommand)
        {
            var command = new DeviceCommand(baseCommand.Type, Uid);

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
