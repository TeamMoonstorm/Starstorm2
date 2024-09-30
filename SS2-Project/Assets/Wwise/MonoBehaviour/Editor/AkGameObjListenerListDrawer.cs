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

[UnityEditor.CustomPropertyDrawer(typeof(AkGameObjListenerList))]
internal class AkGameObjListenerListDrawer : UnityEditor.PropertyDrawer
{
	private const int listenerSpacerWidth = 4;
	private const int removeButtonWidth = 20;

	public override float GetPropertyHeight(UnityEditor.SerializedProperty property, UnityEngine.GUIContent label)
	{
		var height = (UnityEditor.EditorGUIUtility.singleLineHeight + UnityEditor.EditorGUIUtility.standardVerticalSpacing) *
		             2;
		var listenerListProperty = property.FindPropertyRelative("initialListenerList");
		if (listenerListProperty != null && listenerListProperty.isArray)
		{
			height += (UnityEditor.EditorGUIUtility.singleLineHeight + UnityEditor.EditorGUIUtility.standardVerticalSpacing) *
			          listenerListProperty.arraySize + UnityEditor.EditorGUIUtility.standardVerticalSpacing;
		}

		return height;
	}

	public override void OnGUI(UnityEngine.Rect position, UnityEditor.SerializedProperty property,
		UnityEngine.GUIContent label)
	{
		// Using BeginProperty / EndProperty on the parent property means that
		// prefab override logic works on the entire property.
		UnityEditor.EditorGUI.BeginProperty(position, label, property);

		var initialRect = position;

		position = UnityEditor.EditorGUI.PrefixLabel(position,
			UnityEngine.GUIUtility.GetControlID(UnityEngine.FocusType.Passive),
			new UnityEngine.GUIContent("Use Default Listeners:"));
		position.height = UnityEditor.EditorGUIUtility.singleLineHeight +
		                  UnityEditor.EditorGUIUtility.standardVerticalSpacing;

		var useDefaultListenersProperty = property.FindPropertyRelative("useDefaultListeners");
		useDefaultListenersProperty.boolValue = UnityEngine.GUI.Toggle(position, useDefaultListenersProperty.boolValue, "");

		var listenerListProperty = property.FindPropertyRelative("initialListenerList");
		if (listenerListProperty.isArray)
		{
			position.height = UnityEditor.EditorGUIUtility.singleLineHeight +
			                  UnityEditor.EditorGUIUtility.standardVerticalSpacing;

			for (var ii = 0; ii < listenerListProperty.arraySize; ++ii)
			{
				var listenerFieldWidth = initialRect.width - removeButtonWidth;
				position.y += UnityEditor.EditorGUIUtility.singleLineHeight + UnityEditor.EditorGUIUtility.standardVerticalSpacing;
				position.x = initialRect.x;
				position.width = listenerFieldWidth - listenerSpacerWidth;

				var listenerProperty = listenerListProperty.GetArrayElementAtIndex(ii);
				UnityEditor.EditorGUI.PropertyField(position, listenerProperty, new UnityEngine.GUIContent("Listener " + ii));

				position.x = initialRect.x + listenerFieldWidth;
				position.width = removeButtonWidth;

				if (UnityEngine.GUI.Button(position, "X"))
				{
					UnityEngine.GUIUtility.keyboardControl = 0;
					UnityEngine.GUIUtility.hotControl = 0;

					listenerProperty.objectReferenceValue = null;
					listenerListProperty.DeleteArrayElementAtIndex(ii);
					--ii;
				}
			}

			position.x = initialRect.x;
			position.width = initialRect.width;
			position.y += UnityEditor.EditorGUIUtility.singleLineHeight + UnityEditor.EditorGUIUtility.standardVerticalSpacing +
			              UnityEditor.EditorGUIUtility.standardVerticalSpacing;

			if (UnityEngine.GUI.Button(position, "Add Listener"))
			{
				UnityEngine.GUIUtility.keyboardControl = 0;
				UnityEngine.GUIUtility.hotControl = 0;

				var lastPosition = listenerListProperty.arraySize;
				listenerListProperty.arraySize = lastPosition + 1;

				// Avoid copying the previous last array element into the newly added last position
				var listenerProperty = listenerListProperty.GetArrayElementAtIndex(lastPosition);
				listenerProperty.objectReferenceValue = null;
			}
		}

		UnityEditor.EditorGUI.EndProperty();
	}
}
#endif