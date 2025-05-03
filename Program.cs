using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using RestoreOverlayIcons.Logging;

namespace RestoreOverlayIcons;

class Program
{
	static int Main(string[] args)
	{
		ILogger logger = Global.LoggerFactory.CreateLogger<Program>();

		int result = 0;

		try
		{
			if (args.Length == 0)
			{
				ShowHelp(logger);
				result = 1;
			}
			else
			{
				string settingsFilePath = args[0];
				if (!File.Exists(settingsFilePath))
				{
					logger.LogError("Settings.json not found: {s}", settingsFilePath);
					result = 2;
				}
				else
				{

					logger.LogInformation(ProgramInfo);

					OverlayIconManager olim = new(logger, settingsFilePath);

					olim.Execute();

				}
			}

			if (result == 0)
				logger.LogInformation("Ready.");
			else
				logger.LogError("Finished with errors.");

			return result;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Finished with errors.");
			return 1;
		}
		finally
		{
			Console.WriteLine("--- Press key to finish ---");
			Console.ReadKey();
		}
	}

	static string ProgramInfo => $"{Assembly.GetEntryAssembly()?.GetName().Name} {Assembly.GetEntryAssembly()?.GetName().Version}";

	static void ShowHelp(ILogger logger)
	{
		Console.WriteLine(ProgramInfo);

		Console.WriteLine("With this program you are able to set the order of Overlay Icons in windows with one click.");
		Console.WriteLine("Problem: Windows supports only up to 15 Overlay Icons in Explorer. Programs as OneDrive");
		Console.WriteLine("often change order without asking!");
		Console.WriteLine("Overlay icons are stored in registry at: HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\ShellOverlayIdentifiers");
		Console.WriteLine();
		Console.WriteLine("Usage: RestoreOverlayIcons.exe <path to settings.json>");
		Console.WriteLine();
		Console.WriteLine("In settings.json, place your favorite Overlay Icons; only names without whitespaces like Tortoise1Normal, Tortoise2Modiefied, ...");
		Console.WriteLine("Execution of this program requires administrator rights!");
	}
}
