using HomeCenter.Broker;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Model.Messages.Queries.Device;

namespace HomeCenter.Model.Conditions.Specific
{
    public class IsNightCondition : TimeRangeCondition
    {
        public IsNightCondition(IEventAggregator eventAggregator)
        {
            WithStart(async () =>
            {
                var value = await eventAggregator.QueryAsync<Query, BaseObject>(SunsetQuery.Default).ConfigureAwait(false);
                var result = value?.AsTime(ConditionProperies.StartTime, null);

                return result;
            });
            WithEnd(async () =>
            {
                var value = await eventAggregator.QueryAsync<Query, BaseObject>(SunsetQuery.Default).ConfigureAwait(false);
                var result = value?.AsTime(ConditionProperies.EndTime, null);

                return result;
            });
        }
    }
}