using HomeCenter.Broker;
using HomeCenter.Model.Core;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.ValueTypes;
using System;

namespace HomeCenter.Model.Conditions.Specific
{
    public class IsNightCondition : TimeRangeCondition
    {
        public IsNightCondition(IEventAggregator eventAggregator)
        {
            WithStart(async () =>
            {
                var value = await eventAggregator.QueryAsync<Query, BaseObject>(SunsetQuery.Default).ConfigureAwait(false);
                var result = value?[ConditionProperies.StartTime] ?? NullValue.Value;

                return result.HasValue ? (TimeSpan?)result.AsTimeSpan() : null;
            });
            WithEnd(async () =>
            {
                var value = await eventAggregator.QueryAsync<Query, BaseObject>(SunsetQuery.Default).ConfigureAwait(false);
                var result = value?[ConditionProperies.EndTime] ?? NullValue.Value;

                return result.HasValue ? (TimeSpan?)result.AsTimeSpan() : null;
            });
        }
    }
}