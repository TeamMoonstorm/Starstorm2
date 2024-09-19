/*******************************************************************************
The content of this file includes portions of the proprietary AUDIOKINETIC Wwise
Technology released in source code form as part of the game integration package.
The content of this file may not be used without valid licenses to the
AUDIOKINETIC Wwise Technology.
Note that the use of the game engine is subject to the Unity(R) Terms of
Service at https://unity3d.com/legal/terms-of-service
 
License Usage
 
Licensees holding valid licenses to the AUDIOKINETIC Wwise Technology may use
this file in accordance with the end user license agreement provided with the
software or, alternatively, in accordance with the terms contained
in a written agreement between you and Audiokinetic Inc.
Copyright (c) 2024 Audiokinetic Inc.
*******************************************************************************/

﻿public class AkShowOnlyAttribute : UnityEngine.PropertyAttribute
{
#if UNITY_EDITOR
	[UnityEditor.CustomPropertyDrawer(typeof(AkShowOnlyAttribute))]
	public class PropertyDrawer : UnityEditor.PropertyDrawer
	{
		public override float GetPropertyHeight(UnityEditor.SerializedProperty property, UnityEngine.GUIContent label)
		{
			return UnityEditor.EditorGUI.GetPropertyHeight(property, label, true);
		}

		public override void OnGUI(UnityEngine.Rect position, UnityEditor.SerializedProperty property, UnityEngine.GUIContent label)
		{
			var saveEnabled = UnityEngine.GUI.enabled;
			UnityEngine.GUI.enabled = false;
			UnityEditor.EditorGUI.PropertyField(position, property, label, true);
			UnityEngine.GUI.enabled = saveEnabled;
		}
	}
#endif // #if UNITY_EDITOR
}
