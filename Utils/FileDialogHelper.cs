using System;
using System.IO;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace Driver_Model_Swap_Util.Utils
{
	[SupportedOSPlatform("windows")]
	internal static class FileDialogHelper
	{
		private static bool _initialized = false;

		// Show a file picker dialog for selecting a KN5 file
		public static string? SelectKn5File(string initialDirectory)
		{
			try
			{
				// Initialize Windows Forms application context (only once)
				if (!_initialized)
				{
					Application.EnableVisualStyles();
					_initialized = true;
					ConsoleUtils.Log(LogLevel.Debug, "Initialized Windows Forms application context");
				}

				// Ensure initial directory is valid
				if (string.IsNullOrEmpty(initialDirectory) || !Directory.Exists(initialDirectory))
				{
					ConsoleUtils.Log(LogLevel.Debug, $"Initial directory invalid: {initialDirectory}, using current directory");
					initialDirectory = Environment.CurrentDirectory;
				}

				using (OpenFileDialog dialog = new OpenFileDialog())
				{
					dialog.Title = "Select Driver Model KN5 File";
					dialog.Filter = "KN5 Files (*.kn5)|*.kn5|All Files (*.*)|*.*";
					dialog.FilterIndex = 1;
					dialog.InitialDirectory = initialDirectory;
					dialog.CheckFileExists = true;
					dialog.CheckPathExists = true;
					dialog.Multiselect = false;
					dialog.RestoreDirectory = true;

					ConsoleUtils.Log(LogLevel.Debug, $"Opening file selection dialog from: {initialDirectory}");

					DialogResult result = dialog.ShowDialog();

					if (result == DialogResult.OK)
					{
						ConsoleUtils.Log(LogLevel.Info, $"Selected KN5 file: {dialog.FileName}");
						return dialog.FileName;
					}

					ConsoleUtils.Log(LogLevel.Info, "File selection cancelled by user.");
					return null;
				}
			}
			catch (Exception ex)
			{
				ConsoleUtils.Log(LogLevel.Error, "Error opening file selection dialog", ex);
				return null;
			}
		}
	}
}