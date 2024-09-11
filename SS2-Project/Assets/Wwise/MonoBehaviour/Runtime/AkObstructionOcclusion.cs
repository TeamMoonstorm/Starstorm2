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

public abstract class AkObstructionOcclusion : UnityEngine.MonoBehaviour
{
	private readonly System.Collections.Generic.List<AkAudioListener> listenersToRemove =
		new System.Collections.Generic.List<AkAudioListener>();

	protected readonly System.Collections.Generic.List<AkAudioListener> currentListenerList = new System.Collections.Generic.List<AkAudioListener>();

	private readonly System.Collections.Generic.Dictionary<AkAudioListener, ObstructionOcclusionValue>
		ObstructionOcclusionValues = new System.Collections.Generic.Dictionary<AkAudioListener, ObstructionOcclusionValue>();

	protected float fadeRate;

	[UnityEngine.Tooltip("Fade time in seconds")]
	/// The number of seconds for fade ins and fade outs.
	public float fadeTime = 0.5f;

	[UnityEngine.Tooltip("Layers of obstructers/occluders")]
	/// Indicates which layers act as obstructers/occluders.
	public UnityEngine.LayerMask LayerMask = -1;

	[UnityEngine.Tooltip("Maximum distance to perform the obstruction/occlusion. Negative values mean infinite")]
	/// The maximum distance at which to perform obstruction/occlusion. A negative value will be interpreted as inifinite distance.
	public float maxDistance = -1.0f;

	[UnityEngine.Tooltip("The number of seconds between raycasts")]
	/// The number of seconds between obstruction/occlusion checks.
	public float refreshInterval = 1;

	private float refreshTime;

	protected void InitIntervalsAndFadeRates()
	{
		refreshTime = refreshInterval + UnityEngine.Random.Range(0.0f, refreshInterval);
		fadeRate = 1 / fadeTime;
	}

	protected abstract void UpdateCurrentListenerList();

	private void UpdateObstructionOcclusionValues()
	{
		// add new listeners
		for (var i = 0; i < currentListenerList.Count; ++i)
		{
			if (!ObstructionOcclusionValues.ContainsKey(currentListenerList[i]))
			{
				ObstructionOcclusionValues.Add(currentListenerList[i], new ObstructionOcclusionValue());
			}
		}

		// remove listeners
		foreach (var ObsOccPair in ObstructionOcclusionValues)
		{
			if (!currentListenerList.Contains(ObsOccPair.Key))
			{
				listenersToRemove.Add(ObsOccPair.Key);
			}
		}

		for (var i = 0; i < listenersToRemove.Count; ++i)
		{
			ObstructionOcclusionValues.Remove(listenersToRemove[i]);
		}

		listenersToRemove.Clear();
	}

	private void CastRays()
	{
		if (refreshTime >= refreshInterval)
		{
			refreshTime -= refreshInterval;

			foreach (var ObsOccPair in ObstructionOcclusionValues)
			{
				var listener = ObsOccPair.Key;
				var ObsOccValue = ObsOccPair.Value;

				var difference = listener.transform.position - transform.position;
				var magnitude = difference.magnitude;

				if (maxDistance > 0 && magnitude > maxDistance)
				{
					ObsOccValue.targetValue = ObsOccValue.currentValue;
				}
				else
				{
					ObsOccValue.targetValue =
						UnityEngine.Physics.Raycast(transform.position, difference / magnitude, magnitude, LayerMask.value) ? 1.0f : 0.0f;
				}
			}
		}

		refreshTime += UnityEngine.Time.deltaTime;
	}

	protected abstract void SetObstructionOcclusion(
		System.Collections.Generic.KeyValuePair<AkAudioListener, ObstructionOcclusionValue> ObsOccPair);

	private void Update()
	{
		currentListenerList.Clear();
		UpdateCurrentListenerList();
		UpdateObstructionOcclusionValues();

		CastRays();

		foreach (var ObsOccPair in ObstructionOcclusionValues)
		{
			if (ObsOccPair.Value.Update(fadeRate))
			{
				SetObstructionOcclusion(ObsOccPair);
			}
		}
	}

	protected class ObstructionOcclusionValue
	{
		public float currentValue;
		public float targetValue;

		public bool Update(float fadeRate)
		{
			if (UnityEngine.Mathf.Approximately(targetValue, currentValue))
			{
				return false;
			}

			currentValue += fadeRate * UnityEngine.Mathf.Sign(targetValue - currentValue) * UnityEngine.Time.deltaTime;
			currentValue = UnityEngine.Mathf.Clamp(currentValue, 0.0f, 1.0f);
			return true;
		}
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.