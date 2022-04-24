/*using Moonstorm.EditorUtils.EditorWindows;
using RoR2EditorKit.Core.Inspectors;
using RoR2EditorKit.Core.Windows;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(VanillaSkinDef))]
    public class VanillaSkinDefInspector : ScriptableObjectInspector
    {
        [OnOpenAsset]
        public static bool OpenEditor(int instanceID, int line)
        {
            if (Settings.InspectorSettings.GetOrCreateInspectorSetting(typeof(VanillaSkinDefInspector)).isEnabled)
            {
                VanillaSkinDef obj = EditorUtility.InstanceIDToObject(instanceID) as VanillaSkinDef;
                if (obj != null)
                {
                    ExtendedEditorWindow.OpenEditorWindow<VanillaSkinDefEditorWindow>(obj, "Vanilla Skin Def Editor Window");
                    return true;
                }
            }
            return false;
        }
        public override void DrawCustomInspector()
        {
            if (GUILayout.Button("Open Editor"))
            {
                ExtendedEditorWindow.OpenEditorWindow<VanillaSkinDefEditorWindow>(target, "Vanilla Skin Def Editor Window");
            }
        }
    }
}*/