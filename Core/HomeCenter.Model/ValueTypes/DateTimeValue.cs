using CSharpFunctionalExtensions;
using HomeCenter.Model.Core;
using System;
using System.Collections.Generic;

namespace HomeCenter.Model.ValueTypes
{
    public class DateTimeValue : ValueObject, IValue
    {
        public DateTimeValue(DateTimeOffset value = default) => Value = value;

        public DateTimeOffset Value
        {
            get;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator DateTimeValue(DateTimeOffset value) => new DateTimeValue(value);

        public static implicit operator DateTimeOffset(DateTimeValue value) => value.Value;

        public override string ToString() => Value.ToString();

        public bool HasValue => true;
    }
}