using CSharpFunctionalExtensions;
using HomeCenter.Model.Core;
using System.Collections.Generic;

namespace HomeCenter.Model.ValueTypes
{
    public class StringValue : ValueObject, IValue
    {
        public StringValue(string value = default) => Value = value;

        public string Value
        {
            get;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator StringValue(string value) => new StringValue(value);

        public static implicit operator string(StringValue value) => value.Value;

        public override string ToString() => Value;

        public bool HasValue => true;
    }
}