namespace HomeCenter.Services.Configuration
{
    public interface IControllerOptions
    {
        AdapterMode AdapterMode { get; set; }
        int? HttpServerPort { get; set; }
    }
}