using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Driver_Model_Swap_Util.Utils
{
	[SupportedOSPlatform("windows")]
	internal static class SteamInstallCheck
	{
		private static readonly Regex PathRegex = new("\\\"path\\\"\\s*\\\"([^\\\"]+)\\\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private static readonly Regex NumericRegex = new("\\\"\\d+\\\"\\s*\\\"([A-Za-z]:\\\\[^\\\"]+)\\\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private static readonly Regex InstallDirRegex = new("\\\"installdir\\\"\\s*\\\"([^\\\"]+)\\\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		// Find where Steam is installed by checking the registry
		public static string? GetSteamPath()
		{
			try
			{
				RegistryView[] views = { RegistryView.Registry64, RegistryView.Registry32 };

				foreach (RegistryView view in views)
				{
					string? steamPath = RegistryHelper.ReadRegistryString(RegistryHive.CurrentUser, "Software\\Valve\\Steam", "SteamPath", view);
					if (!string.IsNullOrEmpty(steamPath) && Directory.Exists(steamPath))
					{
						ConsoleUtils.Log(LogLevel.Debug, $"Found Steam path from registry: {steamPath}");
						return steamPath;
					}

					steamPath = RegistryHelper.ReadRegistryString(RegistryHive.LocalMachine, "Software\\WOW6432Node\\Valve\\Steam", "InstallPath", view);
					if (!string.IsNullOrEmpty(steamPath) && Directory.Exists(steamPath))
					{
						ConsoleUtils.Log(LogLevel.Debug, $"Found Steam path from WOW6432Node: {steamPath}");
						return steamPath;
					}

					steamPath = RegistryHelper.ReadRegistryString(RegistryHive.LocalMachine, "Software\\Valve\\Steam", "InstallPath", view);
					if (!string.IsNullOrEmpty(steamPath) && Directory.Exists(steamPath))
					{
						ConsoleUtils.Log(LogLevel.Debug, $"Found Steam path from Software\\Valve\\Steam: {steamPath}");
						return steamPath;
					}

					steamPath = RegistryHelper.ReadRegistryString(RegistryHive.LocalMachine, "Software\\Valve\\Steam", "SteamPath", view);
					if (!string.IsNullOrEmpty(steamPath) && Directory.Exists(steamPath))
					{
						ConsoleUtils.Log(LogLevel.Debug, $"Found Steam path from Software\\Valve\\Steam SteamPath: {steamPath}");
						return steamPath;
					}
				}
			}
			catch (Exception ex)
			{
				ConsoleUtils.Log(LogLevel.Debug, "Error reading Steam path from registry", ex);
			}

			return null;
		}

		// Parse the libraryfolders.vdf file to get all Steam library locations
		public static List<string> GetSteamLibraryFolders(string steamPath)
		{
			List<string> libraryFolders = [];

			if (string.IsNullOrEmpty(steamPath))
			{
				return libraryFolders;
			}

			try
			{
				string libraryFoldersPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");

				if (!File.Exists(libraryFoldersPath))
				{
					ConsoleUtils.Log(LogLevel.Debug, $"libraryfolders.vdf not found at: {libraryFoldersPath}");
					return libraryFolders;
				}

				string content = File.ReadAllText(libraryFoldersPath);

				foreach (Match match in PathRegex.Matches(content))
				{
					if (match.Groups.Count > 1)
					{
						string path = match.Groups[1].Value.Replace("\\\\", "\\");

						if (Directory.Exists(path) && !libraryFolders.Any(f => f.Equals(path, StringComparison.OrdinalIgnoreCase)))
						{
							libraryFolders.Add(path);
							ConsoleUtils.Log(LogLevel.Debug, $"Found Steam library folder: {path}");
						}
					}
				}

				foreach (Match match in NumericRegex.Matches(content))
				{
					if (match.Groups.Count > 1)
					{
						string path = match.Groups[1].Value.Replace("\\\\", "\\");

						if (Directory.Exists(path) && !libraryFolders.Any(f => f.Equals(path, StringComparison.OrdinalIgnoreCase)))
						{
							libraryFolders.Add(path);
							ConsoleUtils.Log(LogLevel.Debug, $"Found Steam library folder: {path}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				ConsoleUtils.Log(LogLevel.Debug, "Error reading Steam library folders", ex);
			}

			return libraryFolders;
		}

		// Find Assetto Corsa by checking Steam library folders and app manifests
		public static string? TryGetAssettoCorsaPathFromSteam()
		{
			string? steamPath = GetSteamPath();

			if (string.IsNullOrEmpty(steamPath))
			{
				ConsoleUtils.Log(LogLevel.Debug, "Steam path not found in registry");
				return null;
			}

			List<string> libraryFolders = GetSteamLibraryFolders(steamPath);
			libraryFolders.Insert(0, steamPath);

			foreach (string folder in libraryFolders)
			{
				try
				{
					string standardPath = Path.Combine(folder, "steamapps", "common", "assettocorsa", "content");

					if (Directory.Exists(standardPath))
					{
						ConsoleUtils.Log(LogLevel.Info, $"Found Assetto Corsa at: {standardPath}");
						return standardPath;
					}

					string manifestPath = Path.Combine(folder, "steamapps", "appmanifest_244210.acf");

					if (File.Exists(manifestPath))
					{
						string? installDir = ParseInstallDirFromManifest(manifestPath);

						if (!string.IsNullOrEmpty(installDir))
						{
							string customPath = Path.Combine(folder, "steamapps", "common", installDir, "content");

							if (Directory.Exists(customPath))
							{
								ConsoleUtils.Log(LogLevel.Info, $"Found Assetto Corsa at custom path: {customPath}");
								return customPath;
							}
						}
					}
				}
				catch (Exception ex)
				{
					ConsoleUtils.Log(LogLevel.Debug, $"Error checking Steam library folder: {folder}", ex);
				}
			}

			return null;
		}

		// Read the install directory from a Steam app manifest file
		private static string? ParseInstallDirFromManifest(string manifestPath)
		{
			try
			{
				string content = File.ReadAllText(manifestPath);
				Match match = InstallDirRegex.Match(content);

				if (match.Success && match.Groups.Count > 1)
				{
					return match.Groups[1].Value;
				}
			}
			catch (Exception ex)
			{
				ConsoleUtils.Log(LogLevel.Debug, $"Error parsing manifest file: {manifestPath}", ex);
			}

			return null;
		}
	}
}