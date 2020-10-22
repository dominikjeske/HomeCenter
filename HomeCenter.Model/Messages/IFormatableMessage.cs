namespace HomeCenter.Models.Messages
{
    public interface IFormatableMessage<T>
    {
        T FormatMessage();
    }
}