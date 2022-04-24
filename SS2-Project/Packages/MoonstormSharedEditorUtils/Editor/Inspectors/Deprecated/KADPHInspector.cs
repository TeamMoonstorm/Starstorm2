/*using Moonstorm.EditorUtils.EditorWindows;
using RoR2EditorKit.Core.Inspectors;
using RoR2EditorKit.Core.Windows;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(KeyAssetDisplayPairHolder))]
    public class KADPHInspector : ScriptableObjectInspector
    {
        [OnOpenAsset]
        public static bool OpenEditor(int instanceID, int line)
        {
            if (Settings.InspectorSettings.GetOrCreateInspectorSetting(typeof(KADPHInspector)).isEnabled)
            {
                KeyAssetDisplayPairHolder obj = EditorUtility.InstanceIDToObject(instanceID) as KeyAssetDisplayPairHolder;
                if (obj != null)
                {
                    ExtendedEditorWindow.OpenEditorWindow<KADPHEditorWindow>(obj, "Key Asset Display Pair Holder Window");
                    return true;
                }
            }
            return false;
        }

        public override void DrawCustomInspector()
        {
            if(GUILayout.Button("Open Editor"))
            {
                ExtendedEditorWindow.OpenEditorWindow<KADPHEditorWindow>(target, "Key Asset Display Pair Holder Window");
            }
        }
    }
}*/