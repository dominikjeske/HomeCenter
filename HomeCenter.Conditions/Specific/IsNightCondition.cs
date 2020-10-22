using System;
using HomeCenter.Abstractions;
using HomeCenter.Messages.Queries.Device;

namespace HomeCenter.Conditions.Specific
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