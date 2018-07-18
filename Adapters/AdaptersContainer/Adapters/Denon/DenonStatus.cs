using Wirehome.Core;

namespace Wirehome.ComponentModel.Adapters.Denon
{
    public class DenonStatus
    {
        public string ActiveInput { get; set; }
        public bool PowerStatus { get; set; }
        public double? MasterVolume { get; set; }
        public bool Mute { get; set; }
    }
}