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

[UnityEngine.AddComponentMenu("Wwise/Spatial Audio/AkReverbZone")]
///@brief This component establishes a parent-child relationship between two Rooms and allows for sound propagation between them as if they were the same Room, without the need for a connecting Portal.
///@details Setting a Room as a Reverb Zone is useful in situations where two or more acoustic environments are not easily modeled as closed rooms connected by portals.
/// Possible uses for Reverb Zones include: a covered area with no walls, a forested area within an outdoor space, or any situation where multiple reverb effects are desired within a common space.
/// Reverb Zones have many advantages compared to standard Game-Defined Auxiliary Sends.
/// They are part of the wet path, and form reverb chains with other Rooms; they are spatialized according to their 3D extent; they are also subject to other acoustic phenomena simulated in Wwise Spatial Audio, such as diffraction and transmission.
/// A parent Room can have multiple Reverb Zones, but a Reverb Zone can only have a single Parent. A Room cannot be its own parent.
public class AkReverbZone : UnityEngine.MonoBehaviour
{
	#region Fields

	[UnityEngine.Tooltip("Set this Room as a Reverb Zone. Sound propagates between the Reverb Zone and its parent Room as if it were the same Room without the need for a connecting Portal. This is automatically populated by the current GameObject's Room component, if available.")]
	/// Set this Room as a Reverb Zone. Sound propagates between the Reverb Zone and its parent Room as if it were the same Room without the need for a connecting Portal.
	/// Examples include a covered area with no walls, a forested area within an outdoor space, or any situation where multiple reverb effects are desired within a common space.
	public AkRoom ReverbZone;

	[UnityEngine.Tooltip("Set this Room as the parent of the Reverb Zone. Sound propagates between the Reverb Zone and its parent Room as if it were the same Room without the need for a connecting Portal. A parent Room can have multiple Reverb Zones, but a Reverb Zone can only have a single Parent. A Room cannot be its own parent. Leave this set to None to attach the Reverb Zone to the automatically created 'Outdoors' Room.")]
	/// Set this Room as the parent of the Reverb Zone. Sound propagates between the Reverb Zone and its parent Room as if it were the same Room without the need for a connecting Portal.
	/// A parent Room can have multiple Reverb Zones, but a Reverb Zone can only have a single Parent. A Room cannot be its own parent.
	/// The automatically created 'Outdoors' Room is commonly used as a parent Room for Reverb Zones, because they often model open spaces. Leave this set to None to attach the Reverb Zone to 'Outdoors'.
	public AkRoom ParentRoom;

	[UnityEngine.Tooltip("Width of the transition region between the Reverb Zone and its parent. The transition region is centered around the Reverb Zone geometry. It only applies where triangle transmission loss is set to 0.")]
	/// Width of the transition region between the Reverb Zone and its parent. The transition zone is centered around the Reverb Zone geometry. It only applies where triangle transmission loss is set to 0.
	public float TransitionRegionWidth = 1.0f;

	private bool needsUpdate = false;

	#endregion

	/// <summary>
	/// Establish a parent-child relationship between two Rooms. Sound propagate between a Reverb Zone and its parent as if they were the same Room, without the need for a connecting Portal.
	/// Examples of Reverb Zones include a covered area with no walls, a forested area within an outdoor space, or any situation where multiple reverb effects are desired within a common space.
	/// Reverb Zones have many advantages compared to standard Game-Defined Auxiliary Sends. They are part of the wet path, and form reverb chains with other Rooms; they are spatialized according to their 3D extent; they are also subject to other acoustic phenomena simulated in Wwise Spatial Audio, such as diffraction and transmission.
	/// If a Room is already assigned to a parent Room, it is first be removed from the original parent (exactly as if RemoveReverbZone were called) before it is assigned to the new parent Room.
	/// The automatically created 'Outdoors' Room is commonly used as a parent Room for Reverb Zones, since they often model open spaces.
	/// Calls AkRoom::SetReverbZone() from the Reverb Zone AkRoom component with the Parent Room and transition Region Width parameters.
	/// </summary>
	/// <param name="reverbZone">The AkRoom component to set as a Reverb Zone.</param>
	/// <param name="parentRoom">The AkRoom component to set as the Reverb Zone's parent. A parent Room can have multiple Reverb Zones, but a Reverb Zone can only have a single Parent. A Room cannot be its own parent. Set to null to attach the Reverb Zone to the automatically created 'Outdoors' room.</param>
	/// <param name="transitionRegionWidth">The width of the transition region between the Reverb Zone and its parent. The transition region is centered around the Reverb Zone geometry. It only applies where triangle transmission loss is set to 0.</param>
	public static void SetReverbZone(
		AkRoom reverbZone,
		AkRoom parentRoom,
		float transitionRegionWidth)
	{
		if (reverbZone == null)
		{
			UnityEngine.Debug.LogError("SetReverbZone: Invalid Room component as the Reverb Zone parameter.");
			return;
		}

		reverbZone.SetReverbZone(parentRoom, transitionRegionWidth);
	}

	/// <summary>
	/// Remove a Reverb Zone from its parent. Sound can no longer propagate between the two Rooms, unless they are explicitly connected with a Portal.
	/// Calls AkRoom::RemoveReverbZone() from the Reverb Zone AkRoom component.
	/// </summary>
	/// <param name="reverbZone">The Reverb Zone AkRoom component.</param>
	public static void RemoveReverbZone(
		AkRoom reverbZone)
	{
		if (reverbZone == null)
		{
			UnityEngine.Debug.LogWarning("RemoveReverbZone has an invalid Room component as its Reverb Zone parameter.");
			return;
		}

		reverbZone.RemoveReverbZone();
	}

	/// <summary>
	/// Calls AkReverbZone::SetReverbZone() with this component's properties
	/// </summary>
	public void SetReverbZone()
	{
		SetReverbZone(ReverbZone, ParentRoom, TransitionRegionWidth);
		needsUpdate = false;
	}

	/// <summary>
	/// Calls AkReverbZone::RemoveReverbZone() with this component's properties
	/// </summary>
	public void RemoveReverbZone()
	{
		RemoveReverbZone(ReverbZone);
	}

	private void OnEnable()
	{
		SetReverbZone();
	}

	private void OnDisable()
	{
		RemoveReverbZone();
	}

	private void OnValidate()
	{
		if (ReverbZone == null)
		{
			var currentGameObjectRoom = GetComponent<AkRoom>();
			if (currentGameObjectRoom != null)
			{
				ReverbZone = currentGameObjectRoom;
			}
		}
		if (TransitionRegionWidth < 0.0f)
		{
			TransitionRegionWidth = 0.0f;
		}
		needsUpdate = true;
	}

	private void Update()
	{
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			return;
		}
#endif
		if (isActiveAndEnabled && needsUpdate)
		{
			SetReverbZone();
		}
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.