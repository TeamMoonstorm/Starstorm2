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

#if UNITY_EDITOR
public class AkPluginActivatorConstants
{
	internal const string WwisePluginFolder = "Runtime/Plugins";
	
	public const string CONFIG_DEBUG = "Debug";
	public const string CONFIG_PROFILE = "Profile";
	public const string CONFIG_RELEASE = "Release";

	internal static readonly System.Collections.Generic.HashSet<PluginID> builtInPluginIDs =
		new System.Collections.Generic.HashSet<PluginID>
		{
			PluginID.AkCompressor,
			PluginID.AkDelay,
			PluginID.AkExpander,
			PluginID.AkGain,
			PluginID.AkMatrixReverb,
			PluginID.AkMeter,
			PluginID.AkParametricEQ,
			PluginID.AkPeakLimiter,
			PluginID.AkRoomVerb,
			PluginID.AkReflect,
#if !UNITY_2018_3_OR_NEWER
			PluginID.VitaReverb,
			PluginID.VitaCompressor,
			PluginID.VitaDelay,
			PluginID.VitaDistortion,
			PluginID.VitaEQ,
#endif
		};

	internal static readonly System.Collections.Generic.HashSet<PluginID> alwaysSkipPluginsIDs =
		new System.Collections.Generic.HashSet<PluginID>
		{
			PluginID.SineGenerator,
			PluginID.SinkAuxiliary,
			PluginID.SinkCommunication,
			PluginID.SinkControllerHeadphones,
			PluginID.SinkControllerSpeaker,
			PluginID.SinkDVRByPass,
			PluginID.SinkNoOutput,
			PluginID.SinkSystem,
			PluginID.ToneGenerator,
			PluginID.WwiseSilence,
			PluginID.AkAudioInput,
		};

	internal static readonly System.Collections.Generic.Dictionary<PluginID, string> PluginIDToStaticLibName =
		new System.Collections.Generic.Dictionary<PluginID, string>
		{
			{ PluginID.Ak3DAudioBedMixer, "Ak3DAudioBedMixerFX" },
			{ PluginID.AkAudioInput, "AkAudioInputSource" },
			{ PluginID.AkCompressor, "AkCompressorFX" },
			{ PluginID.AkRouterMixer, "AkRouterMixerFX" },
			{ PluginID.AkChannelRouter, "AkChannelRouterFX" },
			{ PluginID.AkConvolutionReverb, "AkConvolutionReverbFX" },
			{ PluginID.AkDelay, "AkDelayFX" },
			{ PluginID.AkExpander, "AkExpanderFX" },
			{ PluginID.AkFlanger, "AkFlangerFX" },
			{ PluginID.AkGain, "AkGainFX" },
			{ PluginID.AkGuitarDistortion, "AkGuitarDistortionFX" },
			{ PluginID.AkHarmonizer, "AkHarmonizerFX" },
			{ PluginID.AkMatrixReverb, "AkMatrixReverbFX" },
			{ PluginID.AkMeter, "AkMeterFX" },
			{ PluginID.AkMotionSink, "AkMotionSink" },
			{ PluginID.AkMotionSource, "AkMotionSourceSource" },
			{ PluginID.AkParametricEQ, "AkParametricEQFX" },
			{ PluginID.AkPeakLimiter, "AkPeakLimiterFX" },
			{ PluginID.AkPitchShifter, "AkPitchShifterFX" },
			{ PluginID.AkRecorder, "AkRecorderFX" },
			{ PluginID.AkReflect, "AkReflectFX" },
			{ PluginID.AkRoomVerb, "AkRoomVerbFX" },
			{ PluginID.AkSoundSeedGrain, "AkSoundSeedGrainSource" },
			{ PluginID.AkSoundSeedWind, "AkSoundSeedWindSource" },
			{ PluginID.AkSoundSeedWoosh, "AkSoundSeedWooshSource" },
			{ PluginID.AkStereoDelay, "AkStereoDelayFX" },
			{ PluginID.AkSynthOne, "AkSynthOneSource" },
			{ PluginID.AkTimeStretch, "AkTimeStretchFX" },
			{ PluginID.AkTremolo, "AkTremoloFX" },
			{ PluginID.AuroHeadphone, "AuroHeadphoneFX" },
			{ PluginID.CrankcaseAudioREVModelPlayer, "CrankcaseAudioREVModelPlayerSource" },
			{ PluginID.iZHybridReverb, "iZHybridReverbFX" },
			{ PluginID.iZTrashBoxModeler, "iZTrashBoxModelerFX" },
			{ PluginID.iZTrashDelay, "iZTrashDelayFX" },
			{ PluginID.iZTrashDistortion, "iZTrashDistortionFX" },
			{ PluginID.iZTrashDynamics, "iZTrashDynamicsFX" },
			{ PluginID.iZTrashFilters, "iZTrashFiltersFX" },
			{ PluginID.iZTrashMultibandDistortion, "iZTrashMultibandDistortionFX" },
			{ PluginID.MasteringSuite, "MasteringSuiteFX" },
			{ PluginID.AkImpacterSource, "AkImpacterSource" },
			{ PluginID.McDSPFutzBox, "McDSPFutzBoxFX" },
			{ PluginID.McDSPLimiter, "McDSPLimiterFX" },
			{ PluginID.ResonanceAudioRenderer, "ResonanceAudioFX" },
			{ PluginID.ResonanceAudioRoomEffect, "ResonanceAudioFX" },
			{ PluginID.IgniterLive, "IgniterLiveSource" },
			{ PluginID.IgniterLiveSynth, "IgniterLiveSource" }
		};
	
