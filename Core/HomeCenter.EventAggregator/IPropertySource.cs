namespace HomeCenter.Broker
{
    public interface IPropertySource
    {
        string this[string propertyName] { get; set; }

        bool ContainsProperty(string propertyName);
    }
}