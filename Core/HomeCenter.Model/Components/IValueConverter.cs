using HomeCenter.Model.Core;

namespace HomeCenter.Model.Components
{
    public interface IValueConverter
    {
        IValue Convert(IValue old);

        IValue ConvertBack(IValue old);
    }
}