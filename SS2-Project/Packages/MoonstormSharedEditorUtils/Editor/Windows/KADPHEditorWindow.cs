/*using RoR2EditorKit.Core.Windows;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils.EditorWindows
{
    public class KADPHEditorWindow : ExtendedEditorWindow
    {
        public Vector2 scrollPos = new Vector2();

        private SerializedProperty mainProperty;

        private string selectedKeyAssetPropPath;
        private SerializedProperty selectedKeyAssetProp;

        private void OnGUI()
        {
            mainProperty = mainSerializedObject.FindProperty("KeyAssetDisplayPairs");

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));

            var tuple = DrawScrollableButtonSidebar(mainProperty, scrollPos, "keyAsset", ref selectedKeyAssetPropPath, ref selectedKeyAssetProp);
            scrollPos = tuple.Item1;

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
            if (selectedKeyAssetProp != null)
            {
                DrawSelectedKADP();
            }
            else
            {
                EditorGUILayout.LabelField("Select a Key Asset Display Pair from the List.");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            ApplyChanges();
        }

        private void DrawSelectedKADP()
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(500));

            DrawField(selectedKeyAssetProp.FindPropertyRelative("keyAsset"), true);

            DrawValueSidebar(selectedKeyAssetProp.FindPropertyRelative("displayPrefabs"));

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}*/