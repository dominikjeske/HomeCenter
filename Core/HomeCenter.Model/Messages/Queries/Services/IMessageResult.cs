namespace HomeCenter.Model.Messages.Queries.Services
{
    public interface IMessageResult<T, R>
    {
        bool Verify(T input, R expectedResult);
    }
}