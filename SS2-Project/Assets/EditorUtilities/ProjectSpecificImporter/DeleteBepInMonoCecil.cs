using System;
using System.Collections;
using System.Collections.Generic;
using ThunderKit.Core.Config;
using UnityEditor;
using UnityEngine;

public class DeleteBepInMonoCecil : OptionalExecutor
{
    public override int Priority => RiskOfThunder.RoR2Importer.Constants.Priority.InstallBepInEx - 1;
    public override string Description => "Deletes BepInEx's MonoCecil assemblies, to allow the Collections package to properly work.";
    public override bool Execute()
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            string baseString = "Packages\\bbepis-bepinexpack\\BepInExPack\\BepInEx\\core\\";
            AssetDatabase.DeleteAsset(baseString + "Mono.Cecil.dll");
            AssetDatabase.DeleteAsset(baseString + "Mono.Cecil.Mdb.dll");
            AssetDatabase.DeleteAsset(baseString + "Mono.Cecil.Pdb.dll");
            AssetDatabase.DeleteAsset(baseString + "Mono.Cecil.Rocks.dll");
        }
        catch(Exception e)
        {
            Debug.LogError($"Failed to delete BepInEx Mono.Cecil dlls.\n{e}");
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        return true;
    }
}