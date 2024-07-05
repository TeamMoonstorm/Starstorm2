using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using RoR2.Artifacts;
using RoR2.Skills;
using UnityEngine.Events;
using System.Runtime.CompilerServices;
namespace SS2
{
	// welcome to hardcode city
	// going to assume curses can't change mid stage. for my own sanity.
	// if something other than lunar gambler adds curses, then we need to track curse sources.
	public enum CurseIndex
	{
		None,
		ChestMonsters,
		ChestVelocity,
		ChestSpite,
		ChestTimer,
		MonsterArmor,
		MonsterAttackSpeed,
		MonsterCloak,
		MonsterCooldownReduction,
		MonsterDamage,
		MonsterHealth,
		MonsterMovementSpeed,
		MonsterShield,
		MonsterSpite,
		PlayerBlind,
		PlayerLock,
		PlayerMoney,
		PlayerRegen,
		TeleporterRadius,
		TeleporterRevive,
		TeleporterVoid
	}
	public static class CurseManager
    {		
		[SystemInitializer(typeof(BuffCatalog))]
		private static void Init()
		{
            Run.onRunStartGlobal += Refresh;
			Run.onRunDestroyGlobal += Refresh; //idk. just being cautious?    
			SS2Assets.LoadAsset<GameObject>("VoidTeleporterFogDamager", SS2Bundle.Interactables).GetComponent<FogDamageController>().dangerBuffDef = RoR2Content.Buffs.VoidFogMild;
		}

		public static Action<Run> onCursesRefreshed;
        private static void Refresh(Run run)
        {
			OnCurseEnd();

			curseStacks = new int[21];
			bombRequestQueue.Clear();
			lastMoneyPenalty = Run.FixedTimeStamp.now;
			totalCurses = 0;
			stageStart = 0;
			onCursesRefreshed?.Invoke(run);
        }
		private static void OnCurseBegin()
        {
			stageStart = Run.instance.stageClearCount;

            SceneDirector.onPrePopulateSceneServer += OnPrePopulateScene;
			Stage.onServerStageBegin += OnServerStageBegin;
			CharacterMaster.onStartGlobal += OnMasterStartGlobal;
			CharacterBody.onBodyStartGlobal += OnBodyStartGlobal;
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
			GlobalEventManager.OnInteractionsGlobal += OnInteractionGlobal;
			GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
			RoR2Application.onFixedUpdate += OnFixedUpdate;
			TeleporterInteraction.onTeleporterBeginChargingGlobal += OnTeleporterBeginChargingGlobal;
			R2API.RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
			On.RoR2.ChestBehavior.Awake += ChestBehavior_Awake;
			On.RoR2.ChestBehavior.ItemDrop += ChestBehavior_ItemDrop;
		}

        

        private static void OnCurseEnd()
		{
			SceneDirector.onPrePopulateSceneServer -= OnPrePopulateScene;
			Stage.onServerStageBegin -= OnServerStageBegin;
			CharacterMaster.onStartGlobal -= OnMasterStartGlobal;
			CharacterBody.onBodyStartGlobal -= OnBodyStartGlobal;
			GlobalEventManager.onServerDamageDealt -= OnServerDamageDealt;
			GlobalEventManager.OnInteractionsGlobal -= OnInteractionGlobal;
			GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeathGlobal;
			RoR2Application.onFixedUpdate -= OnFixedUpdate;
			TeleporterInteraction.onTeleporterBeginChargingGlobal -= OnTeleporterBeginChargingGlobal;
			R2API.RecalculateStatsAPI.GetStatCoefficients -= GetStatCoefficients;
			On.RoR2.ChestBehavior.Awake -= ChestBehavior_Awake;
			On.RoR2.ChestBehavior.ItemDrop -= ChestBehavior_ItemDrop;
		}

