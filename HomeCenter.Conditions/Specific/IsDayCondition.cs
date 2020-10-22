using System;
using HomeCenter.Abstractions;
using HomeCenter.Messages.Queries.Device;

namespace HomeCenter.Conditions.Specific
{
    public class IsDayCondition : TimeRangeCondition
    {
        public IsDayCondition(IMessageBroker messageBroker)
        {
            WithStart(() => messageBroker.QueryService<SunriseQuery, TimeSpan?>(SunriseQuery.Default));

            WithEnd(() => messageBroker.QueryService<SunsetQuery, TimeSpan?>(SunsetQuery.Default));
        }
    }
}