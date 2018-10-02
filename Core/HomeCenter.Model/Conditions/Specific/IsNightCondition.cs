using HomeCenter.Broker;

namespace HomeCenter.Model.Conditions.Specific
{
    public class IsNightCondition : TimeRangeCondition
    {
        public IsNightCondition(IEventAggregator eventAggregator)
        {
            //TODO
            //WithStart(async () =>
            //{
            //    var result = await eventAggregator.QueryForValueType(SunsetQuery.Default, ConditionProperies.EndTime).ConfigureAwait(false);
            //    return result.HasValue ? (TimeSpan?)result.AsTimeSpan() : null;
            //});
            //WithEnd(async () =>
            //{
            //    var result = await eventAggregator.QueryForValueType(SunriseQuery.Default, ConditionProperies.EndTime).ConfigureAwait(false);
            //    return result.HasValue ? (TimeSpan?)result.AsTimeSpan() : null;
            //});
        }
    }
}