namespace HomeCenter.Services.Configuration
{
    public interface IControllerOptions
    {
        AdapterMode AdapterMode { get; set; }
        int? RemoteActorPort { get; set; }
        string? RemoteActorAddress { get; set; }
    }
}