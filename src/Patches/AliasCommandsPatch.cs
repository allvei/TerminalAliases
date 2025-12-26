using HarmonyLib;
using System.Text;
using UnityEngine;

namespace TerminalAliases.Patches;

[HarmonyPatch(typeof(Terminal), "ParsePlayerSentence")]
internal static class AliasCommandsPatch
{
    [HarmonyPrefix]
    private static bool HandleAliasCommands(Terminal __instance, ref TerminalNode __result)
    {
        if (__instance.textAdded <= 0)
            return true;

        var screenText = __instance.screenText.text;
        var inputStart = screenText.Length - __instance.textAdded;
        var input = screenText.Substring(inputStart).Trim().ToLowerInvariant();

        if (string.IsNullOrEmpty(input))
            return true;

        var parts = input.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return true;

        var command = parts[0];

        if (command == "alias")
        {
            __result = HandleAliasCommand(parts, __instance);
            return false;
        }

        if (command == "removealias")
        {
            __result = HandleRemoveAliasCommand(parts, __instance);
            return false;
        }

        return true;
    }

    private static TerminalNode HandleAliasCommand(string[] parts, Terminal terminal)
    {
        if (parts.Length == 1)
        {
            var aliases = Plugin.AliasManager.Aliases;

            if (aliases.Count == 0)
                return CreateNode("No aliases defined.\n\nUse 'alias [name] [command]' to create one.\n\n");

            var sb = new StringBuilder();
            sb.AppendLine("Defined aliases:");
            sb.AppendLine();

            foreach (var kvp in aliases)
                sb.AppendLine($"  {kvp.Key} = {kvp.Value}");

            sb.AppendLine();
            return CreateNode(sb.ToString());
        }

        if (parts.Length == 2)
            return CreateNode("Usage: alias [name] [command]\n\nExample: alias vm view monitor\n\n");

        var alias = parts[1];
        var command = string.Join(" ", parts, 2, parts.Length - 2);

        if (Plugin.AliasManager.Add(alias, command))
            return CreateNode($"Alias '{alias}' set to '{command}'\n\n");

        return CreateNode("Failed to add alias.\n\n");
    }

    private static TerminalNode HandleRemoveAliasCommand(string[] parts, Terminal terminal)
    {
        if (parts.Length < 2)
        {
            return CreateNode("Usage: removealias [name]\n\n");
        }

        var alias = parts[1];

        if (Plugin.AliasManager.Remove(alias))
            return CreateNode($"Alias '{alias}' removed.\n\n");

        return CreateNode($"Alias '{alias}' not found.\n\n");
    }

    private static TerminalNode CreateNode(string text)
    {
        var node = ScriptableObject.CreateInstance<TerminalNode>();
        node.displayText = text;
        node.clearPreviousText = true;
        return node;
    }
}
