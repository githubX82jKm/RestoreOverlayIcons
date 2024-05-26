using Microsoft.Extensions.Logging;
using OverlayIcons;
using RestoreOverlayIcons.Logging;
using System;
using System.IO;
using System.Reflection;

namespace RestoreOverlayIcons;

class Program
{
	static int Main(string[] args)
	{
		ILogger logger = Global.LoggerFactory.CreateLogger<Program>();

		try
		{
			if (args.Length == 0)
			{
				logger.LogError("Als Argument 1 Pfad auf die settings.json angeben!");
				return 1;
			}

			string settingsFilePath = args[0];
			if (!File.Exists(settingsFilePath))
			{
				logger.LogError("Settings.json wurde hier nicht gefunden: {s}", settingsFilePath);
				return 2;
			}

			logger.LogInformation("{name} {version}", Assembly.GetEntryAssembly()?.GetName().Name, Assembly.GetEntryAssembly()?.GetName().Version);

			OverlayIconManager olim = new(logger, settingsFilePath);

			olim.Execute();

			logger.LogInformation("Ausführung beendet.");

			return 0;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Fehler bei der Ausführung");
			return 1;
		}
	}
}
