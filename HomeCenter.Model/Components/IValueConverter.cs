using HomeCenter.Model.Core;

namespace HomeCenter.Model.Components
{
    public interface IValueConverter
    {
        string Convert(string old);

        string ConvertBack(string old);
    }
}