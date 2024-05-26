using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace OverlayIcons;

public class OverlayIconManager
{
	public OverlayIconManager(ILogger logger, string settingsFilePath)
	{
		Logger = logger;

		string key = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\ShellIconOverlayIdentifiers";

		logger.LogInformation("Öffne Registry-Schlüssel: {key}", key);

		var shellIconOverlayIdentifiersKey = Registry.LocalMachine.OpenSubKey(key, true) ?? 
			throw new Exception($"Registry-Schlüssel konnte nicht geöffnet werden: {key}");

		ShellIconOverlayIdentifiersKey = shellIconOverlayIdentifiersKey;

		logger.LogInformation("Lese Einstellungen aus Settings.json: {s}", settingsFilePath);

		string json = File.ReadAllText(settingsFilePath);
		List<string>? keepTheseKeysInFront = JsonSerializer.Deserialize<List<string>>(json) ?? 
			throw new Exception("Settings.json ist leer!");

		KeepTheseKeysInFront = keepTheseKeysInFront;

		logger.LogInformation("{count} Schlüssel werden vorne einsortiert:", keepTheseKeysInFront.Count);

		int i = 0;
		foreach (string k in KeepTheseKeysInFront)
		{
			i++;
			logger.LogInformation("{i}:{k}", i, k);
		}
	}

	ILogger Logger { get; }

	List<string> KeepTheseKeysInFront { get; }

	RegistryKey ShellIconOverlayIdentifiersKey { get; }

	public Dictionary<string, string>? ReadEntries()
	{
		Dictionary<string, string> entries = [];

		foreach (var skn in ShellIconOverlayIdentifiersKey.GetSubKeyNames())
		{
			using var subkey = ShellIconOverlayIdentifiersKey.OpenSubKey(skn);
			string? value = subkey?.GetValue("") as string;
			if (!string.IsNullOrEmpty(value))
				entries.Add(skn, value);
		}

		return entries;

	}

	public void Execute()
	{
		var entries = ReadEntries();
		if (entries is not null)
			RemoveDuplicateKeys(entries);
	}

	/// <summary>
	/// Entfernt doppelte Einträge (bezogen auf den Standardwert)
	/// </summary>
	/// <param name="entries"></param>
	void RemoveDuplicateKeys(Dictionary<string, string> entries)
	{
		HashSet<string> removedKeys = [];

		foreach (var e in entries.ToArray())
		{
			if (removedKeys.Contains(e.Key))
				continue;

			foreach (var e2 in entries.ToArray().Where(x => x.Value == e.Value))
			{
				if (e2.Key == e.Key)
					continue;

				RemoveKey(e2.Key);
				entries.Remove(e2.Key);
				removedKeys.Add(e2.Key);
			}

		}

		foreach (var e in entries)
		{
			string normalizedKey = e.Key.TrimStart(' ');
			string newKeyName = normalizedKey;
			if (KeepTheseKeysInFront.Contains(normalizedKey))
				newKeyName = $" {normalizedKey}";

			if (newKeyName != e.Key)
				RenameKey(e.Key, newKeyName);
		}
	}

	void RemoveKey(string keyName)
	{
		Logger.LogInformation("Lösche (doppelten) Schlüssel: {keyName}", keyName);
		ShellIconOverlayIdentifiersKey.DeleteSubKeyTree(keyName);
	}

	void RenameKey(string oldKeyName, string newKeyName)
	{
		Logger.LogInformation("Benenne Schlüssel um (alt => neu): '{oldKeyName}' => '{newKeyName}'", oldKeyName, newKeyName);
		RegistryUtils.RenameSubKey(ShellIconOverlayIdentifiersKey, oldKeyName, newKeyName);
	}

}
