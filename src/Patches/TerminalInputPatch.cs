using HarmonyLib;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TerminalAliases.Patches;

[HarmonyPatch(typeof(Terminal))]
internal static class TerminalInputPatch
{
    private static readonly string[] ConfirmKeywords = { "confirm", "yes", "y" };

    private static FieldInfo? _modifyingTextField;
    private static FieldInfo ModifyingTextField => _modifyingTextField ??=
        typeof(Terminal).GetField("modifyingText", BindingFlags.Instance | BindingFlags.NonPublic)!;

    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    private static void HandleCtrlEnter(Terminal __instance)
    {
        if (!__instance.terminalInUse)
            return;

        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if ((keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed) &&
            keyboard.enterKey.wasPressedThisFrame)
        {
            __instance.StartCoroutine(SubmitAndConfirm(__instance));
        }
    }

    private static IEnumerator SubmitAndConfirm(Terminal terminal)
    {
        terminal.OnSubmit();

        yield return new WaitForEndOfFrame();
        yield return null;

        if (!terminal.terminalInUse)
            yield break;

        var node = terminal.currentNode;
        if (node == null || node.terminalOptions == null || node.terminalOptions.Length == 0)
            yield break;

        foreach (var option in node.terminalOptions)
        {
            if (option.noun == null)
                continue;

            var word = option.noun.word;
            if (word == null)
                continue;

            var wordLower = word.ToLowerInvariant();
            foreach (var keyword in ConfirmKeywords)
            {
                if (wordLower == keyword)
                {
                    TypeAndSubmit(terminal, word);
                    Plugin.Log.LogDebug($"Auto-confirmed via Ctrl+Enter (keyword: {word}).");
                    yield break;
                }
            }
        }
    }

    private static void TypeAndSubmit(Terminal terminal, string text)
    {
        var wasModifying = (bool)ModifyingTextField.GetValue(terminal)!;
        ModifyingTextField.SetValue(terminal, true);

        terminal.screenText.text = terminal.screenText.text.Substring(0, terminal.screenText.text.Length - terminal.textAdded);
        terminal.screenText.text += text;
        terminal.textAdded = text.Length;

        ModifyingTextField.SetValue(terminal, wasModifying);

        terminal.OnSubmit();
    }
}
