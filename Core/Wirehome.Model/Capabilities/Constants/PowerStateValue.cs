using Wirehome.Core;
using Wirehome.Core.Extensions;

namespace Wirehome.ComponentModel.Capabilities.Constants
{
    public static class PowerStateValue
    {
        public const string ON = "ON";
        public const string OFF = "OFF";

        public static bool ToBinaryState(string powerStringValue) => powerStringValue.Compare(ON) == 0 ? true : false;
    }
}
