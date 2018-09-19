using CSharpFunctionalExtensions;
using HomeCenter.Model.Core;
using System.Collections.Generic;

namespace HomeCenter.ComponentModel.ValueTypes
{
    public class BooleanValue : ValueObject, IValue
    {
        public static BooleanValue TrueValue = new BooleanValue(true);
        public static BooleanValue FalseValue = new BooleanValue(false);

        public BooleanValue(bool value = default) => Value = value;

        public bool Value
        {
            get;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator BooleanValue(bool value) => new BooleanValue(value);

        public static implicit operator bool(BooleanValue value) => value.Value;

        public override string ToString() => Value.ToString();

        public bool HasValue => true;
    }
}