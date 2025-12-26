using HarmonyLib;
using UnityEngine.InputSystem;

namespace TerminalAliases.Patches;

[HarmonyPatch(typeof(Terminal))]
internal static class TerminalInputPatch
{
    private static bool _pendingConfirm;

    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    private static void HandleCtrlEnter(Terminal __instance)
    {
        if (!__instance.terminalInUse)
            return;

        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        // Ctrl+Enter to submit and auto-confirm
        if ((keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed) &&
            keyboard.enterKey.wasPressedThisFrame)
        {
            _pendingConfirm = true;
            __instance.OnSubmit();
        }
    }

    [HarmonyPatch("OnSubmit")]
    [HarmonyPostfix]
    private static void AutoConfirm(Terminal __instance)
    {
        if (!_pendingConfirm)
            return;

        _pendingConfirm = false;

        __instance.StartCoroutine(ConfirmCoroutine(__instance));
    }

    private static System.Collections.IEnumerator ConfirmCoroutine(Terminal terminal)
    {
        yield return null; // Wait one frame for the terminal to process the command

        if (!terminal.terminalInUse)
            yield break;

        var currentNode = terminal.currentNode;
        if (currentNode == null)
            yield break;

        // TerminalNode.terminalOptions contains compatible keywords
        // When a confirmation is pending, one of the options will have "confirm" as the word
        // and point to the result node via terminalOptions[i].result
        if (currentNode.terminalOptions == null || currentNode.terminalOptions.Length == 0)
            yield break;

        foreach (var option in currentNode.terminalOptions)
        {
            if (option.noun == null)
                continue;

            var word = option.noun.word;
            if (word != null && word.ToLowerInvariant() == "confirm")
            {
                terminal.LoadNewNode(option.result);
                Plugin.Log.LogDebug("Auto-confirmed via Ctrl+Enter.");
                yield break;
            }
        }
    }
}
