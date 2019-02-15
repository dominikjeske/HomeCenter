using HomeCenter.Model.Core;
using System.Linq;

namespace HomeCenter.Model.Messages.Queries.Device
{
    public class DiscoverQuery : Query
    {
        public static DiscoverQuery Default = new DiscoverQuery();

        public static DiscoverQuery CreateQuery(BaseObject parent)
        {
            var query = new DiscoverQuery();
            foreach (var property in parent.GetProperties().Where(p => !string.IsNullOrWhiteSpace(p.Value)))
            {
                query[property.Key] = property.Value;
            }
            return query;
        }
    }
}