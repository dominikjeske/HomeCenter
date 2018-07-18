using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace Wirehome.ComponentModel.ValueTypes
{
    public class TimeSpanValue : ValueObject, IValue
    {
        public TimeSpanValue(TimeSpan value = default) => Value = value;

        public TimeSpan Value
        {
            get;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator TimeSpanValue(TimeSpan value) => new TimeSpanValue(value);
        public static implicit operator TimeSpan?(TimeSpanValue value) => value == null ? (TimeSpan?)null : value.Value;

        public override string ToString() => Value.ToString();
        public bool HasValue => true;
    }    
}
