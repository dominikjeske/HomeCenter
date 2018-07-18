using System.Runtime.InteropServices;

namespace Wirehome.WindowsService.Interop
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct DisplayConfigSourceDeviceName : IDisplayConfigInfo
	{
		private const int Cchdevicename = 32;

		public DisplayConfigDeviceInfoHeader header;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = Cchdevicename)] public string viewGdiDeviceName;
	}
}