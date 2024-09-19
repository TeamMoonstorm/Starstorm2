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

[UnityEditor.CustomEditor(typeof(AkRoomAwareObject))]
public class AkRoomAwareObjectInspector : UnityEditor.Editor
{
	private bool hideDefaultHandle;
	private UnityEditor.SerializedProperty listeners;
	private AkRoomAwareObject m_AkRoomAwareObject;

	private void OnEnable()
	{
		m_AkRoomAwareObject = target as AkRoomAwareObject;
	}

	public override void OnInspectorGUI()
	{
		RigidbodyCheck(m_AkRoomAwareObject.gameObject);
		ColliderCheck(m_AkRoomAwareObject.gameObject);
	}

	public static void ColliderCheck(UnityEngine.GameObject gameObject)
	{
		if (AkWwiseEditorSettings.Instance.ShowSpatialAudioWarningMsg)
		{
			var collider = gameObject.GetComponent<UnityEngine.Collider>();
			if (collider == null || !collider.enabled)
			{
				UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

				using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
				{
					UnityEditor.EditorGUILayout.HelpBox(
						"Interactions between AkRoomAwareObject and AkRoom require a Collider component on the object.",
						UnityEditor.MessageType.Error);
				}
			}
		}
	}

	public static void RigidbodyCheck(UnityEngine.GameObject gameObject)
	{
		if (AkWwiseEditorSettings.Instance.ShowSpatialAudioWarningMsg && gameObject.GetComponent<UnityEngine.Rigidbody>() == null)
		{
			UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

			using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
			{
				UnityEditor.EditorGUILayout.HelpBox(
					"Interactions between AkRoomAwareObject and AkRoom require a Rigidbody component on the object or the room.",
					UnityEditor.MessageType.Warning);

				if (UnityEngine.GUILayout.Button("Add Rigidbody"))
				{
					var rb = UnityEditor.Undo.AddComponent<UnityEngine.Rigidbody>(gameObject);
					rb.useGravity = false;
					rb.isKinematic = true;
				}
			}
		}
	}
}
#endif