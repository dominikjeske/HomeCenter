namespace HomeCenter.Services.Controllers
{
    public interface IControllerOptions
    {
        string AdapterMode { get; set; }
        int? RemoteActorPort { get; set; }
        string? RemoteActorAddress { get; set; }
    }
}