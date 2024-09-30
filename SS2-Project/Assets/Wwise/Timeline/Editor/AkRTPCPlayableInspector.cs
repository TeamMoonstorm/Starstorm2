#if UNITY_EDITOR
#if !UNITY_2019_1_OR_NEWER
#define AK_ENABLE_TIMELINE
#endif
#if AK_ENABLE_TIMELINE

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

[System.Obsolete(AkSoundEngine.Deprecation_2019_2_0)]
[UnityEditor.CustomEditor(typeof(AkRTPCPlayable))]
public class AkRTPCPlayableInspector : UnityEditor.Editor
{
	private UnityEditor.SerializedProperty Behaviour;
	private UnityEditor.SerializedProperty overrideTrackObject;
	private AkRTPCPlayable playable;
	private UnityEditor.SerializedProperty RTPCObject;
	private UnityEditor.SerializedProperty setRTPCGlobally;

	public void OnEnable()
	{
		playable = target as AkRTPCPlayable;

		setRTPCGlobally = serializedObject.FindProperty("setRTPCGlobally");
		overrideTrackObject = serializedObject.FindProperty("overrideTrackObject");
		RTPCObject = serializedObject.FindProperty("RTPCObject");
		Behaviour = serializedObject.FindProperty("template");

		if (playable != null && playable.OwningClip != null)
			playable.OwningClip.displayName = playable.Parameter.Name;
	}

	public override void OnInspectorGUI()
	{
		UnityEditor.EditorGUILayout.HelpBox(AkSoundEngine.Deprecation_2019_2_0, UnityEditor.MessageType.Warning);

		serializedObject.Update();

		UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

		using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
		{
			if (setRTPCGlobally != null)
			{
				UnityEditor.EditorGUILayout.PropertyField(setRTPCGlobally, new UnityEngine.GUIContent("Set RTPC Globally: "));
				if (!setRTPCGlobally.boolValue)
				{
					if (overrideTrackObject != null)
					{
						UnityEditor.EditorGUILayout.PropertyField(overrideTrackObject,
							new UnityEngine.GUIContent("Override Track Object: "));
						if (overrideTrackObject.boolValue)
						{
							if (RTPCObject != null)
								UnityEditor.EditorGUILayout.PropertyField(RTPCObject, new UnityEngine.GUIContent("RTPC Object: "));
						}
					}
				}
			}
		}

		if (Behaviour != null)
			UnityEditor.EditorGUILayout.PropertyField(Behaviour, new UnityEngine.GUIContent("Animated Value: "), true);

		if (playable != null && playable.OwningClip != null)
			playable.OwningClip.displayName = playable.Parameter.Name;

		serializedObject.ApplyModifiedProperties();
	}
}

#endif // AK_ENABLE_TIMELINE
#endif //#if UNITY_EDITOR