using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using RoR2.CharacterAI;
using EntityStates.AI.Walker;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;
using RoR2.UI;
namespace Moonstorm.Starstorm2.Components
{
	public class ChirrFriendController : MonoBehaviour
	{
		private CharacterMaster master;
		public static float friendAimSpeedCoefficient = 3f;
		public Friend currentFriend;
		public bool hasFriend
        {
			get => this.currentFriend.master != null; /////////////////////// ?
        }
		public struct Friend // holds info from before friending
		{
			public Friend(CharacterMaster master)
            {
				this.master = master;
				this.teamIndex = master.teamIndex;
				this.itemStacks = HG.ArrayUtils.Clone(this.master.inventory.itemStacks);
				this.itemAcquisitionOrder = this.master.inventory.itemAcquisitionOrder; // reference fix later			
            }
			public CharacterMaster master;
			public TeamIndex teamIndex;
			public int[] itemStacks;
			public List<ItemIndex> itemAcquisitionOrder;		
		}

		public void Awake()
		{
			this.master = base.GetComponent<CharacterMaster>();
			this.currentFriend = default(Friend);
            Inventory.onServerItemGiven += ShareNewItem;
		}
      
        private void ShareNewItem(Inventory inventory, ItemIndex itemIndex, int count)
        {
            if(inventory == this.master.inventory && this.currentFriend.master)
            {
				if(Inventory.defaultItemCopyFilterDelegate(itemIndex)) // IMPLEMENT OUR OWN BLACKLIST LATER IF NEEDED
                {
					this.currentFriend.master.inventory.GiveItem(itemIndex, count);
				}			
            }
        }

		private void FixedUpdate()
		{
			if (!NetworkServer.active) return;

            if (this.master.GetBodyObject() && this.master.IsDeadAndOutOfLivesServer() && this.hasFriend)
            {
				this.RemoveFriend(this.currentFriend.master);
            }
        }
		
		private void OnFriendDeath()
        {
			if (!NetworkServer.active) return;

			// apparently stage transition = death, because the body doesnt exist.
			if(this.currentFriend.master.GetBodyObject() && this.currentFriend.master.IsDeadAndOutOfLivesServer())
            {
				this.RemoveFriend(this.currentFriend.master);
            }
        }

		// PROBABLY HAVE TO NETWORK THIS SOMEHOW
		// CLIENTS NEED TO SEE DONTDESTROYONLOAD (i think) AND CURRENTFRIEND (i think). EVERYTHING ELSE CAN BE SERVER ONLY
		public void AddFriend([NotNull] CharacterMaster master)
		{
			if (!NetworkServer.active)
			{
				SS2Log.Warning("ChirrFriendTracker.RemoveFriend called on client.");
				return;
			}

			if (this.hasFriend)
            {
				this.RemoveFriend(this.currentFriend.master);            
            }
			// change team
			// dontdestroyonload
			// turn speed
			// stat buffs (?)
			// copy inventory
			// heal
			// cleanse debuffs
			// add to minionownership
			this.currentFriend = new Friend(master);

			CharacterBody body = master.GetBody();
			TeamIndex newTeam = this.master.teamIndex;
			master.teamIndex = newTeam;
			DontDestroyOnLoad(master);

			if (NetworkServer.active)
			{
				if (body)
                {
					body.teamComponent.teamIndex = newTeam;
					Util.CleanseBody(body, true, false, false, true, true, false); // lol
					body.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 3f);
					body.healthComponent.HealFraction(1f, default(ProcChainMask));					
				}
					
				master.minionOwnership.SetOwner(this.master);			
				master.onBodyDeath.AddListener(OnFriendDeath);
				master.inventory.CopyItemsFrom(this.master.inventory);
				master.inventory.GiveItem(SS2Content.Items.ChirrFriendHelper, 1);
				
			}			
		}
		
		public void RemoveFriend([NotNull] CharacterMaster master)
		{
			if (!NetworkServer.active)
			{
				SS2Log.Warning("ChirrFriendTracker.RemoveFriend called on client.");
				return;
			}
			if (this.currentFriend.master != null && this.currentFriend.master != master)
			{
				SS2Log.Warning("ChirrFriendTracker.RemoveFriend: " + master + " was not a friend");
				return;
			}
			//undo everything
			CharacterBody body = master.GetBody();

			if(body.teamComponent.indicator) // teammate indicator doesnt go away when changing team
				Destroy(body.teamComponent.indicator);

			TeamIndex newTeam = this.currentFriend.teamIndex;		
			master.teamIndex = newTeam;
			SceneManager.MoveGameObjectToScene(master.gameObject, Stage.instance.gameObject.scene); // i think this is how it works?

			if (NetworkServer.active)
			{				
				if (body)
				{
					body.teamComponent.teamIndex = newTeam;
					SetStateOnHurt setStateOnHurt = body.GetComponent<SetStateOnHurt>(); // stun is good
					if (setStateOnHurt) setStateOnHurt.SetStun(1f);
				}

				master.minionOwnership.SetOwner(this.master); // this apparently unsets the owner if you call it again
				master.onBodyDeath.RemoveListener(OnFriendDeath);

				master.inventory.RemoveItem(SS2Content.Items.ChirrFriendHelper, 1); // dont think its necessary. just being overly safe

				Inventory inventory = master.inventory;
				inventory.itemAcquisitionOrder.Clear();
				int[] array = inventory.itemStacks;
				int num = 0;
				HG.ArrayUtils.SetAll<int>(array, num);
				master.inventory.AddItemsFrom(this.currentFriend.itemStacks, new Func<ItemIndex, bool>(target => true));

				
			}

			this.currentFriend = default(Friend);
		}


		// HEALTHBAR STUFF
		#region Healthbar Stuff
		// shows healthbars of stuff that your friend damaged
		// shows friend healthbar constantly
		private void OnEnable()
		{
			GlobalEventManager.onClientDamageNotified += ShowHealthbarFromFriend;
			On.RoR2.UI.CombatHealthBarViewer.VictimIsValid += FriendIsValid;
		}
		private void OnDisable()
		{
			On.RoR2.UI.CombatHealthBarViewer.VictimIsValid -= FriendIsValid;
			GlobalEventManager.onClientDamageNotified -= ShowHealthbarFromFriend;
		}
		private void ShowHealthbarFromFriend(DamageDealtMessage msg)
		{
			if (!msg.victim || msg.isSilent)
			{
				return;
			}
			TeamIndex objectTeam = TeamComponent.GetObjectTeam(msg.victim);
			foreach (CombatHealthBarViewer combatHealthBarViewer in CombatHealthBarViewer.instancesList)
			{
				if (this.hasFriend && msg.attacker == this.currentFriend.master.GetBodyObject() && Util.HasEffectiveAuthority(base.gameObject))
				{
					combatHealthBarViewer.HandleDamage(msg.victim.GetComponent<HealthComponent>(), objectTeam);
				}
			}
		}
		private bool FriendIsValid(On.RoR2.UI.CombatHealthBarViewer.orig_VictimIsValid orig, RoR2.UI.CombatHealthBarViewer self, HealthComponent victim)
		{
			bool result = orig(self, victim);
			bool isFriend = this.hasFriend && victim && victim.gameObject == this.currentFriend.master.GetBodyObject();
			return result || isFriend;
		}
		#endregion

	}
}