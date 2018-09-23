using CSharpFunctionalExtensions;
using HomeCenter.Model.Core;
using System.Collections.Generic;

namespace HomeCenter.Model.ValueTypes
{
    public class ByteValue : ValueObject, IValue
    {
        public ByteValue(byte value = 0) => Value = value;

        public ByteValue Value
        {
            get;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator ByteValue(byte value) => new ByteValue(value);

        public static implicit operator byte(ByteValue value) => value.Value;

        public override string ToString() => Value.ToString();

        public static ByteValue FromString(string value) => new ByteValue(byte.Parse(value));

        public bool HasValue => true;
    }
}