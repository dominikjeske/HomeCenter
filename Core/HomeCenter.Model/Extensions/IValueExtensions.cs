using HomeCenter.Model.Core;
using HomeCenter.Model.ValueTypes;
using System;
using System.Collections.Generic;

namespace HomeCenter.Model.Extensions
{
    public static class IValueExtensions
    {
        public static double AsDouble(this IValue value) => (value ?? throw new ArgumentNullException(nameof(value))) is DoubleValue val ? val.Value :
                   throw new InvalidCastException($"Cannot cast from type {value.GetType().Name} to {typeof(double).Name}");

        public static bool AsBool(this IValue value, BooleanValue defaultValue = null) => (value ?? defaultValue ?? throw new ArgumentNullException(nameof(value))) is BooleanValue val ? val.Value :
                   throw new InvalidCastException($"Cannot cast from type {value.GetType().Name} to {typeof(double).Name}");

        public static string AsString(this IValue value) => (value ?? throw new ArgumentNullException(nameof(value))) is StringValue val ? val.Value :
                   throw new InvalidCastException($"Cannot cast from type {value.GetType().Name} to {typeof(string).Name}");

        public static int AsInt(this IValue value) => (value ?? throw new ArgumentNullException(nameof(value))) is IntValue val ? val.Value :
                   throw new InvalidCastException($"Cannot cast from type {value.GetType().Name} to {typeof(int).Name}");

        public static uint AsUInt(this IValue value) => (value ?? throw new ArgumentNullException(nameof(value))) is UIntValue val ? val.Value :
                   throw new InvalidCastException($"Cannot cast from type {value.GetType().Name} to {typeof(uint).Name}");

        public static byte AsByte(this IValue value) => (value ?? throw new ArgumentNullException(nameof(value))) is ByteValue val ? val.Value :
           throw new InvalidCastException($"Cannot cast from type {value.GetType().Name} to {typeof(byte).Name}");

        public static TimeSpan AsTimeSpan(this IValue value) => (value ?? throw new ArgumentNullException(nameof(value))) is TimeSpanValue val ? val.Value :
                   throw new InvalidCastException($"Cannot cast from type {value.GetType().Name} to {typeof(TimeSpan).Name}");

        public static IEnumerable<string> AsStringList(this IValue value) => (value ?? throw new ArgumentNullException(nameof(value))) is StringListValue val ? val.Value :
                   throw new InvalidCastException($"Cannot cast from type {value.GetType().Name} to {typeof(IEnumerable<string>).Name}");

        public static TimeSpan AsIntTimeSpan(this IValue value) => (value ?? throw new ArgumentNullException(nameof(value))) is IntValue val ? TimeSpan.FromMilliseconds(val.Value) :
                   throw new InvalidCastException($"Cannot cast from type {value.GetType().Name} to {typeof(double).Name}");
    }
}