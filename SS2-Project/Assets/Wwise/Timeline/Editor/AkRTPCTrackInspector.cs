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

﻿#if !UNITY_2019_1_OR_NEWER
#define AK_ENABLE_TIMELINE
#endif
#if AK_ENABLE_TIMELINE
[System.Obsolete(AkSoundEngine.Deprecation_2019_2_0)]
[UnityEditor.CustomEditor(typeof(AkRTPCTrack))]
public class AkRTPCTrackInspector : UnityEditor.Editor
{
	private UnityEditor.SerializedProperty Parameter;

	public void OnEnable()
	{
		Parameter = serializedObject.FindProperty("Parameter");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

		using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
		{
			UnityEditor.EditorGUILayout.PropertyField(Parameter, new UnityEngine.GUIContent("Parameter: "));
		}

		serializedObject.ApplyModifiedProperties();
	}
}
#endif //AK_ENABLE_TIMELINE
