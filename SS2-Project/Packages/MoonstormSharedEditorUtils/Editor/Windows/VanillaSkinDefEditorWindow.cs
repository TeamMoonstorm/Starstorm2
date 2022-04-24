/*using RoR2EditorKit.Core.Windows;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils.EditorWindows
{
    public class VanillaSkinDefEditorWindow : ExtendedEditorWindow
    {
        SerializedProperty selectedArrayProp;
        string selectedArrayPropPath;

        string selectedArrayElementPath;
        SerializedProperty selectedArrayElementProperty;

        private void OnGUI()
        {
            DrawField(mainSerializedObject.FindProperty("bodyResourcePathKeyword"), true);
            DrawField(mainSerializedObject.FindProperty("icon"), true);
            DrawField(mainSerializedObject.FindProperty("nameToken"), true);
            DrawField(mainSerializedObject.FindProperty("unlockableDef"), true);
            DrawField(mainSerializedObject.FindProperty("rootObject"), true);

            string[] arrays = new string[7] { "baseSkins", "rendererInfos", "gameObjectActivations", "meshReplacements", "customGameObjectActivations", "vanillaProjectileGhostReplacements", "vanillaMinionSkinReplacements" };

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(300), GUILayout.ExpandHeight(true));

            if (DrawButtonSidebar(arrays))
            {
                selectedArrayElementPath = string.Empty;
                selectedArrayElementProperty = null;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            if (selectedArrayProp != null)
            {
                if (selectedArrayProp.displayName == "Base Skins")
                {
                    DrawBaseSkins();
                }
                else
                {
                    DrawSelectedArray();
                }
            }
            else
            {
                EditorGUILayout.LabelField("Select an Content Element from the List.");
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            ApplyChanges();
        }

        private bool DrawButtonSidebar(string[] fieldNames)
        {
            bool pressed = false;
            foreach (string field in fieldNames)
            {
                if (GUILayout.Button(field))
                {
                    selectedArrayPropPath = mainSerializedObject.FindProperty(field).propertyPath;
                    pressed = true;
                }
            }
            if (!string.IsNullOrEmpty(selectedArrayPropPath))
            {
                selectedArrayProp = mainSerializedObject.FindProperty(selectedArrayPropPath);
            }
            return pressed;
        }

        private void DrawBaseSkins()
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(300));

            DrawValueSidebar(selectedArrayProp);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSelectedArray()
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150));

            DrawButtonSidebar(selectedArrayProp, ref selectedArrayElementPath, ref selectedArrayElementProperty);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true), GUILayout.MaxWidth(300));

            if (selectedArrayElementProperty != null)
            {
                DrawProperties(selectedArrayElementProperty, true);
            }
            else
            {
                EditorGUILayout.LabelField("Select an Element from the List.");
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}*/