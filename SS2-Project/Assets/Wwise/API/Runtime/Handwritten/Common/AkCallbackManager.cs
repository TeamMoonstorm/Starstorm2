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

/// <summary>
///     This class manages the callback queue.  All callbacks from the native Wwise SDK go through this queue.
///     The queue needs to be driven by regular calls to PostCallbacks().  This is currently done in AkInitializer.cs, in
///     LateUpdate().
/// </summary>
public static class AkCallbackManager
{
	/// <summary>
	/// Event callback used when posting events.
	/// </summary>
	public delegate void EventCallback(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info);

	/// <summary>
	/// Monitoring callback called when Wwise reports errors.
	/// </summary>
	public delegate void MonitoringCallback(AkMonitorErrorCode in_errorCode, AkMonitorErrorLevel in_errorLevel,
		uint in_playingID, ulong in_gameObjID, string in_msg);

	/// <summary>
	/// Bank callback called upon bank load and unload and when errors occur.
	/// </summary>
	public delegate void BankCallback(uint in_bankID, System.IntPtr in_InMemoryBankPtr, AKRESULT in_eLoadResult, object in_Cookie);

	private static bool IsLoggingEnabled { get; set; }

	private static readonly AkEventCallbackInfo AkEventCallbackInfo = new AkEventCallbackInfo(System.IntPtr.Zero, false);

	private static readonly AkDynamicSequenceItemCallbackInfo AkDynamicSequenceItemCallbackInfo =
		new AkDynamicSequenceItemCallbackInfo(System.IntPtr.Zero, false);

	private static readonly AkMIDIEventCallbackInfo AkMIDIEventCallbackInfo =
		new AkMIDIEventCallbackInfo(System.IntPtr.Zero, false);

	private static readonly AkMarkerCallbackInfo
		AkMarkerCallbackInfo = new AkMarkerCallbackInfo(System.IntPtr.Zero, false);

	private static readonly AkDurationCallbackInfo AkDurationCallbackInfo =
		new AkDurationCallbackInfo(System.IntPtr.Zero, false);

	private static readonly AkMusicSyncCallbackInfo AkMusicSyncCallbackInfo =
		new AkMusicSyncCallbackInfo(System.IntPtr.Zero, false);

	private static readonly AkMusicPlaylistCallbackInfo AkMusicPlaylistCallbackInfo =
		new AkMusicPlaylistCallbackInfo(System.IntPtr.Zero, false);

#if UNITY_IOS && !UNITY_EDITOR
	private static AkAudioInterruptionCallbackInfo AkAudioInterruptionCallbackInfo =
		new AkAudioInterruptionCallbackInfo(System.IntPtr.Zero, false);
#endif // #if UNITY_IOS && ! UNITY_EDITOR

	private static readonly AkAudioSourceChangeCallbackInfo AkAudioSourceChangeCallbackInfo =
		new AkAudioSourceChangeCallbackInfo(System.IntPtr.Zero, false);

	private static readonly AkMonitoringCallbackInfo AkMonitoringCallbackInfo =
		new AkMonitoringCallbackInfo(System.IntPtr.Zero, false);

	private static readonly AkBankCallbackInfo AkBankCallbackInfo = new AkBankCallbackInfo(System.IntPtr.Zero, false);

	/// <summary>
	/// This class holds the data associated with an event callback.
	/// </summary>
	public class EventCallbackPackage
	{
		public bool m_bNotifyEndOfEvent;
		public EventCallback m_Callback;

		public object m_Cookie;
		public uint m_playingID;

		public static EventCallbackPackage Create(EventCallback in_cb, object in_cookie, ref uint io_Flags)
		{
			if (io_Flags == 0 || in_cb == null)
			{
				io_Flags = 0;
				return null;
			}

			var evt = new EventCallbackPackage();

			evt.m_Callback = in_cb;
			evt.m_Cookie = in_cookie;
			evt.m_bNotifyEndOfEvent = (io_Flags & (uint) AkCallbackType.AK_EndOfEvent) != 0;
			io_Flags = io_Flags | (uint) AkCallbackType.AK_EndOfEvent;

			m_mapEventCallbacks[evt.GetHashCode()] = evt;
			m_LastAddedEventPackage = evt;

			return evt;
		}
	}

