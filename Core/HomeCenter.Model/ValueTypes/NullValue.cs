using HomeCenter.Model.Core;

namespace HomeCenter.Model.ValueTypes
{
    public class NullValue : IValue
    {
        public static IValue Value = new NullValue();

        public bool HasValue => false;
    }
}