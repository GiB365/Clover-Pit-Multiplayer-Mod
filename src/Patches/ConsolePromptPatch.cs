using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace CloverPit_MultiplayerMod.Patches;

class ArgumentsConsolePatches : HarmonyPatch
{
    [HarmonyPatch(typeof(ConsolePrompt), "TryExecuteCommand")]
    [HarmonyPrefix]
    static bool TryExecuteCommandPatch(ConsolePrompt __instance)
    {
        System.Type type = __instance.GetType();

        string inputString = (string)AccessTools.Field(type, "inputString").GetValue(__instance);
        int executionIndex = (int)AccessTools.Field(type, "executionIndex").GetValue(__instance);
        var availableCommands = (System.Collections.IDictionary)AccessTools.Field(type, "availableCommands").GetValue(__instance);
        List<string> outputStringList = (List<string>)AccessTools.Field(type, "outputStringList").GetValue(__instance);

        if (inputString.Length == 0) return false;

        executionIndex++;
        inputString = inputString.ToLower();

        string[] words = [];

        if (inputString.Contains(" "))
        {
            int space_index = inputString.IndexOf(" ");

            words = inputString.Split(" ");
        }

        object commandObj = null;
        if (availableCommands.Contains(words[0]))
            commandObj = availableCommands[words[0]];
                
        if (availableCommands.TryGetValue(inputString.ToLower(), out value))
        {
            outputStringList.Add(executionIndex + ": " + inputString);
            if (!value.TryExecute())
            {
                outputStringList.Add(executionIndex + ": <color=red>Error: </color> Command failed. Check player log for errors! Command: " + inputString);
            }
        }
        else
        {
            LogError("Command not found: " + inputString, executionIndex + ": ");
        }


        AccessTools.Field(type, "oldInputString").SetValue(__instance, inputString);
        AccessTools.Field(type, "inputString").SetValue(__instance, "");
        AccessTools.Field(type, "executionIndex").SetValue(__instance, executionIndex);

        return false;
    }
}

