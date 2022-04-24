/*using RoR2EditorKit.Common;
using RoR2EditorKit.Core.Windows;
using System;
using UnityEditor;
using UnityEngine;
using Util = RoR2EditorKit.Util;

namespace Moonstorm.EditorUtils.EditorWindows
{
    public class CreateMSUnlockableDefWindow : CreateRoR2ScriptableObjectWindow<MSUnlockableDef>
    {
        public MSUnlockableDef unlockableDef;

        private string unlockName;
        private string unlockType;
        private bool showExtras;
        [MenuItem(Constants.RoR2EditorKitContextRoot + "MoonstormSharedUtils/MSUnlockableDef", false, Constants.RoR2EditorKitContextPriority)]
        public static void Open()
        {
            OpenEditorWindow<CreateMSUnlockableDefWindow>(null, "Create MSUnlockableDef");
        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();

            unlockableDef = (MSUnlockableDef)ScriptableObject;
            unlockName = "";
            unlockType = "";
            showExtras = false;
            mainSerializedObject = new SerializedObject(unlockableDef);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box");

            unlockName = EditorGUILayout.TextField("Unlockable Name", unlockName);
            unlockType = EditorGUILayout.TextField("Type of Unlock", unlockType);

            DrawField("achievementCondition");
            DrawField("achievedIcon");
            DrawField("unachievedIcon");

            SwitchButton("Extra Settings", ref showExtras);
            if (showExtras)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical("box");
                DrawExtras();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            if (SimpleButton("Create UnlockableDef"))
            {
                var result = CreateUnlockableDef();
                if (result)
                {
                    Debug.Log($"Succesfully Created UnlockableDef {unlockableDef.cachedName}");
                    TryToClose();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            ApplyChanges();
        }

        private void DrawExtras()
        {
            DrawField("hidden");
            var serverTracker = mainSerializedObject.FindProperty("serverTracked");
            DrawField(serverTracker, false);
            if (serverTracker.boolValue)
            {
                DrawField("baseServerAchievement");
            }
            EditorGUILayout.LabelField("Required Unlockable");

            DrawField("prerequisiteAchievement");
        }

        private bool CreateUnlockableDef()
        {
            try
            {
                unlockableDef.cachedName = $"{Settings.TokenPrefix.ToLowerInvariant()}.{unlockType.ToLowerInvariant()}.{unlockName.ToLowerInvariant()}";
                unlockableDef.nameToken = $"{Settings.TokenPrefix}_UNLOCK_{unlockType.ToUpperInvariant()}_{unlockName.ToUpperInvariant()}";
                unlockableDef.achievementNameToken = $"{Settings.TokenPrefix}_ACHIEVEMENT_{unlockName.ToUpperInvariant()}_NAME";
                unlockableDef.achievementDescToken = $"{Settings.TokenPrefix}_ACHIEVEMENT_{unlockName.ToUpperInvariant()}_DESC";

                Util.CreateAssetAtSelectionPath(unlockableDef);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while creating MSUnlockableDef: {e}");
                return false;
            }
        }
    }
}
*/