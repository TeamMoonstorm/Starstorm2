﻿using System;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using JetBrains.Annotations;
using RoR2.UI;
using System.Runtime.CompilerServices;
namespace SS2.Components
{
    public class ChirrFriendController : MonoBehaviour
	{
		// this class is half supports-multiple-friends, half not. i need to be less indecisive.

		// this class manages adding and removing friends
		// FOR STUFF THAT DIRECTLY INTERACTS WITH FRIEND'S AI AND BODY: SEE Items.ChirrFriendHelper
		private CharacterMaster master;
		public static float friendAimSpeedCoefficient = 3f;
		public Friend currentFriend;

		public static float healthDecayTime = 60f;

		public bool isScepter
		{
			get => _isScepter;
			set
			{
				if (_isScepter == value) return;
				this._isScepter = value;
				this.UpdateScepter();
			}
		}
		private bool _isScepter;

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
            TeamComponent.onLeaveTeamGlobal += CheckFriendLeftTeam;
		}
        private void OnDestroy()
        {
			TeamComponent.onLeaveTeamGlobal -= CheckFriendLeftTeam;
        }

		private void UpdateScepter()
        {
			if (!this.hasFriend) return;

			CharacterMaster master = this.currentFriend.master;
			this.RemoveFriend(master); 
			this.AddFriend(master); // FFFFFFFFFFFFFFFFFUCK im so lazy
        }
		// teamcomponent sets team to none on destroy XDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD
        private void CheckFriendLeftTeam(TeamComponent teamComponent, TeamIndex teamIndex) // teamIndex is the team our friend left (Player)
        {
			if (!NetworkServer.active || !this.hasFriend || !teamComponent.enabled) return;

			CharacterBody body = currentFriend.master.GetBody();
			if(body && body == teamComponent.body && body.healthComponent.alive && teamIndex == this.master.teamIndex)
            {
				this.RemoveFriend(currentFriend.master, false);
            }
        }

		public bool ItemFilter(ItemIndex itemIndex)
        {
			bool defaultFilter = Inventory.defaultItemCopyFilterDelegate(itemIndex);
			ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
			return defaultFilter && itemDef.DoesNotContainTag(ItemTag.HoldoutZoneRelated) && itemDef.DoesNotContainTag(ItemTag.OnStageBeginEffect)
				&& itemDef.DoesNotContainTag(ItemTag.InteractableRelated) && !ItemIsScepter(itemDef);
        }

		// make a util class
		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
		private bool ItemIsScepter(ItemDef def)
        {
			//return Starstorm.ScepterInstalled && itemIndex != AncientScepter.AncientScepterItem.instance.ItemDef.itemIndex; // idk how to do this
			return (def.name == "ITEM_ANCIENT_SCEPTER" || def.nameToken == "ITEM_ANCIENT_SCEPTER_NAME");

		}
        private void ShareNewItem(Inventory inventory, ItemIndex itemIndex, int count)
        {
            if(inventory == this.master.inventory && this.currentFriend.master)
            {
				if(ItemFilter(itemIndex))
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
				SS2Log.Warning("ChirrFriendTracker.AddFriend called on client.");
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
			if(body)
			this.FriendRewards(body.GetComponent<DeathRewards>());
			TeamIndex oldTeam = body ? body.teamComponent.teamIndex : TeamIndex.Monster;
			TeamIndex newTeam = this.master.teamIndex;
			master.teamIndex = newTeam;

			//indicator and dontdestroyonload
			FriendManager.instance.RpcSetupFriend(master.gameObject, false, this.isScepter);

			if (body)
            {
				body.teamComponent.teamIndex = newTeam;
				body.healthComponent.Networkhealth = body.healthComponent.fullCombinedHealth * 0.5f;
				Util.CleanseBody(body, false, false, false, true, true, false); // lol

				// manual cleanse cuz debuffs cleansebody only works on timed buffs
				BuffIndex buffIndex = (BuffIndex)0;
				BuffIndex buffCount = (BuffIndex)BuffCatalog.buffCount;
				while (buffIndex < buffCount)
				{
					BuffDef buffDef = BuffCatalog.GetBuffDef(buffIndex);
					if (buffDef.isDebuff)
					{
						body.ClearTimedBuffs(buffIndex);
						int count = body.GetBuffCount(buffIndex);
						if(count > 0)
                        {
							for (int i = 0; i < count; i++)
							{
								body.RemoveBuff(buffIndex);
							}
						}
						
					}
					buffIndex++;
				}
				body.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 3f);
				HauntedAffixFix(body, newTeam);
				BeadAffixFix(body, newTeam);
				//body.healthComponent.HealFraction(1f, default(ProcChainMask)); // BORING. HEAL IT YOURSELF WITH SECONDARY
			}

			// removes friend from combat squad so simulacrum and shit dont break
			List<CombatSquad> combatSquads = InstanceTracker.GetInstancesList<CombatSquad>();
			foreach(CombatSquad c in combatSquads)
            {
				if (c && c.ContainsMember(master))
				{
					c.RemoveMember(master);

					if(c.memberCount == 0 && !c.defeatedServer)
                    {
						c.TriggerDefeat();
                    }
				}
            }

			master.minionOwnership.SetOwner(this.master);			
			master.onBodyDeath.AddListener(OnFriendDeath);
			master.inventory.AddItemsFrom(this.master.inventory, this.ItemFilter);
			master.inventory.ResetItem(RoR2Content.Items.UseAmbientLevel);
			master.inventory.GiveItem(SS2Content.Items.ChirrFriendHelper, 1);
			master.inventory.GiveItem(SS2Content.Items.FlowerTurret, 1);

			if(!isScepter)
				master.inventory.GiveItem(SS2Content.Items.HealthDecayWithRegen, (int)healthDecayTime); // item stack = how long it takes to go from 100% health to 0

			if(oldTeam == TeamIndex.Void && master.inventory.GetEquipmentIndex() == DLC1Content.Equipment.EliteVoidEquipment.equipmentIndex) // UNDO VOIDTOUCHED
				master.inventory.SetEquipmentIndex(EquipmentIndex.None);
						
		}
		
