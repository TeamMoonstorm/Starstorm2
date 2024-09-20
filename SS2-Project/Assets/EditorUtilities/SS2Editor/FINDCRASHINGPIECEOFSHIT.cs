using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SS2.Editor
{
    public static class FINDCRASHINGPIECEOFSHIT
    {
        private static string _guidFromAssetStorm = "";
        [MenuItem("Tools/ENACT GOD'S WILL 2")]
        public static void GodMethod()
        {
            if(!EditorUtility.DisplayDialog("ENACT GOD'S WILL 2", "By enacting God's will 2, a method will be ran where all the Assets from the starstorm 2 assetbundles will be logged with their path, and subsequently it'll try to load it. This is useful to find assets that are causing crashes in the editor (IE: a buff def that it's icon path wasnt removed, so the editor tries to load the sprite causing a hang.)", "Ok, proceed", "Take me back!"))
            {
                Debug.Log("Not enacting god's will 2...");
                return;
            }
            Debug.LogWarning("Executing " + nameof(FINDCRASHINGPIECEOFSHIT) + "." + nameof(GodMethod) + "()");
            var assetGuids = AssetDatabase.FindAssets("", new string[] { "Assets/Starstorm2/AssetStorm" });
            foreach(var guid in assetGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                Debug.Log($"Trying to load asset with guid {guid} (Path={path})");

                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                Debug.Log($"Succesfully loaded {asset}");
            }
        }
    }
}