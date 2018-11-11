using HomeCenter.Model.Core;
using HomeCenter.Utils.Extensions;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Model.Conditions.Specific
{
    public class TimeRangeCondition : Condition
    {
        private Func<Task<TimeSpan?>> _startValueProvider = null;
        private Func<Task<TimeSpan?>> _endValueProvider = null;

        public TimeRangeCondition WithStart(Func<Task<TimeSpan?>> start)
        {
            _startValueProvider = start;
            return this;
        }

        public TimeRangeCondition WithEnd(Func<Task<TimeSpan?>> end)
        {
            _endValueProvider = end;
            return this;
        }

        public TimeRangeCondition WithStart(TimeSpan start)
        {
            SetProperty(ConditionProperies.StartTime, start);
            return this;
        }

        public TimeRangeCondition WithEnd(TimeSpan end)
        {
            SetProperty(ConditionProperies.EndTime, end);
            return this;
        }

        public TimeRangeCondition WithStartAdjustment(TimeSpan value)
        {
            SetProperty(ConditionProperies.StartAdjustment, value);
            return this;
        }

        public TimeRangeCondition WithEndAdjustment(TimeSpan value)
        {
            SetProperty(ConditionProperies.EndAdjustment, value);
            return this;
        }

        public override async Task<bool> Validate()
        {
            TimeSpan? startValue = ContainsProperty(ConditionProperies.StartTime) ? AsTime(ConditionProperies.StartTime) : await _startValueProvider().ConfigureAwait(false);
            TimeSpan? endValue = ContainsProperty(ConditionProperies.EndTime) ? AsTime(ConditionProperies.EndTime) : await _endValueProvider().ConfigureAwait(false);

            if (!startValue.HasValue || !endValue.HasValue) return false;

            startValue += AsTime(ConditionProperies.StartAdjustment, TimeSpan.Zero);
            endValue += AsTime(ConditionProperies.EndAdjustment, TimeSpan.Zero);

            return SystemTime.Now.TimeOfDay.IsTimeInRange(startValue.Value, endValue.Value);
        }
    }
}