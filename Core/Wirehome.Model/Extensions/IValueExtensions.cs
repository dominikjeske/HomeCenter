using System;
using System.Collections.Generic;
using HomeCenter.ComponentModel.ValueTypes;

namespace HomeCenter.Model.Extensions
{
    public static class IValueExtensions
    {
        public static double AsDouble(this IValue value) => (value ?? throw new ArgumentNullException(nameof(value))) is DoubleValue val ? val.Value :
                   throw new InvalidCastException($"Cannot cast from type {value.GetType().Name} to {typeof(double).Name}");

        public static string AsString(this IValue value) => (value ?? throw new ArgumentNullException(nameof(value))) is StringValue val ? val.Value :
                   throw new InvalidCastException($"Cannot cast from type {value.GetType().Name} to {typeof(string).Name}");

        public static int AsInt(this IValue value) => (value ?? throw new ArgumentNullException(nameof(value))) is IntValue val ? val.Value :
                   throw new InvalidCastException($"Cannot cast from type {value.GetType().Name} to {typeof(int).Name}");

        public static TimeSpan AsTimeSpan(this IValue value) => (value ?? throw new ArgumentNullException(nameof(value))) is TimeSpanValue val ? val.Value :
                   throw new InvalidCastException($"Cannot cast from type {value.GetType().Name} to {typeof(TimeSpan).Name}");

        public static IEnumerable<string> AsStringList(this IValue value) => (value ?? throw new ArgumentNullException(nameof(value))) is StringListValue val ? val.Value :
                   throw new InvalidCastException($"Cannot cast from type {value.GetType().Name} to {typeof(IEnumerable<string>).Name}");

        public static TimeSpan AsIntTimeSpan(this IValue value) => (value ?? throw new ArgumentNullException(nameof(value))) is IntValue val ? TimeSpan.FromMilliseconds(val.Value) :
                   throw new InvalidCastException($"Cannot cast from type {value.GetType().Name} to {typeof(double).Name}");
    }
}