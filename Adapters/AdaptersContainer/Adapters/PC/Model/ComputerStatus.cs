namespace Wirehome.ComponentModel.Adapters.Pc
{
    public class ComputerStatus
    {
        public string ActiveInput { get; set; }
        public bool PowerStatus { get; set; }
        public double? MasterVolume { get; set; }
        public bool Mute { get; set; }
    }
}