	/// <summary>
	/// This class holds the data associated with a bank load or unload callback.
	/// </summary>
	public class BankCallbackPackage
	{
		public BankCallback m_Callback;
		public object m_Cookie;

		public BankCallbackPackage(BankCallback in_cb, object in_cookie)
		{
			m_Callback = in_cb;
			m_Cookie = in_cookie;

			m_mapBankCallbacks[GetHashCode()] = this;
		}
	}

	private static readonly System.Collections.Generic.Dictionary<int, EventCallbackPackage> m_mapEventCallbacks =
		new System.Collections.Generic.Dictionary<int, EventCallbackPackage>();

	private static readonly System.Collections.Generic.Dictionary<int, BankCallbackPackage> m_mapBankCallbacks =
		new System.Collections.Generic.Dictionary<int, BankCallbackPackage>();

	private static EventCallbackPackage m_LastAddedEventPackage;

	public static void RemoveEventCallback(uint in_playingID)
	{
		var cookiesToRemove = new System.Collections.Generic.List<int>();
		foreach (var pair in m_mapEventCallbacks)
		{
			if (pair.Value.m_playingID == in_playingID)
			{
				cookiesToRemove.Add(pair.Key);
				break;
			}
		}

		var Count = cookiesToRemove.Count;
		for (var ii = 0; ii < Count; ++ii)
			m_mapEventCallbacks.Remove(cookiesToRemove[ii]);

		AkSoundEnginePINVOKE.CSharp_CancelEventCallback(in_playingID);
	}

	public static void RemoveEventCallbackCookie(object in_cookie)
	{
		var cookiesToRemove = new System.Collections.Generic.List<int>();
		foreach (var pair in m_mapEventCallbacks)
		{
			if (pair.Value.m_Cookie == in_cookie)
				cookiesToRemove.Add(pair.Key);
		}

		var Count = cookiesToRemove.Count;
		for (var ii = 0; ii < Count; ++ii)
		{
			var toRemove = cookiesToRemove[ii];
			m_mapEventCallbacks.Remove(toRemove);
			AkSoundEnginePINVOKE.CSharp_CancelEventCallbackCookie((System.IntPtr) toRemove);
		}
	}

	public static void RemoveBankCallback(object in_cookie)
	{
		var cookiesToRemove = new System.Collections.Generic.List<int>();
		foreach (var pair in m_mapBankCallbacks)
		{
			if (pair.Value.m_Cookie == in_cookie)
				cookiesToRemove.Add(pair.Key);
		}

		var Count = cookiesToRemove.Count;
		for (var ii = 0; ii < Count; ++ii)
		{
			var toRemove = cookiesToRemove[ii];
			m_mapBankCallbacks.Remove(toRemove);
			AkSoundEnginePINVOKE.CSharp_CancelBankCallbackCookie((System.IntPtr) toRemove);
		}
	}

	public static void SetLastAddedPlayingID(uint in_playingID)
	{
		if (m_LastAddedEventPackage != null && m_LastAddedEventPackage.m_playingID == 0)
			m_LastAddedEventPackage.m_playingID = in_playingID;
	}

	private static MonitoringCallback m_MonitoringCB;

#if UNITY_IOS && !UNITY_EDITOR
	public delegate AKRESULT AudioInterruptionCallback(bool in_bEnterInterruption, object in_Cookie);
	// App implements its own callback.
	private static AudioInterruptionCallbackPackage ms_interruptCallbackPkg = null;

