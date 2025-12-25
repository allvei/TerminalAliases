using BepInEx.Configuration;
using System;
using System.Collections.Generic;

namespace TerminalAliases;

public class PluginConfig
{
    private const string AliasesSection = "Aliases";

    public ConfigEntry<string> AliasDefinitions { get; private set; }

    private Dictionary<string, string> _aliasMap = new();
    public IReadOnlyDictionary<string, string> AliasMap => _aliasMap;

    public PluginConfig(ConfigFile config)
    {
        AliasDefinitions = config.Bind(
            AliasesSection,
            "Definitions",
            "vm=view monitor\nsh=switch\nsc=scan\nst=store\nm=moons",
            "Define aliases, one per line, in the format: alias=command\n" +
            "Arguments are preserved, e.g. 'fl=flash' allows 'fl Phil' to become 'flash Phil'.\n" +
            "Works with modded commands too, e.g. 'buy=purchase' makes 'hotbarplus buy' work as 'hotbarplus purchase'."
        );

        ParseAliases();
        AliasDefinitions.SettingChanged += (_, _) => ParseAliases();
    }

    private void ParseAliases()
    {
        _aliasMap.Clear();

        if (string.IsNullOrWhiteSpace(AliasDefinitions.Value))
            return;

        var lines = AliasDefinitions.Value.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            var separatorIndex = trimmed.IndexOf('=');
            if (separatorIndex <= 0 || separatorIndex >= trimmed.Length - 1)
            {
                Plugin.Log.LogWarning($"Invalid alias definition (missing '='): {trimmed}");
                continue;
            }

            var alias = trimmed.Substring(0, separatorIndex).Trim().ToLowerInvariant();
            var command = trimmed.Substring(separatorIndex + 1).Trim();

            if (string.IsNullOrEmpty(alias) || string.IsNullOrEmpty(command))
            {
                Plugin.Log.LogWarning($"Invalid alias definition (empty alias or command): {trimmed}");
                continue;
            }

            if (_aliasMap.ContainsKey(alias))
            {
                Plugin.Log.LogWarning($"Duplicate alias '{alias}' ignored.");
                continue;
            }

            _aliasMap[alias] = command;
            Plugin.Log.LogDebug($"Registered alias: '{alias}' -> '{command}'");
        }

        Plugin.Log.LogInfo($"Loaded {_aliasMap.Count} alias(es).");
    }
}
