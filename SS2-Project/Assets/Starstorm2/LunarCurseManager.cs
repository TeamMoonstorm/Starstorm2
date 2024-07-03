using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using RoR2.Artifacts;
using RoR2.Skills;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
namespace SS2
{
	// going to assume curses can't change mid stage. for my own sanity.
    public static class LunarCurseManager
    {
		[SystemInitializer(new Type[]
		{

		})]
		private static void Init()
		{
            Stage.onServerStageBegin += OnServerStageBegin;
            CharacterMaster.onStartGlobal += OnMasterStartGlobal;
            CharacterBody.onBodyStartGlobal += OnBodyStartGlobal;
            GlobalEventManager.OnInteractionsGlobal += OnInteractionGlobal;
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
            RoR2Application.onFixedUpdate += OnFixedUpdate;
            TeleporterInteraction.onTeleporterBeginChargingGlobal += OnTeleporterBeginChargingGlobal;
            R2API.RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
            On.RoR2.ChestBehavior.Awake += ChestBehavior_Awake;
            On.RoR2.ChestBehavior.ItemDrop += ChestBehavior_ItemDrop;

			SS2Assets.LoadAsset<GameObject>("VoidTeleporterFogDamager", SS2Bundle.Interactables).GetComponent<FogDamageController>().dangerBuffDef = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Common/bdVoidFogMild").WaitForCompletion();
		}

        private static void OnServerStageBegin(Stage stage)
        {
			cloakRng = new Xoroshiro128Plus(Run.instance.stageRng.nextUlong);
            if(true) // curse is active, stage is valid
            {
				curseIntensity *= curseIntensityPerStage;
            }
        }

