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

public class DefaultHandles
{
	public static bool Hidden
	{
		get
		{
			var type = typeof(UnityEditor.Tools);
			var field = type.GetField("s_Hidden",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			return (bool) field.GetValue(null);
		}
		set
		{
			var type = typeof(UnityEditor.Tools);
			var field = type.GetField("s_Hidden",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			field.SetValue(null, value);
		}
	}
}

[UnityEditor.CanEditMultipleObjects]
[UnityEditor.CustomEditor(typeof(AkGameObj))]
public class AkGameObjectInspector : UnityEditor.Editor
{
	private bool hideDefaultHandle;
	private UnityEditor.SerializedProperty listeners;
	private AkGameObj m_AkGameObject;

	private void OnEnable()
	{
		m_AkGameObject = target as AkGameObj;
		listeners = serializedObject.FindProperty("m_listeners");

		DefaultHandles.Hidden = hideDefaultHandle;
	}

	private void OnDisable()
	{
		DefaultHandles.Hidden = false;
	}

	public override void OnInspectorGUI()
	{
		// Unity tries to construct a AkGameObjPositionOffsetData all the time. Need this ugly workaround
		// to prevent it from doing this.
		if (m_AkGameObject.m_positionOffsetData != null)
		{
			if (!m_AkGameObject.m_positionOffsetData.KeepMe)
			{
				m_AkGameObject.m_positionOffsetData = null;
			}
		}

		var positionOffsetData = m_AkGameObject.m_positionOffsetData;
		var positionOffset = UnityEngine.Vector3.zero;

		UnityEditor.EditorGUI.BeginChangeCheck();

		using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
		{
			var applyPosOffset = UnityEditor.EditorGUILayout.Toggle("Apply Position Offset:", positionOffsetData != null);

			if (applyPosOffset != (positionOffsetData != null))
			{
				positionOffsetData = applyPosOffset ? new AkGameObjPositionOffsetData(true) : null;
			}

			if (positionOffsetData != null)
			{
				positionOffset = UnityEditor.EditorGUILayout.Vector3Field("Position Offset", positionOffsetData.positionOffset);

				UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

				if (hideDefaultHandle)
				{
					if (UnityEngine.GUILayout.Button("Show Main Transform"))
					{
						hideDefaultHandle = false;
						DefaultHandles.Hidden = hideDefaultHandle;
					}
				}
				else if (UnityEngine.GUILayout.Button("Hide Main Transform"))
				{
					hideDefaultHandle = true;
					DefaultHandles.Hidden = hideDefaultHandle;
				}
			}
			else if (hideDefaultHandle)
			{
				hideDefaultHandle = false;
				DefaultHandles.Hidden = hideDefaultHandle;
			}
		}

		UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

		var isEnvironmentAware = m_AkGameObject.isEnvironmentAware;

		using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
		{
			isEnvironmentAware = UnityEditor.EditorGUILayout.Toggle("Environment Aware:", isEnvironmentAware);
		}
		
		if (m_AkGameObject.gameObject.GetComponent<AkRoomAwareObject>() == null)
		{
			using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
			{
				UnityEditor.EditorGUILayout.LabelField("Room Aware: ", "");
				if (UnityEngine.GUILayout.Button("Add AkRoomAwareObject"))
					UnityEditor.Undo.AddComponent<AkRoomAwareObject>(m_AkGameObject.gameObject);
			}
		}

		if (UnityEditor.EditorGUI.EndChangeCheck())
		{
			UnityEditor.Undo.RecordObject(target, "AkGameObj Parameter Change");

			m_AkGameObject.m_positionOffsetData = positionOffsetData;

			var posOffsetDataProperty = serializedObject.FindProperty("m_positionOffsetData");
			var KeepMeProperty = posOffsetDataProperty.FindPropertyRelative("KeepMe");
			KeepMeProperty.boolValue = (positionOffsetData != null);
			if (positionOffsetData != null)
			{
				var posOffsetProperty = posOffsetDataProperty.FindPropertyRelative("positionOffset");
				posOffsetProperty.vector3Value = positionOffset;
			}
			serializedObject.ApplyModifiedProperties();

			m_AkGameObject.isEnvironmentAware = isEnvironmentAware;
		}

		if (isEnvironmentAware)
		{
			RigidbodyCheck(m_AkGameObject.gameObject);
		}

		UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

		UnityEditor.EditorGUI.BeginChangeCheck();
		m_AkGameObject.ScalingFactor = UnityEditor.EditorGUILayout.FloatField("Attenuation Scaling Factor", m_AkGameObject.ScalingFactor);
		if (UnityEditor.EditorGUI.EndChangeCheck())
		{
			if (m_AkGameObject.ScalingFactor <= 0)
			{
				m_AkGameObject.ScalingFactor = 0;
			}
			else
			{
				AkSoundEngine.SetScalingFactor(m_AkGameObject.gameObject, m_AkGameObject.ScalingFactor);
			}
		}
		UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

		using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
		{
			UnityEditor.EditorGUI.BeginChangeCheck();
			UnityEditor.EditorGUILayout.PropertyField(listeners);
			if (UnityEditor.EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
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
					"Interactions between AkGameObj and AkEnvironment require a Rigidbody component on the object or the environment.",
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

	private void OnSceneGUI()
	{
		if (m_AkGameObject.m_positionOffsetData == null)
		{
			return;
		}

		UnityEditor.EditorGUI.BeginChangeCheck();

		// Transform local offset to world coordinate
		var pos = m_AkGameObject.transform.TransformPoint(m_AkGameObject.m_positionOffsetData.positionOffset);

		// Get new handle position
		pos = UnityEditor.Handles.PositionHandle(pos, UnityEngine.Quaternion.identity);

		if (UnityEditor.EditorGUI.EndChangeCheck())
		{
			UnityEditor.Undo.RecordObject(target, "Position Offset Change");

			// Transform world offset to local coordinate
			m_AkGameObject.m_positionOffsetData.positionOffset = m_AkGameObject.transform.InverseTransformPoint(pos);
		}
	}
}
#endif