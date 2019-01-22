using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Queries.Device;
using System;

namespace HomeCenter.Model.Conditions.Specific
{
    public class IsNightCondition : TimeRangeCondition
    {
        public IsNightCondition(IMessageBroker messageBroker)
        {
            WithStart(() => messageBroker.QueryService<SunsetQuery, TimeSpan?>(SunsetQuery.Default));
            WithEnd(() => messageBroker.QueryService<SunriseQuery, TimeSpan?>(SunriseQuery.Default));
        }
    }
}