using Wirehome.ComponentModel.ValueTypes;

namespace Wirehome.ComponentModel.Components
{
    public interface IValueConverter
    {
        IValue Convert(IValue old);
        IValue ConvertBack(IValue old);
    }
}