		public void RemoveFriend([NotNull] CharacterMaster master)
		{
			if (!NetworkServer.active)
			{
				SS2Log.Warning("ChirrFriendTracker.RemoveFriend(CharacterMaster master) called on client.");
				return;
			}
			RemoveFriend(master, true);
		}

		public void RemoveFriend([NotNull] CharacterMaster master, bool useOldTeam) // useOldTeam should be false when friend's team change was forced (void infestor)
        {
			if (!NetworkServer.active)
			{
				SS2Log.Warning("ChirrFriendTracker.RemoveFriend(CharacterMaster master, bool wasStolen) called on client.");
				return;
			}
			if (this.currentFriend.master != null && this.currentFriend.master != master)
			{
				SS2Log.Warning("ChirrFriendTracker.RemoveFriend: " + master + " was not a friend");
				return;
			}
			//undo everything

			CharacterBody body = master.GetBody();

			// indicator and dontdestroyonload
			FriendManager.instance.RpcSetupFriend(master.gameObject, true, this.isScepter);

			TeamIndex newTeam = this.currentFriend.teamIndex;
			if(useOldTeam)
            {
				master.teamIndex = newTeam;
				if(body)
					body.teamComponent.teamIndex = newTeam;
			}			

			if (body)
			{
				HauntedAffixFix(body, newTeam);
				BeadAffixFix(body, newTeam);
				SetStateOnHurt setStateOnHurt = body.GetComponent<SetStateOnHurt>(); // stun is good
				if (setStateOnHurt) setStateOnHurt.SetStun(1f);
			}

			master.minionOwnership.SetOwner(this.master); // this apparently unsets the owner if you call it again
			master.onBodyDeath.RemoveListener(OnFriendDeath);

			master.inventory.RemoveItem(SS2Content.Items.ChirrFriendHelper, 1); // dont think its necessary. just being overly safe

			if(this.currentFriend.itemStacks != null && this.currentFriend.itemStacks.Length > 0) // ........
            {
				Inventory inventory = master.inventory;
				inventory.itemAcquisitionOrder.Clear();
				int[] array = inventory.itemStacks;
				int num = 0;
				HG.ArrayUtils.SetAll<int>(array, num);
				master.inventory.AddItemsFrom(this.currentFriend.itemStacks, new Func<ItemIndex, bool>(target => true)); // NRE HERE???? how
			}
				
			this.currentFriend = default(Friend);
		}


		//copypasted from deathrewards
		private void FriendRewards(DeathRewards deathRewards)
        {
			if (!NetworkServer.active || !deathRewards) return;

			Vector3 corePosition = deathRewards.characterBody.corePosition;
			uint num = deathRewards.goldReward;
			if (Run.instance.selectedDifficulty >= DifficultyIndex.Eclipse6)
			{
				num = (uint)(num * 0.8f);
			}
			TeamManager.instance.GiveTeamMoney(this.master.teamIndex, num);
			EffectManager.SpawnEffect(DeathRewards.coinEffectPrefab, new EffectData
			{
				origin = corePosition,
				genericFloat = deathRewards.goldReward,
				scale = deathRewards.characterBody.radius
			}, true);
			float num2 = 1f + (deathRewards.characterBody.level - 1f) * 0.3f;
			CharacterBody body = this.master.GetBody();
			if(body)
				ExperienceManager.instance.AwardExperience(corePosition, body, (ulong)((uint)(deathRewards.expReward * num2)));

			deathRewards.goldReward = 0;
			deathRewards.expReward = 0;
		}

		// changes haunted affix ward when changing teams
		private void HauntedAffixFix(CharacterBody body, TeamIndex newTeam)
        {
			CharacterBody.AffixHauntedBehavior component = body.GetComponent<CharacterBody.AffixHauntedBehavior>();
			if(component && component.affixHauntedWard)
            {
				component.affixHauntedWard.GetComponent<TeamFilter>().teamIndex = newTeam;
            }
        }

		private void BeadAffixFix(CharacterBody body, TeamIndex newTeam)
		{
			AffixBeadBehavior component = body.GetComponent<AffixBeadBehavior>();
			if(component && component.affixBeadWard)
            {
				component.affixBeadWard.GetComponent<TeamFilter>().teamIndex = newTeam;
            }
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