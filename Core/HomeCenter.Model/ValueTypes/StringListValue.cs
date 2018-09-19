using CSharpFunctionalExtensions;
using HomeCenter.Model.Core;
using System.Collections.Generic;
using System.Linq;

namespace HomeCenter.ComponentModel.ValueTypes
{
    public class StringListValue : ValueObject, IValue
    {
        public StringListValue(IEnumerable<string> list) => Value = list.ToArray();

        public StringListValue(params string[] value) => Value = value;

        public string[] Value
        {
            get;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => string.Join(", ", Value);

        public bool HasValue => true;
    }
}