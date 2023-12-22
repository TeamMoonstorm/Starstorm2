using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using RoR2;

namespace Moonstorm.Starstorm2.Components
{
	// technically we can just throw any rpc methods in here????????? does that really work???????????????? that feels fucking stupid?????????????????????????????????
    public class FriendManager : NetworkBehaviour
    {
		public static FriendManager instance;
		[SystemInitializer]
		private static void Init()
		{
			Run.onRunStartGlobal += (run) =>
			{				
				//if (!NetworkServer.active) return; // ???
				instance = UnityEngine.Object.Instantiate<GameObject>(SS2Assets.LoadAsset<GameObject>("FriendManager", SS2Bundle.Chirr), run.transform).GetComponent<FriendManager>();
				NetworkServer.Spawn(instance.gameObject);
			};
		}


		// am i fucking stupid? i couldnt think of any other way to do this
		// couldnt be in ChirrFriendController since its a monobehavior added to the player master at runtime
		// couldnt be in ChirrFriendTracker since it could only be used if the master's body object was ChirrBody
		// theres like 10 different classes for all of chirr's behavior. theres no way this is correct
		[ClientRpc]
		public void RpcSetupFriend(GameObject masterObject, bool unfriend, bool isScepter)
		{
			CharacterMaster master = masterObject.GetComponent<CharacterMaster>();
			if (!unfriend)
			{
				if (isScepter)
					DontDestroyOnLoad(master); // BORING BORING BORING. CLAY TEMPLAR = AUTOWIN AND -1 ABILITY WHOLEW GAME. scepter ok tho :)
			}
			else
			{
				CharacterBody body = master.GetBody();
				if (body && body.teamComponent.indicator) // teammate indicator doesnt go away when changing team
					Destroy(body.teamComponent.indicator);
				if (Stage.instance)
					SceneManager.MoveGameObjectToScene(master.gameObject, Stage.instance.gameObject.scene); // i think this is how it works? // this is how it works :3
			}

		}
	}
}