	public class AudioInterruptionCallbackPackage
	{
		public object m_Cookie;
		public AudioInterruptionCallback m_Callback;
	}
#endif // #if UNITY_IOS && ! UNITY_EDITOR

	public delegate AKRESULT BGMCallback(bool in_bOtherAudioPlaying, object in_Cookie);

	// App implements its own callback.
	private static BGMCallbackPackage ms_sourceChangeCallbackPkg;

	public class BGMCallbackPackage
	{
		public BGMCallback m_Callback;
		public object m_Cookie;
	}

	public class InitializationSettings
	{
		public const bool DefaultIsLoggingEnabled = true;
		public bool IsLoggingEnabled = DefaultIsLoggingEnabled;
	}

	public static void Init(InitializationSettings settings)
	{
		IsLoggingEnabled = settings.IsLoggingEnabled;

		AkCallbackSerializer.Init();
		SetLocalOutput(AkMonitorErrorLevel.ErrorLevel_All);
	}

	public static void Term()
	{
		AkCallbackSerializer.Term();
	}

	/// Call this to set a function to call whenever Wwise prints a message (warnings or errors).
	public static void SetMonitoringCallback(AkMonitorErrorLevel in_Level, MonitoringCallback in_CB)
	{
		SetLocalOutput(in_CB != null ? in_Level : 0);
		m_MonitoringCB = in_CB;
	}

	private static void SetLocalOutput(AkMonitorErrorLevel in_Level)
	{
#if UNITY_EDITOR
		try
		{
			uint XmlTimeout = uint.Parse(AkWwiseEditorSettings.Instance.XMLTranslatorTimeout);
			uint WaapiTimeout = uint.Parse(AkWwiseEditorSettings.Instance.WaapiTranslatorTimeout);
			uint portAsInt = uint.Parse(AkWwiseEditorSettings.Instance.WaapiPort);
			string baseSoundBankPath = AkBasePathGetter.GetPlatformBasePath();
			baseSoundBankPath += "SoundbanksInfo.xml";
			AkCallbackSerializer.SetLocalOutput((uint)in_Level,
				AkWwiseEditorSettings.Instance.WaapiIP, portAsInt,
				baseSoundBankPath,
				XmlTimeout, WaapiTimeout);
		}
		catch (System.Exception)
		{
			UnityEngine.Debug.LogWarning("Error parsing WaapiPort, XMLTranslatorTimeout or WaapiTranslatorTimeout. Must be an integer.");
		}
#endif
	}

#if UNITY_IOS && !UNITY_EDITOR
	/// Call this function to set a iOS callback interruption function. By default this callback is not defined.
	public static void SetInterruptionCallback(AudioInterruptionCallback in_CB, object in_cookie)
	{
		ms_interruptCallbackPkg = new AudioInterruptionCallbackPackage { m_Callback = in_CB, m_Cookie = in_cookie };
	}
#endif // #if UNITY_IOS && ! UNITY_EDITOR

	/// Call this to set a background music callback function. By default this callback is not defined.
	public static void SetBGMCallback(BGMCallback in_CB, object in_cookie)
	{
		ms_sourceChangeCallbackPkg = new BGMCallbackPackage { m_Callback = in_CB, m_Cookie = in_cookie };
	}

