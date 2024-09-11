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

#if !(UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.

#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
using AK.Wwise.Unity.WwiseAddressables;
#endif

namespace AK.Wwise
{
	[System.Serializable]
	///@brief This type can be used to post Events to the sound engine.
	public class Event : BaseType
	{
		public WwiseEventReference WwiseObjectReference;
		private uint m_playingId;
		public uint PlayingId
		{
			get { return m_playingId; }
		}

		public override WwiseObjectReference ObjectReference
		{
			get { return WwiseObjectReference; }
			set { WwiseObjectReference = value as WwiseEventReference; }
		}

		public override WwiseObjectType WwiseObjectType { get { return WwiseObjectType.Event; } }

		private void VerifyPlayingID(uint playingId)
		{
#if UNITY_EDITOR
			if (playingId == AkSoundEngine.AK_INVALID_PLAYING_ID && AkSoundEngine.IsInitialized())
			{
				UnityEngine.Debug.LogError("WwiseUnity: Could not post event (name: " + Name + ", ID: " + Id +
				                           "). Please make sure to load or rebuild the appropriate SoundBank.");
			}
#endif
		}

		/// <summary>
		///     Posts this Event on a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject</param>
		/// <returns>Returns the playing ID.</returns>
		public uint Post(UnityEngine.GameObject gameObject)
		{
			if (!IsValid())
				return AkSoundEngine.AK_INVALID_PLAYING_ID;

#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
			var args = new object[] { gameObject };
			var argTypes = new System.Type[] { gameObject.GetType()};
			if (!AkAddressableBankManager.Instance.LoadedBankContainsEvent(Name, Id, this, "Post", argTypes, args))
			{
				return AkSoundEngine.AK_PENDING_EVENT_LOAD_ID;
			}
#endif

			m_playingId = AkSoundEngine.PostEvent(Id, gameObject);
			VerifyPlayingID(m_playingId);
			return m_playingId;
		}

		/// <summary>
		///     Posts this Event on a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject</param>
		/// <param name="flags"></param>
		/// <param name="callback"></param>
		/// <param name="cookie">Optional cookie received by the callback</param>
		/// <returns>Returns the playing ID.</returns>
		public uint Post(UnityEngine.GameObject gameObject, CallbackFlags flags, AkCallbackManager.EventCallback callback,
			object cookie = null)
		{
			if (!IsValid())
				return AkSoundEngine.AK_INVALID_PLAYING_ID;

#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
			var args = new object[] { gameObject, flags, callback, cookie };
			var argTypes = new System.Type[] { typeof(UnityEngine.GameObject), 
				typeof(CallbackFlags), typeof (AkCallbackManager.EventCallback), typeof(object) };
			if (!AkAddressableBankManager.Instance.LoadedBankContainsEvent(Name, Id, this, "Post", argTypes, args))
			{
				return AkSoundEngine.AK_PENDING_EVENT_LOAD_ID;
			}
#endif

			m_playingId = AkSoundEngine.PostEvent(Id, gameObject, flags.value, callback, cookie);
			VerifyPlayingID(m_playingId);
			return m_playingId;
		}

		/// <summary>
		///     Posts this Event on a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject</param>
		/// <param name="flags"></param>
		/// <param name="callback"></param>
		/// <param name="cookie">Optional cookie received by the callback</param>
		/// <returns>Returns the playing ID.</returns>
		public uint Post(UnityEngine.GameObject gameObject, uint flags, AkCallbackManager.EventCallback callback,
			object cookie = null)
		{
			if (!IsValid())
				return AkSoundEngine.AK_INVALID_PLAYING_ID;

#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
			var args = new object[] { gameObject, flags, callback, cookie };
			var argTypes = new System.Type[] { typeof(UnityEngine.GameObject), 
				typeof(uint), typeof(AkCallbackManager.EventCallback), typeof(object) };
			if (!AkAddressableBankManager.Instance.LoadedBankContainsEvent(Name, Id, this, "Post", argTypes, args ))
			{
				return AkSoundEngine.AK_PENDING_EVENT_LOAD_ID;
			}
#endif

