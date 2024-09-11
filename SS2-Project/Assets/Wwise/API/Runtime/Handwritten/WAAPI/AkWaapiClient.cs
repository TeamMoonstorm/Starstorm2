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
/// The Waapi Client provides a core interface to Waapi using strings only. You will need to provide your own JSON serialization.
/// </summary>
internal class AkWaapiClient
{
	public Wamp wamp;

	public event Wamp.DisconnectedHandler Disconnected;

	/// <summary>Connect to a running instance of Wwise Authoring.</summary>
	/// <param name="uri">URI to connect. Usually the WebSocket protocol (ws:) followed by the hostname and port, followed by waapi.</param>
	/// <example>Connect("ws://localhost:8080/waapi")</example>
	/// <param name="timeout">The maximum timeout in milliseconds for the function to execute. Will raise Waapi.TimeoutException when timeout is reached.</param>
	public async System.Threading.Tasks.Task Connect(
		string uri = "ws://localhost:8080/waapi",
		int timeout = System.Int32.MaxValue)
	{
		if (wamp == null)
			wamp = new Wamp();
		wamp.Disconnected += Wamp_Disconnected;
		await wamp.Connect(uri, timeout);
	}

	private void Wamp_Disconnected()
	{
		if (Disconnected != null)
		{
			Disconnected();
		}
	}

	/// <summary>Close the connection.</summary>
	/// <param name="timeout">The maximum timeout in milliseconds for the function to execute. Will raise Waapi.TimeoutException when timeout is reached.</param>
	public async System.Threading.Tasks.Task Close(
		int timeout = System.Int32.MaxValue)
	{
		if (wamp == null)
			throw new Wamp.WampNotConnectedException("WAMP connection is not established");
		
		await wamp.Close(timeout);

		wamp.Disconnected -= Wamp_Disconnected;
		wamp = null;
	}

	/// <summary>
	/// Return true if the client is connected and ready for operations.
	/// </summary>
	public bool IsConnected()
	{
		if (wamp == null)
			return false;

		return wamp.IsConnected();
	}

	/// <summary>Call a WAAPI remote procedure. Refer to WAAPI reference documentation for a list of URIs and their arguments and options.</summary>
	/// <param name="uri">The URI of the remote procedure.</param>
	/// <param name="args">The arguments of the remote procedure.</param>
	/// <param name="options">The options the remote procedure.</param>
	/// <param name="timeout">The maximum timeout in milliseconds for the function to execute. Will raise Waapi.TimeoutException when timeout is reached.</param>
	/// <returns>A JSON string with the result of the Remote Procedure Call.</returns>
	public async System.Threading.Tasks.Task<string> Call(
		string uri,
		string args = "{}",
		string options = "{}",
		int timeout = System.Int32.MaxValue)
	{
		if (wamp == null)
			throw new Wamp.WampNotConnectedException("WAMP connection is not established");

		if (args == null)
			args = "{}";
		if (options == null)
			options = "{}";

		return await wamp.Call(uri, args, options, timeout);
	}

	/// <summary>Subscribe to WAAPI topic. Refer to WAAPI reference documentation for a list of topics and their options.</summary>
	/// <param name="topic">The topic to which subscribe.</param>
	/// <param name="options">The options the subscription.</param>
	/// <param name="publishHandler/// ">The delegate function to call when the topic is published.</param>
	/// <param name="timeout">The maximum timeout in milliseconds for the function to execute. Will raise Waapi.TimeoutException when timeout is reached.</param>
	/// <returns>Subscription id, that you can use to unsubscribe.</returns>
	public async System.Threading.Tasks.Task<uint> Subscribe(
		string topic,
		string options,
		Wamp.PublishHandler publishHandler,
		int timeout = System.Int32.MaxValue)
	{
		if (wamp == null)
			throw new Wamp.WampNotConnectedException("WAMP connection is not established");

		if (options == null)
			options = "{}";

		return await wamp.Subscribe(topic, options, publishHandler, timeout);
	}

	/// <summary>Unsubscribe from a subscription.</summary>
	/// <param name="subscriptionId">The subscription id received from the initial subscription.</param>
	/// <param name="timeout">The maximum timeout in milliseconds for the function to execute. Will raise Waapi.TimeoutException when timeout is reached.</param>
	public async System.Threading.Tasks.Task Unsubscribe(
		uint subscriptionId,
		int timeout = System.Int32.MaxValue)
	{
		if (wamp == null)
			throw new Wamp.WampNotConnectedException("WAMP connection is not established");

		await wamp.Unsubscribe(subscriptionId, timeout);
	}
}
