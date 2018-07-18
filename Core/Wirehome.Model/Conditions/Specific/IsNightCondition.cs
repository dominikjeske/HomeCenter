using System;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core.EventAggregator;
using Wirehome.Model.Conditions;
using Wirehome.Model.Extensions;

namespace Wirehome.Conditions.Specialized
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