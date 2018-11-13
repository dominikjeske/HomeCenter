namespace HomeCenter.Services.Controllers
{
    public class ControllerOptions : IControllerOptions
    {
        public string AdapterMode { get; set; }
        public int? RemoteActorPort { get; set; }
        public string? RemoteActorAddress { get; set; }
    }
}