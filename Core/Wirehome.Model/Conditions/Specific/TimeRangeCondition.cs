using System;
using System.Threading.Tasks;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Core;
using HomeCenter.Model.Conditions;
using HomeCenter.Model.Extensions;

namespace HomeCenter.Conditions.Specialized
{
    public class TimeRangeCondition : Condition
    {
        private Func<Task<TimeSpan?>> _startValueProvider;
        private Func<Task<TimeSpan?>> _endValueProvider;


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
            this[ConditionProperies.StartTime] = new TimeSpanValue(start);
            return this;
        }

        public TimeRangeCondition WithEnd(TimeSpan end)
        {
            this[ConditionProperies.EndTime] = new TimeSpanValue(end);
            return this;
        }

        public TimeRangeCondition WithStartAdjustment(TimeSpan value)
        {
            this[ConditionProperies.StartAdjustment] = new TimeSpanValue(value);
            return this;
        }

        public TimeRangeCondition WithEndAdjustment(TimeSpan value)
        {
            this[ConditionProperies.EndAdjustment] = new TimeSpanValue(value);
            return this;
        }

        public override async Task<bool> Validate()
        {
            var start = GetPropertyValue(ConditionProperies.StartTime);
            var end = GetPropertyValue(ConditionProperies.EndTime);

            var startValue = start.HasValue ? start.AsTimeSpan() : (await _startValueProvider().ConfigureAwait(false));
            var endValue = end.HasValue ? end.AsTimeSpan() : (await _endValueProvider().ConfigureAwait(false));

            if (!startValue.HasValue || !endValue.HasValue) return false;

            startValue += GetPropertyValue(ConditionProperies.StartAdjustment, (TimeSpanValue)TimeSpan.Zero).AsTimeSpan();
            endValue += GetPropertyValue(ConditionProperies.EndAdjustment, (TimeSpanValue)TimeSpan.Zero).AsTimeSpan();

            return SystemTime.Now.TimeOfDay.IsTimeInRange(startValue.Value, endValue.Value);
        }
    }
}
