using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using Wirehome.ComponentModel.ValueTypes;

namespace Wirehome.ComponentModel
{
    public sealed class Property : ValueObject
    {
        public string Key { get; set; }
        public IValue Value { get; set; }
        public override string ToString() => $"{Key}={Convert.ToString(Value)}";

        public Property() { }

        public Property(string type, IValue value)
        {
            Key = type;
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Key;
        }
    }
}
