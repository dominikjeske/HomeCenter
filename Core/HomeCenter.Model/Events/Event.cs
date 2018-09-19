using HomeCenter.Core.Extensions;
using HomeCenter.Model.Core;
using System.Collections.Generic;

namespace HomeCenter.ComponentModel.Events
{
    public class Event : BaseObject, System.IEquatable<Event>
    {
        public Event()
        {
            SupressPropertyChangeEvent = true;
        }

        public bool Equals(Event other)
        {
            if (other == null || Type.Compare(other.Type) != 0 || !ToProperiesList().LeftEqual(other.ToProperiesList())) return false;

            return true;
        }

        public virtual IEnumerable<string> RoutingAttributes() => GetPropetiesKeys();
    }
}