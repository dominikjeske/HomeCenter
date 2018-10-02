using HomeCenter.Broker;

namespace HomeCenter.Model.Conditions.Specific
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