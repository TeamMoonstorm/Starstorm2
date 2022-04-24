using UnityEditor;
using System;
using System.Reflection;
using UnityEngine;
using System.Linq;

namespace MonoDetector
{
    public static class MonoDetector
    {
        [InitializeOnLoadMethod]
        private static void Detect()
        {
            string[] assemblyNames = AppDomain.CurrentDomain.GetAssemblies().Select(asm => asm.GetName().Name).ToArray();
            if(!assemblyNames.Contains("Mono.Cecil"))
            {
                EditorUtility.DisplayDialog("No Mono.Cecil dll found",
                    "WARNING: No \"Mono.Cecil.dll\" found.\n\n" +
                    "The RiskOfThunder edition of the Unity Multiplayer HLAPI has failed to find the Mono.Cecil dll in this project.\n\n" +
                    "The Mono.Cecil dll is required for the Weaving of assemblies to function properly, It is recommended to Install the BepInExPack of Risk of Rain 2 to this project.\n\n" +
                    "This message will display every time the editor reloads and no Mono.Cecil dll is found.",
                    "Understood.");
            }
        }
    }
}