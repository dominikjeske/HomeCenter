namespace HomeCenter.Model.Messages
{
    public interface IFormatableMessage<T>
    {
        T FormatMessage();
    }
}