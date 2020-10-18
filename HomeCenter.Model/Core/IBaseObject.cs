namespace HomeCenter.Model.Core
{
    public interface IBaseObject
    {
        string Type { get; }
        string Uid { get; }
    }
}