using System;
using System.Collections.Generic;
using System.IO;

namespace TerminalAliases;

public class AliasManager
{
    private readonly string _filePath;
    private Dictionary<string, string> _aliases = new();

    public IReadOnlyDictionary<string, string> Aliases => _aliases;

    public AliasManager(string configDirectory)
    {
        _filePath = Path.Combine(configDirectory, "TerminalAliases.cfg");
        Load();
    }

    public void Load()
    {
        _aliases.Clear();

        if (!File.Exists(_filePath))
        {
            Plugin.Log.LogInfo($"No alias file found at {_filePath}, starting fresh.");
            return;
        }

        try
        {
            var lines = File.ReadAllLines(_filePath);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                    continue;

                var separatorIndex = trimmed.IndexOf('=');
                if (separatorIndex <= 0 || separatorIndex >= trimmed.Length - 1)
                    continue;

                var alias = trimmed.Substring(0, separatorIndex).Trim().ToLowerInvariant();
                var command = trimmed.Substring(separatorIndex + 1).Trim();

                if (!string.IsNullOrEmpty(alias) && !string.IsNullOrEmpty(command))
                    _aliases[alias] = command;
            }

            Plugin.Log.LogInfo($"Loaded {_aliases.Count} alias(es) from {_filePath}");
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Failed to load aliases: {ex.Message}");
        }
    }

    public void Save()
    {
        try
        {
            var lines = new List<string>();
            foreach (var kvp in _aliases)
                lines.Add($"{kvp.Key}={kvp.Value}");

            File.WriteAllLines(_filePath, lines);
            Plugin.Log.LogDebug($"Saved {_aliases.Count} alias(es) to {_filePath}");
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Failed to save aliases: {ex.Message}");
        }
    }

    public bool Add(string alias, string command)
    {
        if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(command))
            return false;

        alias = alias.ToLowerInvariant();
        _aliases[alias] = command;
        Save();
        return true;
    }

    public bool Remove(string alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
            return false;

        alias = alias.ToLowerInvariant();
        if (_aliases.Remove(alias))
        {
            Save();
            return true;
        }
        return false;
    }

    public bool TryGet(string alias, out string command)
    {
        return _aliases.TryGetValue(alias.ToLowerInvariant(), out command!);
    }
}
