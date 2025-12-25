using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace TerminalAliases;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;
    internal static new PluginConfig Config { get; private set; } = null!;
    internal static Harmony Harmony { get; private set; } = null!;

    private void Awake()
    {
        Log = Logger;
        Config = new PluginConfig(base.Config);
        
        Harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        Harmony.PatchAll(Assembly.GetExecutingAssembly());
        
        Log.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} loaded.");
    }
}
