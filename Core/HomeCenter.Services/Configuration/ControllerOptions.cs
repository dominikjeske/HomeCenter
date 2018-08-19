namespace HomeCenter.Core.Services.DependencyInjection
{
    public class ControllerOptions : IControllerOptions
    {
        public AdapterMode AdapterMode { get; set; }
        public int? HttpServerPort { get; set; }
    }
}