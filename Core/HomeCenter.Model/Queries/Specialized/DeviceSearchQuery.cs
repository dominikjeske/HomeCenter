using HomeCenter.ComponentModel.Commands;

namespace HomeCenter.Model.Queries.Specialized
{

    public class DeviceSearchQuery : Query
    {
        public static DeviceSearchQuery Query(string deviceUid) => new DeviceSearchQuery(deviceUid);

        public DeviceSearchQuery(string uid)
        {
            Uid = uid;
        }
    }
}