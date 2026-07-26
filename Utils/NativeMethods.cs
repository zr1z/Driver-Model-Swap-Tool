using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Driver_Model_Swap_Util
{
	[SupportedOSPlatform("windows")]
	internal static class NativeMethods
	{
		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
	}
}
