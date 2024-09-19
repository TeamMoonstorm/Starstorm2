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

[System.AttributeUsage(System.AttributeTargets.Field, Inherited = true)]
public class AkEnumFlagAttribute : UnityEngine.PropertyAttribute
{
	public System.Type Type;

	public AkEnumFlagAttribute(System.Type type)
	{
		Type = type;
	}

#if UNITY_EDITOR
	[UnityEditor.CustomPropertyDrawer(typeof(AkEnumFlagAttribute))]
	public class PropertyDrawer : UnityEditor.PropertyDrawer
	{
		public override void OnGUI(UnityEngine.Rect position, UnityEditor.SerializedProperty property, UnityEngine.GUIContent label)
		{
			UnityEditor.EditorGUI.BeginProperty(position, label, property);
			var flagAttribute = (AkEnumFlagAttribute)attribute;
			property.longValue = UnityEditor.EditorGUI.EnumFlagsField(position, new UnityEngine.GUIContent(label.text, AkUtilities.GetTooltip(property)), (System.Enum)System.Enum.ToObject(flagAttribute.Type, property.longValue)).GetHashCode();
			UnityEditor.EditorGUI.EndProperty();
		}
	}
#endif
}
