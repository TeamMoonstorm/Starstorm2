#if UNITY_EDITOR
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

[UnityEditor.CustomEditor(typeof(AkRadialEmitter))]
public class AkRadialEmitterInspector : UnityEditor.Editor
{
	private AkRadialEmitter m_AkRadialEmitter;

	private UnityEditor.SerializedProperty outerRadius;
	private UnityEditor.SerializedProperty innerRadius;

	private void OnEnable()
	{
		m_AkRadialEmitter = target as AkRadialEmitter;

		outerRadius = serializedObject.FindProperty("outerRadius");
		innerRadius = serializedObject.FindProperty("innerRadius");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		if (m_AkRadialEmitter.outerRadius < 0.0f)
			m_AkRadialEmitter.outerRadius = 0.0f;
		if (m_AkRadialEmitter.innerRadius < 0.0f)
			m_AkRadialEmitter.innerRadius = 0.0f;
		if (m_AkRadialEmitter.innerRadius > m_AkRadialEmitter.outerRadius)
			m_AkRadialEmitter.innerRadius = m_AkRadialEmitter.outerRadius;

		UnityEditor.EditorGUILayout.PropertyField(outerRadius);
		UnityEditor.EditorGUILayout.PropertyField(innerRadius);

		EventCheck(m_AkRadialEmitter.gameObject);

		serializedObject.ApplyModifiedProperties();
	}

	public static void EventCheck(UnityEngine.GameObject gameObject)
	{
		if (AkWwiseEditorSettings.Instance.ShowSpatialAudioWarningMsg && gameObject.GetComponent<AkEvent>() == null)
		{
			UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

			using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
			{
				UnityEditor.EditorGUILayout.HelpBox(
					"Radial emitters are expected to emit sound. Add an AkEvent or an AkAmbient component to this game object.",
					UnityEditor.MessageType.Warning);

				if (UnityEngine.GUILayout.Button("Add AkEvent"))
					UnityEditor.Undo.AddComponent<AkEvent>(gameObject);

				if (UnityEngine.GUILayout.Button("Add AkAmbient"))
					UnityEditor.Undo.AddComponent<AkAmbient>(gameObject);
			}
		}
	}
}
#endif