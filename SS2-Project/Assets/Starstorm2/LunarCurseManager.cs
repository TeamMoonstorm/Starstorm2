using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using RoR2.Artifacts;
namespace SS2
{
    public static class LunarCurseManager
    {
		[SystemInitializer(new Type[]
		{

		})]
		private static void Init()
		{
            CharacterBody.onBodyStartGlobal += OnBodyStartGlobal;
            GlobalEventManager.OnInteractionsGlobal += OnInteractionGlobal;
            RoR2Application.onFixedUpdate += ProcessBombQueue;
            On.RoR2.ChestBehavior.Awake += ChestBehavior_Awake;
            On.RoR2.ChestBehavior.ItemDrop += ChestBehavior_ItemDrop;
		}

        private static void OnBodyStartGlobal(CharacterBody characterBody)
        {
			if (!NetworkServer.active) return;

			if(characterBody.teamComponent.teamIndex != TeamIndex.Player && !characterBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless))
            {

                AddBuff(characterBody, SS2Content.Buffs.bdLunarCurseArmor, monsterArmor);
				AddBuff(characterBody, SS2Content.Buffs.bdLunarCurseAttackSpeed, monsterAttackSpeed);
				AddBuff(characterBody, SS2Content.Buffs.bdLunarCurseCloak, monsterCloak);
				AddBuff(characterBody, SS2Content.Buffs.bdLunarCurseCooldownReduction, monsterArmor);
				AddBuff(characterBody, SS2Content.Buffs.bdLunarCurseArmor, monsterCooldownReduction);
				AddBuff(characterBody, SS2Content.Buffs.bdLunarCurseDamage, monsterDamage);
				AddBuff(characterBody, SS2Content.Buffs.bdLunarCurseHealth, monsterHealth);
				AddBuff(characterBody, SS2Content.Buffs.bdLunarCurseMovementSpeed, monsterMovementSpeed);
				AddBuff(characterBody, SS2Content.Buffs.bdLunarCurseShield, monsterShield);

			}
			else if(characterBody.teamComponent.teamIndex == TeamIndex.Player)
            {

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
			EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("ChestVelocityCurseEffect", SS2Bundle.Interactables), new EffectData { origin = self.transform.position }, true);
		}

        private static void ChestBehavior_Awake(On.RoR2.ChestBehavior.orig_Awake orig, ChestBehavior self)
        {
            if(chestTimerCount > 0)
            {
				self.openState = openState;
            }
			orig(self);
        }

        private static void OnInteractionGlobal(Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            if (!NetworkServer.active) return;

			ChestBehavior chestBehavior = interactableObject.GetComponent<ChestBehavior>();
			CharacterBody body = interactor.GetComponent<CharacterBody>();
			Vector3 position = interactableObject.transform.position;
			if(chestBehavior)
            {
				PurchaseInteraction purchaseInteraction = chestBehavior.GetComponent<PurchaseInteraction>();
				if (chestVelocity > 0)
				{
					chestBehavior.dropForwardVelocityStrength = 50 * chestVelocity;
					chestBehavior.dropUpVelocityStrength = 80 * chestVelocity;
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
							float monsterCredit = 40f * Stage.instance.entryDifficultyCoefficient * chestMonsters;
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
						int bombCount = chestValue * 6;
						Vector3 corePosition = chestBehavior.dropTransform.position;
						float damage = 12f + (2.4f * Run.instance.ambientLevel);

						for(int i = 0; i < bombCount; i++)
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

		private static void ProcessBombQueue()
		{
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
		}

		private static readonly EntityStates.SerializableEntityStateType openState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Barrel.OpeningSlow));
		private static readonly Queue<BombArtifactManager.BombRequest> bombRequestQueue = new Queue<BombArtifactManager.BombRequest>();
		private static int chestMonsters;
		private static int chestVelocity;
		private static int chestSpite;
		public static readonly int chestTimerCount;

		private static int monsterArmor;
		private static int monsterAttackSpeed;
		private static int monsterCloak;
		private static int monsterCooldownReduction;
		private static int monsterDamage;
		private static int monsterHealth;
		private static int monsterMovementSpeed;
		private static int monsterShield;

		private static int monsterSpite;

	}
}
