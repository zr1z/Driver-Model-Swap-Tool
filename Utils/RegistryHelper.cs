using System;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace Driver_Model_Swap_Util.Utils
{
	[SupportedOSPlatform("windows")]
	internal static class RegistryHelper
	{
		// Read a string value from the Windows registry
		public static string? ReadRegistryString(RegistryHive hive, string subKey, string valueName, RegistryView view)
		{
			try
			{
				using RegistryKey? baseKey = RegistryKey.OpenBaseKey(hive, view);
				using RegistryKey? subKeyObj = baseKey.OpenSubKey(subKey);

				return subKeyObj?.GetValue(valueName) as string;
			}
			catch
			{
				return null;
			}
		}

		// Search the uninstall registry entries to find where a program is installed
		public static string? FindInstallLocationFromUninstallRegistry(string displayNameFilter)
		{
			try
			{
				RegistryKey[] registryKeys =
				{
					Registry.LocalMachine,
					Registry.CurrentUser
				};

				string[] uninstallPaths =
				{
					"Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall",
					"Software\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall"
				};

				foreach (RegistryKey registryKey in registryKeys)
				{
					foreach (string uninstallPath in uninstallPaths)
					{
						using RegistryKey? uninstallKey = registryKey.OpenSubKey(uninstallPath);

						if (uninstallKey == null)
						{
							continue;
						}

						foreach (string subKeyName in uninstallKey.GetSubKeyNames())
						{
							try
							{
								using RegistryKey? appKey = uninstallKey.OpenSubKey(subKeyName);

								if (appKey == null)
								{
									continue;
								}

								string? displayName = appKey.GetValue("DisplayName") as string;

								if (!string.IsNullOrEmpty(displayName) &&
									displayName.Contains(displayNameFilter, StringComparison.OrdinalIgnoreCase))
								{
									string? installLocation = appKey.GetValue("InstallLocation") as string;

									if (!string.IsNullOrEmpty(installLocation))
									{
										// ConsoleUtils.Log(LogLevel.Debug, $"Found install location from registry: {installLocation}");
										return installLocation;
									}
								}
							}
							catch
							{
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ConsoleUtils.Log(LogLevel.Debug, "Error searching uninstall registry", ex);
			}

			return null;
		}
	}
}