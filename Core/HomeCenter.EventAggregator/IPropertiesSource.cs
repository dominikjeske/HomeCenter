namespace HomeCenter.Broker
{
    public interface IPropertiesSource
    {
        string this[string propertyName] { get; set; }

        bool ContainsProperty(string propertyName);
    }
}