using HarmonyLib;
using System;
using System.Reflection;
using System.Text;

namespace TerminalAliases.Patches;

[HarmonyPatch(typeof(Terminal), "ParsePlayerSentence")]
internal static class TerminalPatch
{
    private static FieldInfo? _modifyingTextField;

    private static FieldInfo ModifyingTextField => _modifyingTextField ??= 
        typeof(Terminal).GetField("modifyingText", BindingFlags.Instance | BindingFlags.NonPublic)!;

    [HarmonyPrefix]
    private static void ExpandAliases(Terminal __instance)
    {
        if (__instance.textAdded <= 0)
            return;

        var screenText = __instance.screenText.text;
        var inputStart = screenText.Length - __instance.textAdded;
        var input = screenText.Substring(inputStart);

        if (string.IsNullOrWhiteSpace(input))
            return;

        var expanded = ExpandInput(input);

        if (expanded == input)
            return;

        var wasModifying = (bool)ModifyingTextField.GetValue(__instance)!;
        ModifyingTextField.SetValue(__instance, true);

        var lengthDiff = expanded.Length - input.Length;
        __instance.screenText.text = screenText.Substring(0, inputStart) + expanded;
        __instance.textAdded += lengthDiff;

        ModifyingTextField.SetValue(__instance, wasModifying);

        Plugin.Log.LogDebug($"Expanded: '{input.Trim()}' -> '{expanded.Trim()}'");
    }

    private static string ExpandInput(string input)
    {
        var aliases = Plugin.Config.AliasMap;
        if (aliases.Count == 0)
            return input;

        var words = input.Split(new[] { ' ' }, StringSplitOptions.None);
        var modified = false;

        for (int i = 0; i < words.Length; i++)
        {
            var word = words[i];
            var wordLower = word.ToLowerInvariant();

            if (aliases.TryGetValue(wordLower, out var replacement))
            {
                words[i] = PreserveCase(word, replacement);
                modified = true;
            }
        }

        return modified ? string.Join(" ", words) : input;
    }

    private static string PreserveCase(string original, string replacement)
    {
        if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(replacement))
            return replacement;

        if (original == original.ToUpperInvariant())
            return replacement.ToUpperInvariant();

        if (char.IsUpper(original[0]))
        {
            var sb = new StringBuilder(replacement.Length);
            sb.Append(char.ToUpperInvariant(replacement[0]));
            if (replacement.Length > 1)
                sb.Append(replacement.Substring(1));
            return sb.ToString();
        }

        return replacement;
    }
}
