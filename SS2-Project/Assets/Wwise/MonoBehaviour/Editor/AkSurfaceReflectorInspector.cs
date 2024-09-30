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

[UnityEditor.CustomEditor(typeof(AkSurfaceReflector))]
[UnityEditor.CanEditMultipleObjects]
public class AkSurfaceReflectorInspector : UnityEditor.Editor
{
	private AkSurfaceReflector m_AkSurfaceReflector;

	private UnityEditor.SerializedProperty Mesh;
	private UnityEditor.SerializedProperty AcousticTextures;
	private UnityEditor.SerializedProperty TransmissionLossValues;
	private UnityEditor.SerializedProperty EnableDiffraction;
	private UnityEditor.SerializedProperty EnableDiffractionOnBoundaryEdges;
	private UnityEditor.SerializedProperty AssociatedRoom;

	public void OnEnable()
	{
		m_AkSurfaceReflector = target as AkSurfaceReflector;

		Mesh = serializedObject.FindProperty("Mesh");
		AcousticTextures = serializedObject.FindProperty("AcousticTextures");
		TransmissionLossValues = serializedObject.FindProperty("TransmissionLossValues");
		EnableDiffraction = serializedObject.FindProperty("EnableDiffraction");
		EnableDiffractionOnBoundaryEdges = serializedObject.FindProperty("EnableDiffractionOnBoundaryEdges");
		AssociatedRoom = serializedObject.FindProperty("AssociatedRoom");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		UnityEditor.EditorGUILayout.PropertyField(Mesh);

		UnityEditor.EditorGUILayout.PropertyField(AcousticTextures, true);
		CheckArraySize(m_AkSurfaceReflector, m_AkSurfaceReflector.AcousticTextures.Length, "acoustic textures");

		UnityEditor.EditorGUILayout.PropertyField(TransmissionLossValues, true);
		CheckArraySize(m_AkSurfaceReflector, m_AkSurfaceReflector.TransmissionLossValues.Length, "transmission loss values");

		UnityEditor.EditorGUILayout.PropertyField(EnableDiffraction);
		if (EnableDiffraction.boolValue)
		{
			UnityEditor.EditorGUILayout.PropertyField(EnableDiffractionOnBoundaryEdges);
		}

		UnityEditor.EditorGUILayout.PropertyField(AssociatedRoom);
		CheckAssociatedRoom(m_AkSurfaceReflector);

		serializedObject.ApplyModifiedProperties();
	}

	public static void CheckArraySize(AkSurfaceReflector surfaceReflector, int length, string name)
	{
		if (surfaceReflector == null || surfaceReflector.Mesh == null)
		{
			return;
		}

		int maxSize = surfaceReflector.Mesh.subMeshCount;
		if (length <= maxSize)
		{
			return;
		}

		UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

		using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
		{
			UnityEditor.EditorGUILayout.HelpBox(
				"There are more " + name + " than the Mesh has submeshes. Additional ones will be ignored.",
				UnityEditor.MessageType.Warning);
		}
	}

	public static void CheckAssociatedRoom(AkSurfaceReflector surfaceReflector)
	{
		if (surfaceReflector == null || surfaceReflector.AssociatedRoom == null)
		{
			return;
		}

		UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

		using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
		{
			UnityEditor.EditorGUILayout.HelpBox(
				"The Associated Room property is deprecated and will be removed in a future version. We recommend not using it by leaving it set to None.",
				UnityEditor.MessageType.Warning);
		}
	}
}
#endif