			m_playingId = AkSoundEngine.PostEvent(Id, gameObject, flags, callback, cookie);
			VerifyPlayingID(m_playingId);
			return m_playingId;
		}

		/// <summary>
		///     Posts this Event on a GameObject ID.
		/// </summary>
		/// <param name="gameObjectId">The GameObject ID</param>
		/// <returns>Returns the playing ID.</returns>
		public uint Post(ulong gameObjectId)
		{
			if (!IsValid())
				return AkSoundEngine.AK_INVALID_PLAYING_ID;

#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
			var args = new object[] { gameObjectId };
			var argTypes = new System.Type[] { gameObjectId.GetType() };
			if (!AkAddressableBankManager.Instance.LoadedBankContainsEvent(Name, Id, this, "Post", argTypes, args))
			{
				return AkSoundEngine.AK_PENDING_EVENT_LOAD_ID;
			}
#endif

			m_playingId = AkSoundEngine.PostEvent(Id, gameObjectId);
			VerifyPlayingID(m_playingId);
			return m_playingId;
		}

		public void Stop(UnityEngine.GameObject gameObject, int transitionDuration = 0,
			AkCurveInterpolation curveInterpolation = AkCurveInterpolation.AkCurveInterpolation_Linear)
		{
			ExecuteAction(gameObject, AkActionOnEventType.AkActionOnEventType_Stop, transitionDuration, curveInterpolation);
		}

		/// <summary>
		///     Executes various actions on this event associated with a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject</param>
		/// <param name="actionOnEventType"></param>
		/// <param name="transitionDuration"></param>
		/// <param name="curveInterpolation"></param>
		public void ExecuteAction(UnityEngine.GameObject gameObject, AkActionOnEventType actionOnEventType,
			int transitionDuration, AkCurveInterpolation curveInterpolation)
		{
			if (IsValid())
			{

#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
				var args = new object[] { gameObject, actionOnEventType, transitionDuration, curveInterpolation };
				var argTypes = new System.Type[] { gameObject.GetType(), actionOnEventType.GetType(), transitionDuration.GetType(), curveInterpolation.GetType() };
				if (!AkAddressableBankManager.Instance.LoadedBankContainsEvent(Name, Id, this, "ExecuteAction", argTypes, args))
				{
					return;
				}
#endif

				var result = AkSoundEngine.ExecuteActionOnEvent(Id, actionOnEventType, gameObject, transitionDuration,
					curveInterpolation);
				Verify(result);
			}
		}

		/// <summary>
		///     Posts MIDI Events on this Event associated with a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject</param>
		/// <param name="array">The array of AkMIDIPost that are posted.</param>
		public void PostMIDI(UnityEngine.GameObject gameObject, AkMIDIPostArray array)
		{
			if (IsValid())
				array.PostOnEvent(Id, gameObject);
		}

		/// <summary>
		///     Posts MIDI Events on this Event associated with a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject</param>
		/// <param name="array">The array of AkMIDIPost that are posted.</param>
		/// <param name="count">The number of elements from the array that will be posted.</param>
		public void PostMIDI(UnityEngine.GameObject gameObject, AkMIDIPostArray array, int count)
		{
			if (IsValid())
				array.PostOnEvent(Id, gameObject, count);
		}

		/// <summary>
		///     Stops MIDI Events on this Event associated with a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject</param>
		public void StopMIDI(UnityEngine.GameObject gameObject)
		{
			if (IsValid())
				AkSoundEngine.StopMIDIOnEvent(Id, gameObject);
		}

		/// <summary>
		///     Stops all MIDI Events on this Event.
		/// </summary>
		public void StopMIDI()
		{
			if (IsValid())
				AkSoundEngine.StopMIDIOnEvent(Id);
		}
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.