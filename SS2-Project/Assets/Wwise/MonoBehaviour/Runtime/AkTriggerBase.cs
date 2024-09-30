#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
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

/// @brief Base class for the generic triggering mechanism for Wwise Integration.
/// All Wwise components will use this mechanism to drive their behavior.
/// Derive from this class to add your own triggering condition, as described in \ref unity_add_triggers
public abstract class AkTriggerBase : UnityEngine.MonoBehaviour
{
	/// Delegate declaration for all Wwise Triggers.
	public delegate void Trigger(
		UnityEngine.GameObject in_gameObject ///< in_gameObject is used to pass "Collidee" objects when Colliders are used.  Some components have the option "Use other object", this is the object they'll use.
	);

	/// All components reacting to the trigger will be registered in this delegate.
	public Trigger triggerDelegate = null;

	public static System.Collections.Generic.Dictionary<uint, string> GetAllDerivedTypes()
	{
		var derivedTypes = new System.Collections.Generic.Dictionary<uint, string>();

		var baseType = typeof(AkTriggerBase);

		var types = baseType.Assembly.GetTypes();

		for (var i = 0; i < types.Length; i++)
		{
			if (types[i].IsClass &&
			    (types[i].IsSubclassOf(baseType) || baseType.IsAssignableFrom(types[i]) && baseType != types[i]))
			{
				var typeName = types[i].Name;
				derivedTypes.Add(AkUtilities.ShortIDGenerator.Compute(typeName), typeName);
			}
		}

		//Add the Awake, Start and Destroy triggers and build the displayed list.
		derivedTypes.Add(AkUtilities.ShortIDGenerator.Compute("Awake"), "Awake");
		derivedTypes.Add(AkUtilities.ShortIDGenerator.Compute("Start"), "Start");
		derivedTypes.Add(AkUtilities.ShortIDGenerator.Compute("Destroy"), "Destroy");

		return derivedTypes;
	}
}

#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.