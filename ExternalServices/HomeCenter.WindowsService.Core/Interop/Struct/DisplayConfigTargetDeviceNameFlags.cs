using System.Runtime.InteropServices;

namespace HomeCenter.WindowsService.Core.Interop.Struct
{
    [StructLayout(LayoutKind.Explicit)]
    public struct DisplayConfigTargetDeviceNameFlags
    {
        [FieldOffset(0)] private readonly uint _raw;
        [FieldOffset(0)] public uint value;

        /// <summary>
        /// Get first bit from the raw field.
        /// </summary>
        public byte FriendlyNameFromEdid
        {
            get { return (byte)(_raw & (1 << 0)); }
        }

        /// <summary>
        /// Get second bit from the raw field.
        /// </summary>
        public byte FriendlyNameForced
        {
            get { return (byte)(_raw & (1 << 1)); }
        }

        /// <summary>
        /// Get third bit from the raw field.
        /// </summary>
        public byte EdidIdsValid
        {
            get { return (byte)(_raw & (1 << 2)); }
        }

        //public byte Reserved {get{return (byte)((raw>>11)&0x1F);}}
    }
}