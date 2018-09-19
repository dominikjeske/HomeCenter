using CSharpFunctionalExtensions;
using HomeCenter.Model.Core;
using System.Collections.Generic;

namespace HomeCenter.ComponentModel.ValueTypes
{
    public class IntValue : ValueObject, IValue
    {
        public IntValue(int value = 0) => Value = value;

        public int Value
        {
            get;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator IntValue(int value) => new IntValue(value);

        public static implicit operator int(IntValue value) => value.Value;

        public override string ToString() => Value.ToString();

        public static IntValue FromString(string value) => new IntValue(int.Parse(value));

        public bool HasValue => true;
    }
}