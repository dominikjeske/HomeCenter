using CSharpFunctionalExtensions;
using HomeCenter.Model.Core;
using System.Collections.Generic;

namespace HomeCenter.Model.ValueTypes
{

    public class UIntValue : ValueObject, IValue
    {
        public UIntValue(uint value = 0) => Value = value;

        public uint Value
        {
            get;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator UIntValue(uint value) => new UIntValue(value);

        public static implicit operator uint(UIntValue value) => value.Value;

        public override string ToString() => Value.ToString();

        public static UIntValue FromString(string value) => new UIntValue(uint.Parse(value));

        public bool HasValue => true;
    }
}