		// might need to network this
		public static void AddCurse(CurseIndex index, int count = 1)
        {
            if (!NetworkServer.active)
            {
				return;
            }
			bool hadAnyCurses = totalCurses > 0;
			int i = (int)index;
			if(i < pendingCurses.Length)
            {
				pendingCurses[i] += count;
				totalCurses += count;
            }
			if(totalCurses > 0 && !hadAnyCurses)
            {
				OnCurseBegin();
            }
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetCurseCount(CurseIndex index)
        {
			return curseStacks[(int)index];
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetTotal()
		{
			return totalCurses;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetCurseIntensity()
        {
			return Mathf.Pow(curseIntensityPerStage, GetStageClearCount());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetStageClearCount()
        {
			return totalCurses > 0 ? Run.instance.stageClearCount - stageStart : 0;
        }

		public static void ClearCurses()
        {
			OnCurseEnd();

			curseStacks = new int[21];
			bombRequestQueue.Clear();
			lastMoneyPenalty = Run.FixedTimeStamp.now;
			totalCurses = 0;
			stageStart = Run.instance.stageClearCount;
		}


		// this has to be before chests spawn so we can fuck with them properly
		private static void OnPrePopulateScene(SceneDirector obj)
		{
			for (int i = 0; i < pendingCurses.Length; i++)
			{
				curseStacks[i] += pendingCurses[i];
				pendingCurses[i] = 0;
			}
		}

		private static void OnServerStageBegin(Stage stage)
        {
			EffectData effectData = new EffectData
			{
				genericUInt = (uint)GetStageClearCount() + 1,
			};
			EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("CurseReminder", SS2Bundle.Interactables), effectData, true);

			cloakRng = new Xoroshiro128Plus(Run.instance.stageRng.nextUlong);
			lastMoneyPenalty = Run.FixedTimeStamp.now;
			bombRequestQueue.Clear();
        }

        private static void OnTeleporterBeginChargingGlobal(TeleporterInteraction teleporter)
        {
			int teleporterVoid = GetCurseCount(CurseIndex.TeleporterVoid);
			int teleporterRadius = GetCurseCount(CurseIndex.TeleporterRadius);
			int teleporterRevive = GetCurseCount(CurseIndex.TeleporterRevive);


			// TODO: PostProcessing for this? to match void fields and stuff
			if (teleporterVoid > 0)
            {
				GameObject gameObject = SS2Assets.LoadAsset<GameObject>("VoidTeleporterFogDamager", SS2Bundle.Interactables);
				if (gameObject)
				{
					GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, teleporter.transform.position, Quaternion.identity);
					NetworkServer.Spawn(gameObject2);
					FogDamageController fogDamageController = gameObject2.GetComponent<FogDamageController>();
					fogDamageController.AddSafeZone(teleporter.holdoutZoneController);
					fogDamageController.tickPeriodSeconds /= GetCurseIntensity();
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
					radius /= 2f * teleporterRadius * GetCurseIntensity();
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
						inventory.GiveItem(RoR2Content.Items.ExtraLife, teleporterRevive); // should this scale with GetCurseIntensity()?
					}
				}));
            }
        }
		private static void OnServerDamageDealt(DamageReport damageReport)
		{
			int bleedCount = GetCurseCount(CurseIndex.PlayerRegen); // not changing the name
			DamageInfo damageInfo = damageReport.damageInfo;
			CharacterBody victim = damageReport.victimBody;
			if(damageReport.victimTeamIndex == TeamIndex.Player && damageInfo.procCoefficient > 0)
            {
				if(bleedCount > 0)
                {
					//bleed for 20% of damage taken
					// declaring everything because im slow in the brain
					// TODO: CUSTOM DOT FOR THIS. BLEED SUX
					float desiredDamage = .2f * bleedCount * GetCurseIntensity() * damageInfo.damage;
					float compensated = desiredDamage;
					float bleedTicksPerSecond = 4f;
					float bleedDuration = 4f;
					if (damageReport.attackerBody) compensated /= damageReport.attackerBody.damage;
					float desiredPerSecond = compensated / bleedDuration;
					float desiredPerTick = desiredPerSecond / bleedTicksPerSecond;
					float compensatedForBleedDC = desiredPerTick / 0.2f;
					
					DotController.InflictDot(victim.gameObject, damageInfo.attacker, DotController.DotIndex.Bleed, bleedDuration, compensatedForBleedDC, null);
					EffectData effectData = new EffectData
					{
						origin = victim.corePosition,
					};
					effectData.SetNetworkedObjectReference(victim.gameObject);
					EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("BleedOnHurtEffect", SS2Bundle.Interactables), effectData, true);
				}	
			}				
		}
		private static void OnCharacterDeathGlobal(DamageReport damageReport)
        {
			if (damageReport.victimTeamIndex != TeamIndex.Monster)
			{
				return;
			}
			int monsterSpite = GetCurseCount(CurseIndex.MonsterSpite);
			CharacterBody victimBody = damageReport.victimBody;
			Vector3 corePosition = victimBody.corePosition;
			if (monsterSpite > 0)
            {				
				int num = Mathf.Min(BombArtifactManager.maxBombCount, Mathf.CeilToInt(victimBody.bestFitRadius * BombArtifactManager.extraBombPerRadius) * monsterSpite);
				num *= Mathf.RoundToInt(GetCurseIntensity());
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
					CurseManager.bombRequestQueue.Enqueue(item);
				}

				EffectData effectData = new EffectData
				{
					origin = damageReport.damageInfo.position,
				};
				EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("MonsterSpiteCurseEffect", SS2Bundle.Interactables), effectData, true);
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

				args.armorAdd += 25f * armor * GetCurseIntensity();
				args.attackSpeedMultAdd += .30f * attackspeed * GetCurseIntensity();
				args.cooldownMultAdd -= Util.ConvertAmplificationPercentageIntoReductionPercentage(25f * cooldownreduction * GetCurseIntensity()) / 100f;
				args.damageMultAdd += .30f * damage * GetCurseIntensity();
				args.healthMultAdd += .30f * health * GetCurseIntensity();
				args.moveSpeedMultAdd += .30f * movespeed * GetCurseIntensity();

				if (shield > 0)
					args.healthMultAdd -= 0.5f; //undo transendence hp. math is prob wrong. dont care.

			}
			// Player curses
			else
			{
				int blind = sender.GetBuffCount(SS2Content.Buffs.bdLunarCurseBlind);

				if (blind > 0)
					sender.visionDistance = 30f * Mathf.Pow(0.5f, blind) / GetCurseIntensity();
            }
		}

		private static void OnMasterStartGlobal(CharacterMaster characterMaster)
		{
			int monsterShield = GetCurseCount(CurseIndex.MonsterShield);
			if (monsterShield > 0)
			{
				characterMaster.inventory.GiveItem(RoR2Content.Items.ShieldOnly); // too lazy to ilhook lol
			}			
		}
		private static void OnBodyStartGlobal(CharacterBody characterBody)
        {
			int playerLock = GetCurseCount(CurseIndex.PlayerLock);
			int playerBlind = GetCurseCount(CurseIndex.PlayerBlind);
			int playerRegen = GetCurseCount(CurseIndex.PlayerRegen);
			int monsterArmor = GetCurseCount(CurseIndex.MonsterArmor);
			int monsterAttackSpeed = GetCurseCount(CurseIndex.MonsterAttackSpeed);
			int monsterCloak = GetCurseCount(CurseIndex.MonsterCloak);
			int monsterCooldownReduction = GetCurseCount(CurseIndex.MonsterCooldownReduction);
			int monsterDamage = GetCurseCount(CurseIndex.MonsterDamage);
			int monsterHealth = GetCurseCount(CurseIndex.MonsterHealth);
			int monsterMovementSpeed = GetCurseCount(CurseIndex.MonsterMovementSpeed);
			int monsterShield = GetCurseCount(CurseIndex.MonsterShield);

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
				
				if(cloakRng.nextNormalizedFloat > 0.5f / GetCurseIntensity())
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
			if(GetCurseCount(CurseIndex.ChestVelocity) > 0)
				EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("ChestVelocityCurseEffect", SS2Bundle.Interactables), new EffectData { origin = self.transform.position }, true);
		}

        private static void ChestBehavior_Awake(On.RoR2.ChestBehavior.orig_Awake orig, ChestBehavior self)
        {
            if(GetCurseCount(CurseIndex.ChestTimer) > 0)
            {
				self.openState = openState;
            }
			orig(self);
        }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetChestTimer()
        {
			return 30f * GetCurseCount(CurseIndex.ChestTimer) * GetCurseIntensity();
        }

        private static void OnInteractionGlobal(Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            if (!NetworkServer.active) return;
			int chestVelocity = GetCurseCount(CurseIndex.ChestVelocity);
			int chestMonsters = GetCurseCount(CurseIndex.ChestMonsters);
			int chestSpite = GetCurseCount(CurseIndex.ChestSpite);

			ChestBehavior chestBehavior = interactableObject.GetComponent<ChestBehavior>();
			Vector3 position = interactableObject.transform.position;
			if(chestBehavior)
            {
				
				PurchaseInteraction purchaseInteraction = chestBehavior.GetComponent<PurchaseInteraction>();
				if (chestVelocity > 0)
				{
					chestBehavior.dropForwardVelocityStrength = 40 * chestVelocity * GetCurseIntensity();
					chestBehavior.dropUpVelocityStrength = 60 * chestVelocity * GetCurseIntensity();
				}
				if (chestMonsters > 0)
				{
					GameObject gameObject = SS2Assets.LoadAsset<GameObject>("MonstersOnChestOpenEncounter", SS2Bundle.Interactables);
					if (gameObject)
					{
						GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, position, Quaternion.identity);
						NetworkServer.Spawn(gameObject2);
						CombatDirector combatDirector = gameObject2.GetComponent<CombatDirector>();
						if (combatDirector && Stage.instance)
						{
							float monsterCredit = 40f * Stage.instance.entryDifficultyCoefficient * chestMonsters * GetCurseIntensity();
							DirectorCard directorCard = combatDirector.SelectMonsterCardForCombatShrine(monsterCredit);
							if (directorCard != null)
							{
								combatDirector.CombatShrineActivation(interactor, monsterCredit, directorCard);
								EffectData effectData = new EffectData
								{
									origin = position,
								};
								EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("MonstersOnChestOpen", SS2Bundle.Interactables), effectData, true);
								return;
							}
							NetworkServer.Destroy(gameObject2);
						}
					}					
				}
				if (chestSpite > 0)
				{
					int chestValue = purchaseInteraction.cost / Run.instance.GetDifficultyScaledCost(25, Stage.instance.entryDifficultyCoefficient);
					chestValue = Mathf.Max(chestValue, 1);
					int bombCount = chestValue * 6 * Mathf.RoundToInt(GetCurseIntensity());
					SS2Log.Info("ChestSpite bombCount=" + bombCount);
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

		private static void OnFixedUpdate()
		{
			if (!NetworkServer.active) return;

			if (CurseManager.bombRequestQueue.Count > 0)
			{
				BombArtifactManager.BombRequest bombRequest = CurseManager.bombRequestQueue.Dequeue();
				Ray ray = new Ray(bombRequest.raycastOrigin + new Vector3(0f, BombArtifactManager.maxBombStepUpDistance, 0f), Vector3.down);
				float maxDistance = BombArtifactManager.maxBombStepUpDistance + BombArtifactManager.maxBombFallDistance;
				RaycastHit raycastHit;
				if (Physics.Raycast(ray, out raycastHit, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
				{
					BombArtifactManager.SpawnBomb(bombRequest, raycastHit.point.y);
				}
			}

			int playerMoney = GetCurseCount(CurseIndex.PlayerMoney);
			if (playerMoney > 0)
            {
				if(lastMoneyPenalty.timeSince > 30f / playerMoney)
                {
					lastMoneyPenalty = Run.FixedTimeStamp.now;
					foreach (TeamComponent t in TeamComponent.GetTeamMembers(TeamIndex.Player))
                    {
						if (!t.body.isPlayerControlled) continue;

						uint moneyPenalty = (uint)Mathf.Min(t.body.master.money, Run.instance.GetDifficultyScaledCost(25, Stage.instance.entryDifficultyCoefficient) * GetCurseIntensity());
						t.body.master.money -= moneyPenalty;

						EffectManager.SpawnEffect(DeathRewards.coinEffectPrefab, new EffectData
						{
							origin = t.body.corePosition,
							genericFloat = moneyPenalty,
							scale = t.body.radius
						}, true);
						EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("PlayerMoneyCurseEffect", SS2Bundle.Interactables), new EffectData { origin = t.transform.position }, true);
					}					
                }
            }
		}

		

		private static Xoroshiro128Plus cloakRng = new Xoroshiro128Plus(0UL);
		private static Run.FixedTimeStamp lastMoneyPenalty;
		private static Color teleporterRadiusColor = new Color(0f, 3.9411764f, 5f, 1f);
		private static readonly EntityStates.SerializableEntityStateType openState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Barrel.OpeningSlow));
		private static readonly Queue<BombArtifactManager.BombRequest> bombRequestQueue = new Queue<BombArtifactManager.BombRequest>();

		private static int[] pendingCurses = new int[21];
		private static int[] curseStacks = new int[21];
		private static int totalCurses;

		private static int stageStart;
		private static readonly float curseIntensityPerStage = 1.33f;
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

				disableInterval = 16f / (1 + (GetCurseCount(CurseIndex.PlayerLock)-1)) / GetCurseIntensity();
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

					for (int i = disabledSkills.Count - 1; i >= 0; i--)
					{
						DisabledSkill skill = disabledSkills[i];
						if (skill.startTime.timeSince > disableDuration)
						{
							EnableSkill(skill.target);
							disabledSkills.RemoveAt(i);
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
				skill.UnsetSkillOverride(this, disabledSkillDef, GenericSkill.SkillOverridePriority.Replacement);
			}
        }

	}
}
