using HomeCenter.Core;

namespace HomeCenter.ComponentModel.Adapters.Denon
{
    internal class DenonStatus
    {
        public string ActiveInput { get; set; }
        public bool PowerStatus { get; set; }
        public double? MasterVolume { get; set; }
        public bool Mute { get; set; }
    }
}