	public static void ParseCallbackInfoMessage(ref string in_message)
	{
		if(in_message.Contains("$g"))
		{
			int currentPos = in_message.IndexOf("$g", 0);
			while (currentPos > 0)
			{
				if (currentPos == -1)
					break;
				int spacePos = in_message.IndexOf(' ', currentPos);
				int idStringSize = (spacePos == -1 ? in_message.Length : spacePos )- currentPos - 2;
				string s_gID = in_message.Substring(currentPos + 2, idStringSize);
				ulong gId = AkSoundEngine.AK_INVALID_GAME_OBJECT;
				try
				{
					gId = ulong.Parse(s_gID);
				}
				catch (System.ArgumentNullException)
				{
					UnityEngine.Debug.LogWarning(s_gID + " was null.");
				}
				catch (System.ArgumentException)
				{
					UnityEngine.Debug.LogWarning(s_gID + " is not a number.");
				}
				catch (System.FormatException)
				{
					UnityEngine.Debug.LogWarning("Unable to parse " + s_gID + ".");
				}
				catch (System.OverflowException)
				{
					UnityEngine.Debug.LogWarning(s_gID + " is out of range of the UInt64 type.");
				}
				bool gameIdResolved = false;
#if UNITY_EDITOR
				if (gId != AkSoundEngine.AK_INVALID_GAME_OBJECT)
				{
					var obj =
						UnityEditor.EditorUtility.InstanceIDToObject((int)AkMonitoringCallbackInfo.gameObjID) as
							UnityEngine.GameObject;
					if (obj != null)
					{
						in_message = in_message.Replace(in_message.Substring(currentPos, idStringSize + 2), obj.name);
						in_message += " (Instance ID: " + AkMonitoringCallbackInfo.gameObjID + ")";
						gameIdResolved = true;
					}
				}
#endif
				if(!gameIdResolved)
					in_message = in_message.Replace(in_message.Substring(currentPos, idStringSize + 2), in_message.Substring(currentPos + 2, idStringSize));
				currentPos += 1;
				currentPos = in_message.IndexOf("$g", currentPos);
			}
		}
	}