	// Support libraries are DLLs that do not have an associated Wwise plug-in ID; they are meant to be loaded manually by the application
	internal static readonly System.Collections.Generic.List<string> SupportLibraries = 
		new System.Collections.Generic.List<string>
		{
			"AkVorbisHwAccelerator"
		};
		
	internal enum PluginID
	{
		// Built-in plugins
		Ak3DAudioBedMixer = 0x00BE0003, // Wwise 3D Audio Bed Mixer
		AkCompressor = 0x006C0003, //Wwise Compressor
		AkRouterMixer = 0x00AC0006, //Wwise RouterMixer
		AkChannelRouter = 0x00BF0003, // Wwise Channel Router
		AkDelay = 0x006A0003, //Delay
		AkExpander = 0x006D0003, //Wwise Expander
		AkGain = 0x008B0003, //Gain
		AkMatrixReverb = 0x00730003, //Matrix Reverb
		AkMeter = 0x00810003, //Wwise Meter
		AkParametricEQ = 0x00690003, //Wwise Parametric EQ
		AkPeakLimiter = 0x006E0003, //Wwise Peak Limiter
		AkRoomVerb = 0x00760003, //Wwise RoomVerb
		SineGenerator = 0x00640002, //Sine
		SinkAuxiliary = 0xB40007,
		SinkCommunication = 0xB00007,
		SinkControllerHeadphones = 0xB10007,
		SinkControllerSpeaker = 0xB30007,
		SinkDVRByPass = 0xAF0007,
		SinkNoOutput = 0xB50007,
		SinkSystem = 0xAE0007,
		ToneGenerator = 0x00660002, //Tone Generator
		WwiseSilence = 0x00650002, //Wwise Silence
#if !UNITY_2018_3_OR_NEWER
		VitaReverb = 0x008C0003, //Vita Reverb
		VitaCompressor = 0x008D0003, //Vita Compressor
		VitaDelay = 0x008E0003, //Vita Delay
		VitaDistortion = 0x008F0003, //Vita Distortion
		VitaEQ = 0x00900003, //Vita EQ
#endif

		// Static or DLL plugins
		AkAudioInput = 0xC80002,
		AkConvolutionReverb = 0x7F0003,
		AkFlanger = 0x7D0003,
		AkGuitarDistortion = 0x7E0003,
		AkHarmonizer = 0x8A0003,
		AkMotionSink = 0x1FB0007,
		AkMotionSource = 0x1990002,
		AkPitchShifter = 0x880003,
		AkRecorder = 0x840003,
		AkReflect = 0xAB0003,
		AkSoundSeedGrain = 0xB70002,
		AkSoundSeedWind = 0x770002,
		AkSoundSeedWoosh = 0x780002,
		AkStereoDelay = 0x870003,
		AkSynthOne = 0x940002,
		AkTimeStretch = 0x820003,
		AkTremolo = 0x830003,
		AuroHeadphone = 0x44C1073,
		CrankcaseAudioREVModelPlayer = 0x1A01052,
		iZHybridReverb = 0x21033,
		iZTrashBoxModeler = 0x71033,
		iZTrashDelay = 0x41033,
		iZTrashDistortion = 0x31033,
		iZTrashDynamics = 0x51033,
		iZTrashFilters = 0x61033,
		iZTrashMultibandDistortion = 0x91033,
		MasteringSuite = 0xBA0003,
		AkImpacterSource = 0xB80002,
		McDSPFutzBox = 0x6E1003,
		McDSPLimiter = 0x671003,
		ResonanceAudioRenderer = 0x641103,
		ResonanceAudioRoomEffect = 0xC81106,
		IgniterLive = 0x5110D2,
		IgniterLiveSynth = 0x5210D2
	}
}
#endif