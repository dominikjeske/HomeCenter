namespace HomeCenter.Model.Core
{
    public interface IBaseObject
    {
        string Type { get; set; }
        string Uid { get; set; }
    }
}