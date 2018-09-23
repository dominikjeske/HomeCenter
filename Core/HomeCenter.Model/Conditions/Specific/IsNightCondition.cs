using HomeCenter.Messaging;
using HomeCenter.Model.Conditions;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Queries.Device;
using System;

namespace HomeCenter.Conditions.Specialized
{
    public class IsNightCondition : TimeRangeCondition
    {
        public IsNightCondition(IEventAggregator eventAggregator)
        {
            WithStart(async () =>
            {
                var result = await eventAggregator.QueryForValueType(SunsetQuery.Default, ConditionProperies.EndTime).ConfigureAwait(false);
                return result.HasValue ? (TimeSpan?)result.AsTimeSpan() : null;
            });
            WithEnd(async () =>
            {
                var result = await eventAggregator.QueryForValueType(SunriseQuery.Default, ConditionProperies.EndTime).ConfigureAwait(false);
                return result.HasValue ? (TimeSpan?)result.AsTimeSpan() : null;
            });
        }
    }
}