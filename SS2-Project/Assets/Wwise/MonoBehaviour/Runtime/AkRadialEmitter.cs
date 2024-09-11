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

[UnityEngine.AddComponentMenu("Wwise/Spatial Audio/AkRadialEmitter")]
[UnityEngine.RequireComponent(typeof(AkGameObj))]
[UnityEngine.DisallowMultipleComponent]
/// @brief A radial emitter is for sounds that are not point sources, but instead originate from a region of space.
/// @details A radial emitter is described by an inner and outer radius. The radii are used in spread and distance calculations, simulating a radial sound source.
/// Since all game objects have a position and orientation, the position (center) and local axes are defined by the AkGameObj component required by this component.
public class AkRadialEmitter : UnityEngine.MonoBehaviour
{
	#region Fields
	[UnityEngine.Tooltip("Define the outer radius around each sound position to simulate a radial sound source. If the listener is outside the outer radius, the spread is defined by the area that the sphere takes in the listener field of view. When the listener intersects the outer radius, the spread is exactly 50%. When the listener is in between the inner and outer radius, the spread interpolates linearly from 50% to 100%.")]
	/// Define an outer radius around each sound position to simulate a radial sound source.
	/// The distance used for applying attenuation curves is taken as the distance between the listener and the point on the sphere, defined by the sound position and the outer radius, that is closest to the listener. 
	/// The spread for each sound position is calculated as follows:
	/// - If the listener is outside the outer radius, the spread is defined by the area that the sphere takes in the listener field of view. Specifically, this angle is calculated as 2.0*asinf( outerRadius / distance ), where distance is the distance between the listener and the sound position.
	///	- When the listener intersects the outer radius (the listener is exactly outerRadius units away from the sound position), the spread is exactly 50%.
	/// - When the listener is in between the inner and outer radius, the spread interpolates linearly from 50% to 100% as the listener transitions from the outer radius towards the inner radius.
	/// Note that transmission and diffraction calculations in Spatial Audio always use the center of the sphere (the position(s) passed into \c AK::SoundEngine::SetPosition or \c AK::SoundEngine::SetMultiplePositions) for raycasting. 
	/// To obtain accurate diffraction and transmission calculations for radial sources, where different parts of the volume may take different paths through or around geometry,
	/// it is necessary to pass multiple sound positions into \c AK::SoundEngine::SetMultiplePositions to allow the engine to 'sample' the area at different points.
	public float outerRadius = 0.0f;
	[UnityEngine.Tooltip("Define an inner radius around each sound position to simulate a radial sound source. If the listener is inside the inner radius, the spread is 100%.")]
	/// Define an inner radius around each sound position to simulate a radial sound source. If the listener is inside the inner radius, the spread is 100%.
	/// Note that transmission and diffraction calculations in Spatial Audio always use the center of the sphere (the position(s) passed into \c AK::SoundEngine::SetPosition or \c AK::SoundEngine::SetMultiplePositions) for raycasting. 
	/// To obtain accurate diffraction and transmission calculations for radial sources, where different parts of the volume may take different paths through or around geometry,
	/// it is necessary to pass multiple sound positions into \c AK::SoundEngine::SetMultiplePositions to allow the engine to 'sample' the area at different points.
	public float innerRadius = 0.0f;

	private float previousOuterRadius = 0.0f;
	private float previousInnerRadius = 0.0f;
	#endregion

	public void SetGameObjectOuterRadius(float in_outerRadius)
	{
		AkSoundEngine.SetGameObjectRadius(AkSoundEngine.GetAkGameObjectID(gameObject), in_outerRadius, innerRadius);
		previousOuterRadius = outerRadius = in_outerRadius;
		previousInnerRadius = innerRadius;
	}

	public void SetGameObjectInnerRadius(float in_innerRadius)
	{
		AkSoundEngine.SetGameObjectRadius(AkSoundEngine.GetAkGameObjectID(gameObject), outerRadius, in_innerRadius);
		previousOuterRadius = outerRadius;
		previousInnerRadius = innerRadius = in_innerRadius;
	}

	public void SetGameObjectRadius(float in_outerRadius, float in_innerRadius)
	{
		AkSoundEngine.SetGameObjectRadius(AkSoundEngine.GetAkGameObjectID(gameObject), in_outerRadius, in_innerRadius);
		previousOuterRadius = outerRadius = in_outerRadius;
		previousInnerRadius = innerRadius = in_innerRadius;
	}

	public void SetGameObjectRadius()
	{
		AkSoundEngine.SetGameObjectRadius(AkSoundEngine.GetAkGameObjectID(gameObject), outerRadius, innerRadius);
		previousOuterRadius = outerRadius;
		previousInnerRadius = innerRadius;
	}

	public void SetGameObjectRadius(UnityEngine.GameObject in_gameObject)
	{
		AkSoundEngine.SetGameObjectRadius(AkSoundEngine.GetAkGameObjectID(in_gameObject), outerRadius, innerRadius);
	}

	private void OnEnable()
	{
		SetGameObjectRadius();
	}

#if UNITY_EDITOR
	private void Update()
	{
		if (UnityEditor.EditorApplication.isPlaying)
		{
			if (previousOuterRadius != outerRadius ||
				previousInnerRadius != innerRadius)
			{
				SetGameObjectRadius();
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (!enabled)
		{
			return;
		}

		AkAmbient Ambient = GetComponent<AkAmbient>();
		bool showSpheres = true;
		if (Ambient && Ambient.multiPositionTypeLabel == MultiPositionTypeLabel.Large_Mode)
		{
			showSpheres = false;
		}

		if (showSpheres)
		{
			UnityEngine.Color SphereColor = UnityEngine.Color.yellow;
			SphereColor.a = 0.25f;
			UnityEngine.Gizmos.color = SphereColor;

			UnityEngine.Gizmos.DrawSphere(gameObject.transform.position, innerRadius);
			UnityEngine.Gizmos.DrawSphere(gameObject.transform.position, outerRadius);
		}
	}

#endif
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.