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
				logger.LogError("Als Argument 1 Pfad auf die settings.json angeben!");
				result = 1;
			}
			else
			{

				string settingsFilePath = args[0];
				if (!File.Exists(settingsFilePath))
				{
					logger.LogError("Settings.json wurde hier nicht gefunden: {s}", settingsFilePath);
					result = 2;
				}
				else
				{

					logger.LogInformation("{name} {version}", Assembly.GetEntryAssembly()?.GetName().Name, Assembly.GetEntryAssembly()?.GetName().Version);

					OverlayIconManager olim = new(logger, settingsFilePath);

					olim.Execute();

				}
			}

			if (result == 0)
				logger.LogInformation("Ausführung beendet.");
			else
				logger.LogError("Ausführung fehlerhaft beendet.");

			return result;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Fehler bei der Ausführung");
			return 1;
		}
		finally
		{
			Console.WriteLine("--- Taste drücken zum Beenden ---");
			Console.ReadKey();
		}
	}
}
