using System;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Messaging;
using HomeCenter.Model.Conditions;
using HomeCenter.Model.Extensions;

namespace HomeCenter.Conditions.Specialized
{
    public class IsNightCondition : TimeRangeCondition
    {
        public IsNightCondition(IEventAggregator eventAggregator)
        {
            WithStart(async () =>
            {
                var result = await eventAggregator.QueryForValueType(CommandFatory.GetSunsetCommand, ConditionProperies.EndTime).ConfigureAwait(false);
                return result.HasValue ? (TimeSpan?)result.AsTimeSpan() : null;
            });
            WithEnd(async () =>
            {
                var result = await eventAggregator.QueryForValueType(CommandFatory.GetSunriseCommand, ConditionProperies.EndTime).ConfigureAwait(false);
                return result.HasValue ? (TimeSpan?)result.AsTimeSpan() : null;
            });
        }
    }
}