using HarmonyLib;
using TMPro;
using UnityEngine;

namespace TerminalAliases.Patches;

[HarmonyPatch(typeof(Terminal))]
internal static class TerminalInputPatch
{
    private static bool _pendingConfirm;

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    private static void SetMultilineInput(Terminal __instance)
    {
        if (__instance.screenText != null)
        {
            __instance.screenText.lineType = TMP_InputField.LineType.MultiLineNewline;
            Plugin.Log.LogDebug("Terminal input set to multiline.");
        }
    }

    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    private static void HandleCtrlEnter(Terminal __instance)
    {
        if (!__instance.terminalInUse)
            return;

        // Ctrl+Enter to auto-confirm
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
            Input.GetKeyDown(KeyCode.Return))
        {
            _pendingConfirm = true;
        }
    }

    [HarmonyPatch("OnSubmit")]
    [HarmonyPostfix]
    private static void AutoConfirm(Terminal __instance)
    {
        if (!_pendingConfirm)
            return;

        _pendingConfirm = false;

        // Submit "confirm" on the next frame
        __instance.StartCoroutine(SubmitConfirmCoroutine(__instance));
    }

    private static System.Collections.IEnumerator SubmitConfirmCoroutine(Terminal terminal)
    {
        yield return null; // Wait one frame for the terminal to process

        if (!terminal.terminalInUse)
            yield break;

        // Type and submit "confirm"
        terminal.screenText.text += "confirm";
        terminal.textAdded = 7; // Length of "confirm"
        terminal.OnSubmit();

        Plugin.Log.LogDebug("Auto-confirmed via Ctrl+Enter.");
    }
}
