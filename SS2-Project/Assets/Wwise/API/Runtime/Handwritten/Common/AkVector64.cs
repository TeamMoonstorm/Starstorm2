#if !(UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
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

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public class AkVector64
{
	public void Zero() { 
		X = 0.0;
		Y = 0.0;
		Z = 0.0;
	}

	public double X = 0.0;
	public double Y = 0.0;
	public double Z = 0.0;

	public static implicit operator AkVector64(UnityEngine.Vector3 vector) {
		AkVector64 ret = new AkVector64();
		ret.X = vector.x;
		ret.Y = vector.y;
		ret.Z = vector.z;
		return ret; 
	}

}

#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
