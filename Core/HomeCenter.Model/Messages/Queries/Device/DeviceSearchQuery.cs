namespace HomeCenter.Model.Messages.Queries.Device
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