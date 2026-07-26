using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace Driver_Model_Swap_Util.Utils
{
	internal static class AdminUtils
	{
		// Check if the current process is running with admin rights
		[SupportedOSPlatform("windows")]
		public static bool IsRunAsAdministrator()
		{
			using (WindowsIdentity ntIdentity = WindowsIdentity.GetCurrent())
			{
				return new WindowsPrincipal(ntIdentity).IsInRole(WindowsBuiltInRole.Administrator);
			}
		}

		// Restart the application with admin privileges using UAC prompt
		[SupportedOSPlatform("windows")]
		public static void Elevate()
		{
			string exePath = Process.GetCurrentProcess().MainModule?.FileName;
			if (string.IsNullOrEmpty(exePath))
			{
				ConsoleUtils.Log(LogLevel.Warn, "Could not get executable path for elevation.");
				return;
			}

			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = exePath,
				UseShellExecute = true,
				Verb = "runas"
			};

			try
			{
				Process.Start(startInfo);
			}
			catch (System.ComponentModel.Win32Exception ex)
			{
				ConsoleUtils.Log(LogLevel.Warn, "User declined elevation prompt.", ex);
			}
		}
	}
}