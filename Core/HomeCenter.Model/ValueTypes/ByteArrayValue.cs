using CSharpFunctionalExtensions;
using HomeCenter.Model.Core;
using System.Collections.Generic;
using System.Text;

namespace HomeCenter.Model.ValueTypes
{
    public class ByteArrayValue : ValueObject, IValue
    {
        public ByteArrayValue(byte[] value) => Value = value;

        public byte[] Value
        {
            get;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator ByteArrayValue(byte[] value) => new ByteArrayValue(value);

        public static implicit operator byte[](ByteArrayValue value) => value.Value;

        public override string ToString() => Value.ToString();

        public static ByteArrayValue FromString(string value) => new ByteArrayValue(Encoding.UTF8.GetBytes(value));

        public bool HasValue => true;
    }
}