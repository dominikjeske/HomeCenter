using HomeCenter.ComponentModel.ValueTypes;

namespace HomeCenter.ComponentModel.Components
{
    public interface IValueConverter
    {
        IValue Convert(IValue old);
        IValue ConvertBack(IValue old);
    }
}
