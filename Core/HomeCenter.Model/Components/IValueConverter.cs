using HomeCenter.Model.Core;

namespace HomeCenter.ComponentModel.Components
{
    public interface IValueConverter
    {
        IValue Convert(IValue old);

        IValue ConvertBack(IValue old);
    }
}