        private static void OnTeleporterBeginChargingGlobal(TeleporterInteraction teleporter)
        {
            if (teleporterVoid > 0)
            {
				GameObject gameObject = SS2Assets.LoadAsset<GameObject>("VoidTeleporterFogDamager", SS2Bundle.Interactables);
				if (gameObject)
				{
					GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, teleporter.transform.position, Quaternion.identity);
					NetworkServer.Spawn(gameObject2);
					FogDamageController fogDamageController = gameObject2.GetComponent<FogDamageController>();
					fogDamageController.AddSafeZone(teleporter.holdoutZoneController);
					fogDamageController.tickPeriodSeconds /= curseIntensity;
					EffectData effectData = new EffectData
					{
						origin = teleporter.transform.position,
					};
					EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("VoidTeleporterCurseEffect", SS2Bundle.Interactables), effectData, true);
				}
			}
			if (teleporterRadius > 0)
            {
				teleporter.holdoutZoneController.calcRadius += (ref float radius) =>
				{
					radius /= 2f * teleporterRadius * curseIntensity;
				};

				teleporter.holdoutZoneController.calcColor += (ref Color color) =>
				{
					color = teleporterRadiusColor;
				};
				EffectData effectData = new EffectData
				{
					origin = teleporter.transform.position,
				};
				EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("TeleporterRadiusCurseEffect", SS2Bundle.Interactables), effectData, true);
			}
			if (teleporterRevive > 0)
            {
				teleporter.bossDirector.onSpawnedServer.AddListener(new UnityAction<GameObject>((masterObject) =>
				{
					Inventory inventory = masterObject.GetComponent<Inventory>();
					if (inventory)
					{
						inventory.GiveItem(RoR2Content.Items.ExtraLife, teleporterRevive); // should this scale with curseIntensity?
					}
				}));
            }
        }

        private static void OnCharacterDeathGlobal(DamageReport damageReport)
        {
			if (damageReport.victimTeamIndex != TeamIndex.Monster)
			{
				return;
			}
			CharacterBody victimBody = damageReport.victimBody;
			Vector3 corePosition = victimBody.corePosition;
			int num = Mathf.Min(BombArtifactManager.maxBombCount, Mathf.CeilToInt(victimBody.bestFitRadius * BombArtifactManager.extraBombPerRadius * monsterSpite));
			num *= Mathf.RoundToInt(curseIntensity);
			for (int i = 0; i < num; i++)
			{
				Vector3 b = UnityEngine.Random.insideUnitSphere * (BombArtifactManager.bombSpawnBaseRadius + victimBody.bestFitRadius * BombArtifactManager.bombSpawnRadiusCoefficient);
				BombArtifactManager.BombRequest item = new BombArtifactManager.BombRequest
				{
					spawnPosition = corePosition,
					raycastOrigin = corePosition + b,
					bombBaseDamage = victimBody.damage * 1.5f,
					attacker = victimBody.gameObject,
					teamIndex = damageReport.victimTeamIndex,
					velocityY = UnityEngine.Random.Range(5f, 25f)
				};
				LunarCurseManager.bombRequestQueue.Enqueue(item);
			}
		}

        private static void GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
			// Monster curses
			if(sender.teamComponent.teamIndex != TeamIndex.Player)
            {
				int armor = sender.GetBuffCount(SS2Content.Buffs.bdLunarCurseArmor);
				int attackspeed = sender.GetBuffCount(SS2Content.Buffs.bdLunarCurseAttackSpeed);
				int cooldownreduction = sender.GetBuffCount(SS2Content.Buffs.bdLunarCurseCooldownReduction);
				int damage = sender.GetBuffCount(SS2Content.Buffs.bdLunarCurseDamage);
				int health = sender.GetBuffCount(SS2Content.Buffs.bdLunarCurseHealth);
				int movespeed = sender.GetBuffCount(SS2Content.Buffs.bdLunarCurseMovementSpeed);
				int shield = sender.GetBuffCount(SS2Content.Buffs.bdLunarCurseShield);

				args.armorAdd += 25f * armor * curseIntensity;
				args.attackSpeedMultAdd += .25f * attackspeed * curseIntensity;
				args.cooldownMultAdd -= Util.ConvertAmplificationPercentageIntoReductionPercentage(25f * cooldownreduction * curseIntensity) / 100f;
				args.damageMultAdd += .25f * damage * curseIntensity;
				args.healthMultAdd += .25f * health * curseIntensity;
				args.moveSpeedMultAdd += .25f * movespeed * curseIntensity;

				if (shield > 0)
					args.healthMultAdd -= 0.5f; //undo transendence hp. math is prob wrong. dont care.

			}
			// Player curses
			else
			{
				int blind = sender.GetBuffCount(SS2Content.Buffs.bdLunarCurseBlind);

				if (blind > 0)
					sender.visionDistance = 30f * Mathf.Pow(0.5f, blind) / curseIntensity;
            }
		}

		private static void OnMasterStartGlobal(CharacterMaster characterMaster)
		{
			if (characterMaster.teamIndex == TeamIndex.Player)
            {
				if(playerRegen > 0)
                {
					float timeUntilDeath = 180f * Mathf.Pow(0.67f, playerRegen);
					characterMaster.inventory.GiveItem(RoR2Content.Items.HealthDecay, (int)timeUntilDeath);
				}
				return;
            }

			if (monsterShield > 0)
			{
				characterMaster.inventory.GiveItem(RoR2Content.Items.ShieldOnly); // too lazy to ilhook lol
			}			
		}
		private static void OnBodyStartGlobal(CharacterBody characterBody)
        {
			if (playerLock > 0 && characterBody.teamComponent.teamIndex == TeamIndex.Player)
			{
				characterBody.gameObject.AddComponent<SkillDisableBehavior>();
			}

			if (!NetworkServer.active) return;

			if(characterBody.teamComponent.teamIndex != TeamIndex.Player)
            {
				if (characterBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless)) return;

                AddBuff(characterBody, SS2Content.Buffs.bdLunarCurseArmor, monsterArmor);
				AddBuff(characterBody, SS2Content.Buffs.bdLunarCurseAttackSpeed, monsterAttackSpeed);
				AddBuff(characterBody, SS2Content.Buffs.bdLunarCurseCloak, monsterCloak);
				AddBuff(characterBody, SS2Content.Buffs.bdLunarCurseCooldownReduction, monsterCooldownReduction);
				AddBuff(characterBody, SS2Content.Buffs.bdLunarCurseDamage, monsterDamage);
				AddBuff(characterBody, SS2Content.Buffs.bdLunarCurseHealth, monsterHealth);
				AddBuff(characterBody, SS2Content.Buffs.bdLunarCurseMovementSpeed, monsterMovementSpeed);
				AddBuff(characterBody, SS2Content.Buffs.bdLunarCurseShield, monsterShield);		
				
				if(cloakRng.nextNormalizedFloat > 0.5f / curseIntensity)
                {
					characterBody.AddBuff(RoR2Content.Buffs.Cloak);
                }
			}
			else
            {
				AddBuff(characterBody, SS2Content.Buffs.bdLunarCurseBlind, playerBlind);
				AddBuff(characterBody, SS2Content.Buffs.bdLunarCurseBlind, playerRegen);				
			}
        }

		private static void AddBuff(CharacterBody body, BuffDef buff, int count)
        {
			if (count <= 0) return;
			for(int i = 0; i < count; i++)
            {
				body.AddBuff(buff);
            }
        }
        private static void ChestBehavior_ItemDrop(On.RoR2.ChestBehavior.orig_ItemDrop orig, ChestBehavior self)
        {
			orig(self);
			if(chestVelocity > 0)
				EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("ChestVelocityCurseEffect", SS2Bundle.Interactables), new EffectData { origin = self.transform.position }, true);
		}

        private static void ChestBehavior_Awake(On.RoR2.ChestBehavior.orig_Awake orig, ChestBehavior self)
        {
            if(chestTimer > 0)
            {
				self.openState = openState;
            }
			orig(self);
        }
		public static float GetChestTimer()
        {
			return chestTimer > 0 ? 30f * chestTimer * curseIntensity : 1.5f;
        }

        private static void OnInteractionGlobal(Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            if (!NetworkServer.active) return;

			ChestBehavior chestBehavior = interactableObject.GetComponent<ChestBehavior>();
			Vector3 position = interactableObject.transform.position;
			if(chestBehavior)
            {
				PurchaseInteraction purchaseInteraction = chestBehavior.GetComponent<PurchaseInteraction>();
				if (chestVelocity > 0)
				{
					chestBehavior.dropForwardVelocityStrength = 50 * chestVelocity * curseIntensity;
					chestBehavior.dropUpVelocityStrength = 80 * chestVelocity * curseIntensity;
				}

				if (chestMonsters > 0)
				{
					GameObject gameObject = SS2Assets.LoadAsset<GameObject>("MonsterOnChestOpenEncounter", SS2Bundle.Interactables);
					if (gameObject)
					{
						GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, position, Quaternion.identity);
						NetworkServer.Spawn(gameObject2);
						CombatDirector combatDirector = gameObject2.GetComponent<CombatDirector>();
						if (combatDirector && Stage.instance)
						{
							float monsterCredit = 40f * Stage.instance.entryDifficultyCoefficient * chestMonsters * curseIntensity;
							DirectorCard directorCard = combatDirector.SelectMonsterCardForCombatShrine(monsterCredit);
							if (directorCard != null)
							{
								combatDirector.CombatShrineActivation(interactor, monsterCredit, directorCard);
								EffectData effectData = new EffectData
								{
									origin = position,
								};
								EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("MonsterOnChestOpen", SS2Bundle.Interactables), effectData, true);
								return;
							}
							NetworkServer.Destroy(gameObject2);
						}
					}

					if(chestSpite > 0)
                    {
						int chestValue = purchaseInteraction.cost / Run.instance.GetDifficultyScaledCost(25, Stage.instance.entryDifficultyCoefficient);
						int bombCount = chestValue * 6 * Mathf.RoundToInt(curseIntensity);
						Vector3 corePosition = chestBehavior.dropTransform.position;
						float damage = 12f + (2.4f * Run.instance.ambientLevel);
						EffectData effectData = new EffectData
						{
							origin = position,
						};
						EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("BombsOnChestOpen", SS2Bundle.Interactables), effectData, true);
						for (int i = 0; i < bombCount; i++)
                        {
							bombRequestQueue.Enqueue(new BombArtifactManager.BombRequest
							{
								attacker = null,
								spawnPosition = corePosition,
								raycastOrigin = corePosition + UnityEngine.Random.insideUnitSphere * 3,
								bombBaseDamage = damage * 1.5f,
								teamIndex = TeamIndex.Monster,
								velocityY = UnityEngine.Random.Range(5f, 25f)
							});

						}						
                    }
				}
			}			
        }

		private static void OnFixedUpdate()
		{
			if (!NetworkServer.active) return;

			if (LunarCurseManager.bombRequestQueue.Count > 0)
			{
				BombArtifactManager.BombRequest bombRequest = BombArtifactManager.bombRequestQueue.Dequeue();
				Ray ray = new Ray(bombRequest.raycastOrigin + new Vector3(0f, BombArtifactManager.maxBombStepUpDistance, 0f), Vector3.down);
				float maxDistance = BombArtifactManager.maxBombStepUpDistance + BombArtifactManager.maxBombFallDistance;
				RaycastHit raycastHit;
				if (Physics.Raycast(ray, out raycastHit, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
				{
					BombArtifactManager.SpawnBomb(bombRequest, raycastHit.point.y);
				}
			}

			if(playerMoney > 0)
            {
				if(lastMoneyPenalty.timeSince > 30f)
                {
					lastMoneyPenalty = Run.FixedTimeStamp.now;
					foreach (TeamComponent t in TeamComponent.GetTeamMembers(TeamIndex.Player))
                    {
						if (!t.body.isPlayerControlled) continue;

						uint moneyPenalty = (uint)Mathf.Min(t.body.master.money, Run.instance.GetDifficultyScaledCost(25, Stage.instance.entryDifficultyCoefficient) * curseIntensity);
						t.body.master.money -= moneyPenalty;

						EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("PlayerMoneyCurseEffect", SS2Bundle.Interactables), new EffectData { origin = t.transform.position }, true);
					}					
                }
            }
		}

		private static float curseIntensity = 1;
		private static readonly float curseIntensityPerStage = 1.33f;

		private static Xoroshiro128Plus cloakRng = new Xoroshiro128Plus(0UL);
		private static Run.FixedTimeStamp lastMoneyPenalty;
		private static Color teleporterRadiusColor = new Color(0f, 3.9411764f, 5f, 1f);
		private static readonly EntityStates.SerializableEntityStateType openState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Barrel.OpeningSlow));
		private static readonly Queue<BombArtifactManager.BombRequest> bombRequestQueue = new Queue<BombArtifactManager.BombRequest>();

		private static int chestMonsters;
		private static int chestVelocity;
		private static int chestSpite;
		private static int chestTimer;

		private static int monsterArmor;
		private static int monsterAttackSpeed;
		private static int monsterCloak;
		private static int monsterCooldownReduction;
		private static int monsterDamage;
		private static int monsterHealth;
		private static int monsterMovementSpeed;
		private static int monsterShield;
		private static int monsterSpite;

		private static int playerBlind;
		private static int playerLock;
		private static int playerMoney;
		private static int playerRegen;

		private static int teleporterRadius;
		private static int teleporterVoid;
		private static int teleporterRevive;

		private class SkillDisableBehavior : MonoBehaviour
        {
			private static SkillDef disabledSkillDef = SS2Assets.LoadAsset<SkillDef>("DisabledSkill", SS2Bundle.Shared);
			private SkillLocator skillLocator;
			private List<DisabledSkill> disabledSkills;
			private float disableInterval;
			private float disableDuration = 16f;
			private float disableStopwatch;

			private Xoroshiro128Plus rng;

			public struct DisabledSkill
            {
				public GenericSkill target;
				public Run.FixedTimeStamp startTime;
            }
            private void Start()
            {
				rng = new Xoroshiro128Plus(Run.instance.seed ^ (ulong)base.GetInstanceID());
				disabledSkills = new List<DisabledSkill>();
				skillLocator = base.GetComponent<SkillLocator>();
				if(!skillLocator)
                {
					Destroy(this);
					return;
                }

				disableInterval = 16f / (1 + (playerLock-1)) / curseIntensity;
            }
            private void FixedUpdate()
            {
				if(Util.HasEffectiveAuthority(base.gameObject))
                {
					disableStopwatch -= Time.fixedDeltaTime;
					if (disableStopwatch <= 0f)
					{
						disableStopwatch += disableInterval;
						int index = rng.RangeInt(0, 4);
						GenericSkill target;
						switch (index)
						{
							case 0: target = skillLocator.primary; break;
							case 1: target = skillLocator.secondary; break;
							case 2: target = skillLocator.utility; break;
							case 3: target = skillLocator.special; break;
							default: target = skillLocator.special; break;
						}
						DisableSkill(target);

						EffectData effectData = new EffectData
						{
							origin = skillLocator.primary.characterBody.corePosition, // lol wut
						};
						EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("PlayerSkillCurseEffect", SS2Bundle.Interactables), effectData, true);
					}

					foreach (DisabledSkill skill in disabledSkills)
					{
						if (skill.startTime.timeSince > disableDuration)
						{
							EnableSkill(skill.target);
						}
					}
				}				
            }

			private void DisableSkill(GenericSkill skill)
            {				
				DisabledSkill dskill = new DisabledSkill { startTime = Run.FixedTimeStamp.now, target = skill };
				for (int i = 0; i < disabledSkills.Count; i++)
				{
					if(disabledSkills[i].target == skill)
                    {
						disabledSkills[i] = dskill;
						return;
                    }
				}

				skill.SetSkillOverride(this, disabledSkillDef, GenericSkill.SkillOverridePriority.Replacement);
				disabledSkills.Add(dskill);
			}

			private void EnableSkill(GenericSkill skill)
            {
				for (int i = 0; i < disabledSkills.Count; i++)
				{
					if (disabledSkills[i].target == skill)
					{
						disabledSkills.RemoveAt(i);
					}
				}

				skill.UnsetSkillOverride(this, disabledSkillDef, GenericSkill.SkillOverridePriority.Replacement);
			}
        }

	}
}
