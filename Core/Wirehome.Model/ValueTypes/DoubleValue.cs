using CSharpFunctionalExtensions;
using System.Collections.Generic;

namespace Wirehome.ComponentModel.ValueTypes
{
    public class DoubleValue : ValueObject, IValue
    {
        public DoubleValue(double value = default) => Value = value;

        public double Value
        {
            get;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator DoubleValue(double value) => new DoubleValue(value);
        public static implicit operator double(DoubleValue value) => value.Value;
        public override string ToString() => Value.ToString();
        public bool HasValue => true;
    }    
}
