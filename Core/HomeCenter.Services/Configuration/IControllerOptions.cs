namespace HomeCenter.Core.Services.DependencyInjection
{
    public interface IControllerOptions
    {
        AdapterMode AdapterMode { get; set; }
        int? HttpServerPort { get; set; }
    }
}