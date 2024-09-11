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

public static class AkXboxOneUtils
{
	private static readonly int[] Usages = { 0, 1, 4, 7 };
	private static readonly int SessionRequirement = 0;
	private static readonly int[] DeviceUsages = { 0 };

	[UnityEditor.MenuItem("Assets/Wwise/Xbox One/Enable Network Sockets")]
	public static void EnableXboxOneNetworkSockets()
	{
		var definitions = new[]
		{
			new SocketDefinition("WwiseDiscoverySocket", "24024", 1, "WwiseDiscovery"),
			new SocketDefinition("WwiseCommandSocket", "24025", 0, "WwiseCommand")
		};

		foreach (var def in definitions)
		{
			UnityEditor.PlayerSettings.XboxOne.SetSocketDefinition(def.Name, def.Port, def.Protocol, Usages, def.TemplateName,
				SessionRequirement, DeviceUsages);
		}
	}

	private class SocketDefinition
	{
		public readonly string Name;
		public readonly string Port;
		public readonly int Protocol;
		public readonly string TemplateName;

		public SocketDefinition(string name, string port, int protocol, string templateName)
		{
			Name = name;
			Port = port;
			Protocol = protocol;
			TemplateName = templateName;
		}
	}
}