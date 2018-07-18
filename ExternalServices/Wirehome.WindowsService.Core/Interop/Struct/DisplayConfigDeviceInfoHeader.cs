using System.Runtime.InteropServices;
using Wirehome.WindowsService.Interop;

namespace Wirehome.WindowsService.Interop
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