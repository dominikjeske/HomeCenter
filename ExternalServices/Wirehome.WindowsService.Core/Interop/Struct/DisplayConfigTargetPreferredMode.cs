using System.Runtime.InteropServices;

namespace Wirehome.WindowsService.Interop
{
	[StructLayout(LayoutKind.Sequential)]
	public struct DisplayConfigTargetPreferredMode : IDisplayConfigInfo
	{
		public DisplayConfigDeviceInfoHeader header;
		public uint width;
		public uint height;
		public DisplayConfigTargetMode targetMode;
	}
}