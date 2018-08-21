using HomeCenter.ComponentModel.Commands;

namespace HomeCenter.Model.Queries.Specialized
{
    public class DiscoverQuery : Query
    {
        public static DiscoverQuery Query(string deviceUid) => new DiscoverQuery(deviceUid);

        public DiscoverQuery(string uid)
        {
            Uid = uid;
        }
    }
}