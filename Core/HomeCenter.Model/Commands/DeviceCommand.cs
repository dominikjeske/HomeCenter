using HomeCenter.ComponentModel.ValueTypes;

namespace HomeCenter.ComponentModel.Commands
{
    public class DeviceCommand : Command
    {
        public static DeviceCommand GenerateDiscoverCommand(string deviceUid) => new DeviceCommand(CommandType.DiscoverCapabilities, deviceUid);

        public DeviceCommand(string commandType, string uid)
        {
            Type = commandType;
            Uid = uid;
        }
    }
}