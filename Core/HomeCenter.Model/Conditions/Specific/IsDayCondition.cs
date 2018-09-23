using HomeCenter.Model.Messages.Commands;
using HomeCenter.Messaging;
using HomeCenter.Model.Conditions;
using HomeCenter.Model.Extensions;

namespace HomeCenter.Conditions.Specialized
{
    public class IsDayCondition : TimeRangeCondition
    {
        public IsDayCondition(IEventAggregator eventAggregator)
        {
            //WithStart(async () => (await eventAggregator.QueryForValueType(CommandFatory.GetSunriseCommand, ConditionProperies.StartTime).ConfigureAwait(false)).ToTimeSpanValue());
            //WithEnd(async () => (await eventAggregator.QueryForValueType(CommandFatory.GetSunsetCommand, ConditionProperies.EndTime).ConfigureAwait(false)).ToTimeSpanValue());
        }
    }
}