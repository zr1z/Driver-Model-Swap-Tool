using System;
using System.IO;
using System.Runtime.Versioning;

namespace Driver_Model_Swap_Util.Utils
{
	internal static class ContentPathFinder
	{
		// Find the Assetto Corsa content folder by checking multiple locations
		public static string GetAssettoCorsaContentPath()
		{
			// First, check if running within the content directory structure
			string baseDirectory = AppContext.BaseDirectory;
			string currentDirectory = Environment.CurrentDirectory;

			string contentPath = FindContentFromStart(baseDirectory);
			if (!string.IsNullOrEmpty(contentPath))
			{
				ConsoleUtils.Log(LogLevel.Debug, $"Found content path from base directory: {contentPath}");
				return contentPath;
			}

			contentPath = FindContentFromStart(currentDirectory);
			if (!string.IsNullOrEmpty(contentPath))
			{
				ConsoleUtils.Log(LogLevel.Debug, $"Found content path from current directory: {contentPath}");
				return contentPath;
			}

			// Try Steam installation paths
			if (OperatingSystem.IsWindows())
			{
				contentPath = SteamInstallCheck.TryGetAssettoCorsaPathFromSteam();
				if (!string.IsNullOrEmpty(contentPath))
				{
					return contentPath;
				}

				// Try finding from uninstall registry
				string installLocation = RegistryHelper.FindInstallLocationFromUninstallRegistry("Assetto Corsa");
				if (!string.IsNullOrEmpty(installLocation))
				{
					contentPath = TryGetContentPathFromInstallLocation(installLocation);
					if (!string.IsNullOrEmpty(contentPath))
					{
						return contentPath;
					}
				}
			}

			// Try common default paths
			contentPath = TryCommonInstallPaths();
			if (!string.IsNullOrEmpty(contentPath))
			{
				return contentPath;
			}

			ConsoleUtils.Log(LogLevel.Error, "Assetto Corsa content folder not found. Please ensure the game is installed.");
			return null;
		}

		// Walk up directory tree looking for a content folder with a driver subfolder
		private static string FindContentFromStart(string startPath)
		{
			try
			{
				DirectoryInfo currentDir = new DirectoryInfo(Path.GetFullPath(startPath));

				// Walk up directory tree looking for content folder
				while (currentDir != null)
				{
					// Check if current directory is named 'content' and has a 'driver' folder
					if (string.Equals(currentDir.Name, "content", StringComparison.OrdinalIgnoreCase))
					{
						string driverPath = Path.Combine(currentDir.FullName, "driver");
						if (Directory.Exists(driverPath))
						{
							return currentDir.FullName;
						}
					}

					// Check for assettocorsa/content structure
					string acContentPath = Path.Combine(currentDir.FullName, "assettocorsa", "content");
					if (Directory.Exists(acContentPath))
					{
						return acContentPath;
					}

					// Check for content/driver in current directory
					string contentPath = Path.Combine(currentDir.FullName, "content");
					if (Directory.Exists(contentPath))
					{
						string driverPath = Path.Combine(contentPath, "driver");
						if (Directory.Exists(driverPath))
						{
							return contentPath;
						}
					}

					currentDir = currentDir.Parent;
				}
			}
			catch (Exception ex)
			{
				ConsoleUtils.Log(LogLevel.Debug, $"Error searching for content directory from {startPath}", ex);
			}

			return null;
		}

		// Check if the install location has a valid content folder
		private static string TryGetContentPathFromInstallLocation(string installLocation)
		{
			try
			{
				string contentPath = Path.Combine(installLocation, "content");
				if (Directory.Exists(contentPath))
				{
					ConsoleUtils.Log(LogLevel.Info, $"Found content at: {contentPath}");
					return contentPath;
				}

				string nestedPath = Path.Combine(installLocation, "assettocorsa", "content");
				if (Directory.Exists(nestedPath))
				{
					ConsoleUtils.Log(LogLevel.Info, $"Found content at: {nestedPath}");
					return nestedPath;
				}
			}
			catch (Exception ex)
			{
				ConsoleUtils.Log(LogLevel.Debug, $"Error checking install location: {installLocation}", ex);
			}

			return null;
		}

		// Check some common installation paths as a last resort
		private static string TryCommonInstallPaths()
		{
			string[] commonPaths = new[]
			{
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), 
					"Steam", "steamapps", "common", "assettocorsa", "content"),
				Path.Combine("D:\\SteamLibrary", "steamapps", "common", "assettocorsa", "content")
			};

			foreach (string path in commonPaths)
			{
				if (Directory.Exists(path))
				{
					ConsoleUtils.Log(LogLevel.Info, $"Found Assetto Corsa at common path: {path}");
					return path;
				}
			}

			return null;
		}
	}
}