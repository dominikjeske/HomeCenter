using System;
using System.Threading.Tasks;
using HomeCenter.Abstractions;
using HomeCenter.Extensions;

namespace HomeCenter.Conditions.Specific
{
    public class TimeRangeCondition : Condition
    {
        private Func<Task<TimeSpan>>? _startValueProvider;
        private Func<Task<TimeSpan>>? _endValueProvider;

        public TimeRangeCondition WithStart(Func<Task<TimeSpan>> start)
        {
            _startValueProvider = start;
            return this;
        }

        public TimeRangeCondition WithEnd(Func<Task<TimeSpan>> end)
        {
            _endValueProvider = end;
            return this;
        }

        public TimeRangeCondition WithStart(TimeSpan start)
        {
            this.SetProperty(ConditionProperies.StartTime, start);
            return this;
        }

        public TimeRangeCondition WithEnd(TimeSpan end)
        {
            this.SetProperty(ConditionProperies.EndTime, end);
            return this;
        }

        public TimeRangeCondition WithStartAdjustment(TimeSpan value)
        {
            this.SetProperty(ConditionProperies.StartAdjustment, value);
            return this;
        }

        public TimeRangeCondition WithEndAdjustment(TimeSpan value)
        {
            this.SetProperty(ConditionProperies.EndAdjustment, value);
            return this;
        }

        public override async Task<bool> Validate()
        {
            var startValue = this.AsTime(ConditionProperies.StartTime, _startValueProvider != null ? await _startValueProvider() : throw new InvalidOperationException());
            var endValue = this.AsTime(ConditionProperies.EndTime, _endValueProvider != null ? await _endValueProvider() : throw new InvalidOperationException());

            startValue += this.AsTime(ConditionProperies.StartAdjustment, TimeSpan.Zero);
            endValue += this.AsTime(ConditionProperies.EndAdjustment, TimeSpan.Zero);

            return SystemTime.Now.TimeOfDay.IsTimeInRange(startValue, endValue);
        }
    }
}