namespace HomeCenter.Broker
{
    public interface IPropertySource
    {
        object this[string propertyName] { get; set; }

        bool ContainsProperty(string propertyName);
    }
}