	/// This function dispatches all the accumulated callbacks from the native sound engine. 
	/// It must be called regularly.  By default this is called in AkInitializer.cs.
	public static int PostCallbacks()
	{
		try
		{
			var numCallbacks = 0;

			for (var pNext = AkCallbackSerializer.Lock();
				pNext != System.IntPtr.Zero;
				pNext = AkSoundEnginePINVOKE.CSharp_AkSerializedCallbackHeader_pNext_get(pNext), ++numCallbacks)
			{
				var pPackage = AkSoundEnginePINVOKE.CSharp_AkSerializedCallbackHeader_pPackage_get(pNext);
				var eType = (AkCallbackType) AkSoundEnginePINVOKE.CSharp_AkSerializedCallbackHeader_eType_get(pNext);
				var pData = AkSoundEnginePINVOKE.CSharp_AkSerializedCallbackHeader_GetData(pNext);

				switch (eType)
				{
					case AkCallbackType.AK_AudioInterruption:
#if UNITY_IOS && !UNITY_EDITOR
						if (ms_interruptCallbackPkg != null && ms_interruptCallbackPkg.m_Callback != null)
						{
							AkAudioInterruptionCallbackInfo.setCPtr(pData);
							ms_interruptCallbackPkg.m_Callback(AkAudioInterruptionCallbackInfo.bEnterInterruption, ms_interruptCallbackPkg.m_Cookie);
						}
#endif // #if UNITY_IOS && ! UNITY_EDITOR
						break;

					case AkCallbackType.AK_AudioSourceChange:
						if (ms_sourceChangeCallbackPkg != null && ms_sourceChangeCallbackPkg.m_Callback != null)
						{
							AkAudioSourceChangeCallbackInfo.setCPtr(pData);
							ms_sourceChangeCallbackPkg.m_Callback(AkAudioSourceChangeCallbackInfo.bOtherAudioPlaying,
								ms_sourceChangeCallbackPkg.m_Cookie);
						}
						break;

					case AkCallbackType.AK_Monitoring:
						if (m_MonitoringCB != null)
						{
							AkMonitoringCallbackInfo.setCPtr(pData);
							m_MonitoringCB(AkMonitoringCallbackInfo.errorCode, AkMonitoringCallbackInfo.errorLevel,
								AkMonitoringCallbackInfo.playingID, AkMonitoringCallbackInfo.gameObjID, AkMonitoringCallbackInfo.message);
						}
#if UNITY_EDITOR
						else if (IsLoggingEnabled)
						{
							AkMonitoringCallbackInfo.setCPtr(pData);

							var msg = "Wwise: " + AkMonitoringCallbackInfo.message;
							ParseCallbackInfoMessage(ref msg);

							if (AkMonitoringCallbackInfo.errorLevel == AkMonitorErrorLevel.ErrorLevel_Error)
								UnityEngine.Debug.LogError(msg);
							else
								UnityEngine.Debug.Log(msg);
						}
#endif
						break;

					case AkCallbackType.AK_Bank:
						BankCallbackPackage bankPkg = null;
						if (!m_mapBankCallbacks.TryGetValue((int) pPackage, out bankPkg))
						{
							UnityEngine.Debug.LogError("WwiseUnity: BankCallbackPackage not found for <" + pPackage + ">.");
							break;
						}

						m_mapBankCallbacks.Remove((int) pPackage);

						if (bankPkg != null && bankPkg.m_Callback != null)
						{
							AkBankCallbackInfo.setCPtr(pData);
							bankPkg.m_Callback(AkBankCallbackInfo.bankID, AkBankCallbackInfo.inMemoryBankPtr, AkBankCallbackInfo.loadResult, bankPkg.m_Cookie);
						}
						break;

					default:
						EventCallbackPackage eventPkg = null;
						if (!m_mapEventCallbacks.TryGetValue((int) pPackage, out eventPkg))
						{
							UnityEngine.Debug.LogError("WwiseUnity: EventCallbackPackage not found for <" + pPackage + ">.");
							break;
						}

						AkCallbackInfo info = null;

						switch (eType)
						{
							case AkCallbackType.AK_EndOfEvent:
								m_mapEventCallbacks.Remove(eventPkg.GetHashCode());
								if (eventPkg.m_bNotifyEndOfEvent)
								{
									AkEventCallbackInfo.setCPtr(pData);
									info = AkEventCallbackInfo;
								}
								break;

							case AkCallbackType.AK_MusicPlayStarted:
								AkEventCallbackInfo.setCPtr(pData);
								info = AkEventCallbackInfo;
								break;

							case AkCallbackType.AK_EndOfDynamicSequenceItem:
								AkDynamicSequenceItemCallbackInfo.setCPtr(pData);
								info = AkDynamicSequenceItemCallbackInfo;
								break;

							case AkCallbackType.AK_MIDIEvent:
								AkMIDIEventCallbackInfo.setCPtr(pData);
								info = AkMIDIEventCallbackInfo;
								break;

							case AkCallbackType.AK_Marker:
								AkMarkerCallbackInfo.setCPtr(pData);
								info = AkMarkerCallbackInfo;
								break;

							case AkCallbackType.AK_Duration:
								AkDurationCallbackInfo.setCPtr(pData);
								info = AkDurationCallbackInfo;
								break;

							case AkCallbackType.AK_MusicSyncUserCue:
							case AkCallbackType.AK_MusicSyncBar:
							case AkCallbackType.AK_MusicSyncBeat:
							case AkCallbackType.AK_MusicSyncEntry:
							case AkCallbackType.AK_MusicSyncExit:
							case AkCallbackType.AK_MusicSyncGrid:
							case AkCallbackType.AK_MusicSyncPoint:
								AkMusicSyncCallbackInfo.setCPtr(pData);
								info = AkMusicSyncCallbackInfo;
								break;

							case AkCallbackType.AK_MusicPlaylistSelect:
								AkMusicPlaylistCallbackInfo.setCPtr(pData);
								info = AkMusicPlaylistCallbackInfo;
								break;

							default:
								UnityEngine.Debug.LogError("WwiseUnity: Undefined callback type <" + eType + "> received. Callback object possibly corrupted.");
								break;
						}

						if (info != null)
							eventPkg.m_Callback(eventPkg.m_Cookie, eType, info);
						break;
				}
			}

			return numCallbacks;
		}
		finally
		{
			AkCallbackSerializer.Unlock();
		}
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.