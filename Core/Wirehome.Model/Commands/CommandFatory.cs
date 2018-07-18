using Wirehome.ComponentModel.ValueTypes;

namespace Wirehome.ComponentModel.Commands
{
    public static class CommandFatory
    {
        public static Command RefreshCommand = new Command(CommandType.RefreshCommand);
        public static Command TurnOnCommand = new Command(CommandType.TurnOnCommand);
        public static Command TurnOffCommand = new Command(CommandType.TurnOffCommand);
        public static Command DiscoverCapabilitiesCommand = new Command(CommandType.DiscoverCapabilities);
        public static Command SupportedCapabilitiesCommand = new Command(CommandType.SupportedCapabilitiesCommand);
        public static Command GetSunriseCommand = new Command(CommandType.GetSunriseCommand);
        public static Command GetSunsetCommand = new Command(CommandType.GetSunsetCommand);

        public static Command GetComponentCommand(string uid) => new Command(CommandType.GetComponentCommand, new Property(CommandProperties.DeviceUid, new StringValue(uid)));
    }
}