namespace HomeCenter.Services.Controllers
{
    public class ControllerOptions
    {
        public string AdapterMode { get; set; }
        public int? RemoteActorPort { get; set; }
        public string RemoteActorAddress { get; set; }
        public string Configuration { get; set; }
        public string AdapterRepoName { get; set; }
    }
}