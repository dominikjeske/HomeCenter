namespace HomeCenter.Services.Configuration
{
    public class ControllerOptions : IControllerOptions
    {
        public AdapterMode AdapterMode { get; set; }
        public int? HttpServerPort { get; set; }
    }
}