namespace HomeCenter.Services.Configuration
{
    public class ControllerOptions : IControllerOptions
    {
        public AdapterMode AdapterMode { get; set; }
        public int? RemoteActorPort { get; set; }
        public string? RemoteActorAddress { get; set; }
    }
}