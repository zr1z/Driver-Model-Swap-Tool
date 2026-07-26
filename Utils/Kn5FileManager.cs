using System;
using System.IO;
using System.Linq;

namespace Driver_Model_Swap_Util.Utils
{
	internal static class Kn5FileManager
	{
		// Look for a single KN5 file in the given directory
		public static string? FindSourceKn5(string directory)
		{
			try
			{
				string[] kn5Files = Directory.GetFiles(directory, "*.kn5", SearchOption.TopDirectoryOnly);

				if (kn5Files.Length == 0)
				{
					ConsoleUtils.Log(LogLevel.Error, "No KN5 file found in the executable directory.");
					ConsoleUtils.Log(LogLevel.Info, "Place a KN5 file next to this executable to begin.");
					return null;
				}

				if (kn5Files.Length > 1)
				{
					ConsoleUtils.Log(LogLevel.Error, "Multiple KN5 files found. Only one is allowed:");

					foreach (string file in kn5Files)
					{
						ConsoleUtils.Log(LogLevel.Info, $"  - {Path.GetFileName(file)}");
					}

					return null;
				}

				string sourceFile = kn5Files.First();
				ConsoleUtils.Log(LogLevel.Info, $"Source KN5 file: {Path.GetFileName(sourceFile)}");
				return sourceFile;
			}
			catch (Exception ex)
			{
				ConsoleUtils.Log(LogLevel.Error, $"Error searching for KN5 files in {directory}", ex);
				return null;
			}
		}

		// Replace all KN5 files in the main driver folder with the source file
		public static int ReplaceKn5Files(string? sourceKn5)
		{
			if (string.IsNullOrEmpty(sourceKn5) || !File.Exists(sourceKn5))
			{
				ConsoleUtils.Log(LogLevel.Error, $"Source KN5 file not found: {sourceKn5}");
				return 0;
			}

			string? contentPath = ContentPathFinder.GetAssettoCorsaContentPath();

			if (string.IsNullOrEmpty(contentPath))
			{
				ConsoleUtils.Log(LogLevel.Error, "Assetto Corsa content path could not be determined.");
				return 0;
			}

			string driverPath = Path.Combine(contentPath, "driver");

			if (!Directory.Exists(driverPath))
			{
				ConsoleUtils.Log(LogLevel.Error, $"Driver folder not found: {driverPath}");
				return 0;
			}

			try
			{
				string[] driverKn5Files = Directory.GetFiles(driverPath, "*.kn5", SearchOption.TopDirectoryOnly);

				if (driverKn5Files.Length == 0)
				{
					ConsoleUtils.Log(LogLevel.Warn, "No KN5 files found in driver folder.");
					return 0;
				}

				int replacedCount = 0;

				foreach (string targetFile in driverKn5Files)
				{
					try
					{
						File.Copy(sourceKn5, targetFile, true);
						replacedCount++;
						ConsoleUtils.Log(LogLevel.Debug, $"Replaced: {Path.GetFileName(targetFile)}");
					}
					catch (Exception ex)
					{
						ConsoleUtils.Log(LogLevel.Warn, $"Failed to replace {Path.GetFileName(targetFile)}", ex);
					}
				}

				return replacedCount;
			}
			catch (Exception ex)
			{
				ConsoleUtils.Log(LogLevel.Error, "Error replacing KN5 files in driver folder", ex);
				return 0;
			}
		}

		// Go through all car folders and replace driver KN5 files in their driver/drivers subfolders
		public static int ReplaceDriverKn5InCars(string? sourceKn5)
		{
			if (string.IsNullOrEmpty(sourceKn5) || !File.Exists(sourceKn5))
			{
				ConsoleUtils.Log(LogLevel.Error, $"Source KN5 file not found: {sourceKn5}");
				return 0;
			}

			string? contentPath = ContentPathFinder.GetAssettoCorsaContentPath();

			if (string.IsNullOrEmpty(contentPath))
			{
				ConsoleUtils.Log(LogLevel.Error, "Assetto Corsa content path could not be determined.");
				return 0;
			}

			string carsPath = Path.Combine(contentPath, "cars");

			if (!Directory.Exists(carsPath))
			{
				ConsoleUtils.Log(LogLevel.Error, $"Cars folder not found: {carsPath}");
				return 0;
			}

			try
			{
				int replacedCount = 0;

				foreach (string carDir in Directory.GetDirectories(carsPath))
				{
					try
					{
						replacedCount += ReplaceDriverKn5InCarDriverFolder(sourceKn5, carDir);
					}
					catch (Exception ex)
					{
						ConsoleUtils.Log(LogLevel.Debug, $"Error processing car directory {Path.GetFileName(carDir)}", ex);
					}
				}

				return replacedCount;
			}
			catch (Exception ex)
			{
				ConsoleUtils.Log(LogLevel.Error, "Error replacing KN5 files in car driver folders", ex);
				return 0;
			}
		}

		// Replace KN5 files in a single car's driver folder
		private static int ReplaceDriverKn5InCarDriverFolder(string sourceKn5, string carDirectory)
		{
			int replacedCount = 0;
			string carName = Path.GetFileName(carDirectory);

			foreach (string folderName in new[] { "driver", "drivers" })
			{
				string driverPath = Path.Combine(carDirectory, folderName);

				if (!Directory.Exists(driverPath))
					continue;

				try
				{
					string[] driverKn5Files = Directory.GetFiles(driverPath, "*.kn5", SearchOption.TopDirectoryOnly);

					if (driverKn5Files.Length == 0)
					{
						ConsoleUtils.Log(LogLevel.Debug, $"No KN5 files found in {carName}/{folderName}");
						continue;
					}

					foreach (string targetFile in driverKn5Files)
					{
						try
						{
							File.Copy(sourceKn5, targetFile, true);
							replacedCount++;
							ConsoleUtils.Log(LogLevel.Debug, $"Replaced: {carName}/{folderName}/{Path.GetFileName(targetFile)}");
						}
						catch (Exception ex)
						{
							ConsoleUtils.Log(LogLevel.Warn, $"Failed to replace {Path.GetFileName(targetFile)} in {carName}", ex);
						}
					}
				}
				catch (Exception ex)
				{
					ConsoleUtils.Log(LogLevel.Debug, $"Error processing {folderName} folder in car {carName}", ex);
				}
			}

			return replacedCount;
		}
	}
}