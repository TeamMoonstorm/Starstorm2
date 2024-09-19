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

[UnityEditor.CanEditMultipleObjects]
[UnityEditor.CustomEditor(typeof(AkWwiseTrigger))]
public class AkWwiseTriggerInspector : AkBaseInspector
{
    private readonly AkUnityEventHandlerInspector m_UnityEventHandlerInspector = new AkUnityEventHandlerInspector();

    private void OnEnable()
    {
        m_UnityEventHandlerInspector.Init(serializedObject);
    }

    public override void OnChildInspectorGUI()
    {
        m_UnityEventHandlerInspector.OnGUI();
    }
}
#endif