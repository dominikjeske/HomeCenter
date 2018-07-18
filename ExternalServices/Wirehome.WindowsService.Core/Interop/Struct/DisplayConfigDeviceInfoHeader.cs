using System.Runtime.InteropServices;
using HomeCenter.WindowsService.Interop;

namespace HomeCenter.WindowsService.Interop
{
	[StructLayout(LayoutKind.Sequential)]
	public struct DisplayConfigDeviceInfoHeader
	{
		public DisplayConfigDeviceInfoType type;
		public int size;
		public LUID adapterId;
		public uint id;
	}
}