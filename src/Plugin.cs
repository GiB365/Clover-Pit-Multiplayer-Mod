using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using CloverPit_MultiplayerMod.MonoBehaviours;
using HarmonyLib;
using UnityEngine;

namespace CloverPit_MultiplayerMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]

[BepInDependency("CloverPit_CommandPromptWithArgs", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger;

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        GameObject networkerObject = new("Networker");
        networkerObject.AddComponent<Networker>();
        DontDestroyOnLoad(networkerObject);

        Harmony harmony = new("CloverPit_MultiplayerMod");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        
        
        
        var patchedMethods = harmony.GetPatchedMethods();

        foreach (MethodBase method in patchedMethods) {
            Logger.LogInfo($"Patched method: {method.Name}");
        }

        Logger.LogInfo("Harmony patches applied successfully!");

        CloverPit_CommandPromptWithArgs.CustomCommands.AddCustomCommand(["host"], "Hosts a multiplayer server on specified port with optional password. Ex: host 7777", Networker.instance.Host);
        CloverPit_CommandPromptWithArgs.CustomCommands.AddCustomCommand(["join"], "Joins a multiplayer server on specified IP address and port with optional password. Ex: join 127.0.0.1 7777", Networker.instance.Join);
        CloverPit_CommandPromptWithArgs.CustomCommands.AddCustomCommand(["leave"], "Leaves the multiplayer server. Ex: leave", Networker.instance.Leave);
    }
}
