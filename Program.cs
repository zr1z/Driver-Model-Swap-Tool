using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Driver_Model_Swap_Util.Utils;

namespace Driver_Model_Swap_Util
{
	internal class Program
	{
		[STAThread]
		[SupportedOSPlatform("windows")]
		private static void Main()
		{
			Console.Title = "KN5 Driver Replacer Tool";
			ConsoleUtils.ConfigureLogger(LogLevel.Debug, "driver-swap-log.txt", 5242880L);

			ConsoleUtils.Log(LogLevel.Info, "========================================");
			ConsoleUtils.Log(LogLevel.Info, " KN5 Driver Replacer Tool ");
			ConsoleUtils.Log(LogLevel.Info, "========================================");

			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

			// Check for admin rights and elevate if needed
			if (!AdminUtils.IsRunAsAdministrator())
			{
				ConsoleUtils.Log(LogLevel.Warn, "Not running as Administrator. Attempting to elevate...");
				AdminUtils.Elevate();
				return;
			}

			// Locate the Assetto Corsa content folder
			string assettoCorsaContentPath = ContentPathFinder.GetAssettoCorsaContentPath();
			if (string.IsNullOrEmpty(assettoCorsaContentPath))
			{
				ConsoleUtils.Log(LogLevel.Error, "Failed to locate Assetto Corsa content folder.");
				ConsoleUtils.PauseExit();
				return;
			}

			// Copy the exe to the content folder and relaunch if needed
			if (!IsRunningInContentFolder(baseDirectory, assettoCorsaContentPath))
			{
				if (!RelaunchiExecutableInContentFolder(baseDirectory, assettoCorsaContentPath))
				{
					ConsoleUtils.PauseExit();
					return;
				}
			}

			// Let user select the source KN5 file
			ConsoleUtils.Log(LogLevel.Debug, "Attempting to open file selection dialog...");
			
			// Minimize console window to make dialog more visible
			System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
			IntPtr consoleHandle = currentProcess.MainWindowHandle;
			if (consoleHandle != IntPtr.Zero)
			{
				// ShowMinimized = 2
				NativeMethods.ShowWindow(consoleHandle, 2);
				Thread.Sleep(500); // Give time for window to minimize
			}

			string? sourceKn5 = FileDialogHelper.SelectKn5File(baseDirectory);
			
			// Restore console window
			if (consoleHandle != IntPtr.Zero)
			{
				// ShowNormal = 1
				NativeMethods.ShowWindow(consoleHandle, 1);
			}

			if (string.IsNullOrEmpty(sourceKn5))
			{
				ConsoleUtils.Log(LogLevel.Error, "No KN5 file was selected.");
				ConsoleUtils.PauseExit();
				return;
			}

			// Verify driver folder exists
			string driverPath = Path.Combine(assettoCorsaContentPath, "driver");
			if (!Directory.Exists(driverPath))
			{
				ConsoleUtils.Log(LogLevel.Error, $"Driver folder not found at: {driverPath}");
				ConsoleUtils.PauseExit();
				return;
			}

			ConsoleUtils.Log(LogLevel.Info, $"Driver folder: {driverPath}");
			ConsoleUtils.Log(LogLevel.Info, "Starting driver model replacement...");

			try
			{
				// Replace all driver KN5 files in the main driver folder
				int replacedCount = Kn5FileManager.ReplaceKn5Files(sourceKn5);
				ConsoleUtils.Log(LogLevel.Info, $"Replaced {replacedCount} driver KN5 files.");
				ConsoleUtils.Log(LogLevel.Info, "Replacing driver models in car skins (background task)...");

				// Run car skin replacement in the background so it doesn't block the main thread
				Task.Run(() =>
				{
					try
					{
						int skinReplacedCount = Kn5FileManager.ReplaceDriverKn5InCars(sourceKn5);
						ConsoleUtils.Log(LogLevel.Info, $"Car skin replacement completed: {skinReplacedCount} files updated.");
					}
					catch (Exception ex)
					{
						ConsoleUtils.Log(LogLevel.Error, "Error during car skin replacement", ex);
					}
				});
			}
			catch (Exception ex)
			{
				ConsoleUtils.Log(LogLevel.Error, "Unexpected error during replacement", ex);
			}
			finally
			{
				ConsoleUtils.Log(LogLevel.Info, "Operation completed.");
				ConsoleUtils.PauseExit();
			}
		}

		// Check if we're already running from the content folder
		private static bool IsRunningInContentFolder(string baseDirectory, string contentPath)
		{
			string normalizedBase = Path.GetFullPath(baseDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string normalizedContent = Path.GetFullPath(contentPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			return string.Equals(normalizedBase, normalizedContent, StringComparison.OrdinalIgnoreCase);
		}

		// Copy the exe to the content folder and relaunch from there
		private static bool RelaunchiExecutableInContentFolder(string baseDirectory, string contentPath)
		{
			string normalizedContent = Path.GetFullPath(contentPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			ConsoleUtils.Log(LogLevel.Info, $"AC content folder located at: {normalizedContent}");

			string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? AppContext.BaseDirectory;
			if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
			{
				ConsoleUtils.Log(LogLevel.Error, "Could not determine executable path.");
				return false;
			}

			string exeName = Path.GetFileName(exePath);
			string targetPath = Path.Combine(normalizedContent, exeName);

			try
			{
				File.Copy(exePath, targetPath, overwrite: true);
				ConsoleUtils.Log(LogLevel.Info, $"Copied executable to: {targetPath}");

				Process.Start(new ProcessStartInfo
				{
					FileName = targetPath,
					UseShellExecute = true
				});
				return true;
			}
			catch (Exception ex)
			{
				ConsoleUtils.Log(LogLevel.Error, $"Failed to copy and relaunch executable", ex);
				ConsoleUtils.Log(LogLevel.Info, "Place this EXE into the Assetto Corsa 'content' folder and run it from there.");
				return false;
			}
		}
	}
}