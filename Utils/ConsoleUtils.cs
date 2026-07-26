using System;
using System.IO;
using System.Threading;

namespace Driver_Model_Swap_Util.Utils
{
	internal static class ConsoleUtils
	{
		private static LogLevel _currentLogLevel = LogLevel.Info;
		private static readonly object _lock = new object();
		private static string _logFilePath = Path.Combine(AppContext.BaseDirectory, "driver-swap-log.txt");
		private static long _maxLogFileSize = 5242880L;

		// Set up logging settings and clear old log file
		public static void ConfigureLogger(LogLevel logLevel, string logFilePath = "driver-swap-log.txt", long maxLogFileSize = 5242880L)
		{
			_currentLogLevel = logLevel;
			_logFilePath = Path.Combine(AppContext.BaseDirectory, logFilePath);
			_maxLogFileSize = maxLogFileSize;

			try
			{
				if (File.Exists(_logFilePath))
				{
					File.Delete(_logFilePath);
				}
			}
			catch
			{
				// Ignore if we can't delete the log file
			}
		}

		// Log a message without an exception
		public static void Log(LogLevel level, string message)
		{
			Log(level, message, null);
		}

		// Main logging method - writes to console with colors and to file
		public static void Log(LogLevel level, string message, Exception ex)
		{
			if (level < _currentLogLevel)
			{
				return;
			}

			lock (_lock)
			{
				string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
				string formattedMessage = $"[{timestamp}] [{level,-5}] {message}";

				// Write to console with colors
				ConsoleColor originalColor = Console.ForegroundColor;
				try
				{
					Console.ForegroundColor = GetColorForLevel(level);
					Console.WriteLine(formattedMessage);

					// Write exception details if provided
					if (ex != null)
					{
						Console.ForegroundColor = ConsoleColor.Gray;
						Console.WriteLine($"         Exception: {ex.GetType().Name}: {ex.Message}");
						
						if (!string.IsNullOrEmpty(ex.StackTrace))
						{
							Console.WriteLine("         Stack Trace:");
							foreach (string line in ex.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
							{
								if (!string.IsNullOrWhiteSpace(line))
								{
									Console.WriteLine($"           {line}");
								}
							}
						}

						if (ex.InnerException != null)
						{
							Console.ForegroundColor = ConsoleColor.DarkGray;
							Console.WriteLine($"         Inner Exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
						}
					}
				}
				finally
				{
					Console.ForegroundColor = originalColor;
				}

				// Write to file
				WriteToFile(formattedMessage, ex);
			}
		}

		// Append log entry to file, rotate if file gets too big
		private static void WriteToFile(string message, Exception ex)
		{
			try
			{
				if (File.Exists(_logFilePath) && new FileInfo(_logFilePath).Length > _maxLogFileSize)
				{
					RotateLogFile();
				}

				using (StreamWriter writer = new StreamWriter(_logFilePath, append: true))
				{
					writer.WriteLine(message);
					
					if (ex != null)
					{
						writer.WriteLine($"         Exception: {ex.GetType().Name}: {ex.Message}");
						writer.WriteLine($"         Stack Trace: {ex.StackTrace}");
						
						if (ex.InnerException != null)
						{
							writer.WriteLine($"         Inner Exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
						}
					}
				}
			}
			catch
			{
				// If we can't write to file, silently fail
			}
		}

		// Archive current log file with timestamp and start fresh
		private static void RotateLogFile()
		{
			try
			{
				string backupPath = Path.Combine(
					Path.GetDirectoryName(_logFilePath),
					$"{Path.GetFileNameWithoutExtension(_logFilePath)}_backup_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(_logFilePath)}"
				);
				File.Move(_logFilePath, backupPath);
			}
			catch
			{
				// If rotation fails, just continue - don't block logging
			}
		}

		// Pick console color based on log level
		private static ConsoleColor GetColorForLevel(LogLevel level)
		{
			return level switch
			{
				LogLevel.Trace => ConsoleColor.Gray,
				LogLevel.Debug => ConsoleColor.Cyan,
				LogLevel.Info => ConsoleColor.Green,
				LogLevel.Warn => ConsoleColor.Yellow,
				LogLevel.Error => ConsoleColor.Red,
				LogLevel.Fatal => ConsoleColor.Magenta,
				_ => ConsoleColor.White
			};
		}

		// Wait for user input or auto-close after 10 seconds
		public static void PauseExit()
		{
			Console.WriteLine();
			Console.WriteLine("Press any key to close or wait 10 seconds...");
			
			for (int i = 10; i > 0; i--)
			{
				if (Console.KeyAvailable)
				{
					Console.ReadKey(intercept: true);
					return;
				}

				Console.Write($"\rClosing automatically in {i} second{(i > 1 ? "s" : "")}... ");
				Thread.Sleep(1000);
			}

			Console.WriteLine();
		}
	}
}