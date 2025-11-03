using BepInEx;
using BepInEx.Logging;
using CloverPit_MultiplayerMod.MonoBehaviours;
using UnityEngine;

namespace CloverPit_MultiplayerMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]

[BepInDependency("ArgumentsConsole", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger;

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        GameObject networkerObject = new("Networker");
        networkerObject.AddComponent<Networker>();


        if (!Networker.instance)
        {
            Logger.LogError("Networker was instanced incorrectly!");
            return;
        }

        

    }
}
