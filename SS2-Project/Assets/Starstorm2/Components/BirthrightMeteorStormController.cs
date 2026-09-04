using System;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using SS2.Items;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace SS2.Components
{
    public class BirthrightMeteorStormController : MonoBehaviour
    {
        private int aliveChimeras => Util.GetItemCountGlobal(SS2Content.Items.BirthrightChimeraHelper.itemIndex, true);
        private int chimeraCount;
        private CharacterSpawnCard golemSpawnCard;
        private CharacterSpawnCard wispSpawnCard;
        private GameObject warningEffectPrefab;
        private GameObject impactEffectPrefab;
        private float waveTimer;
        private List<ChimeraSpawn> chimeraSpawns = new List<ChimeraSpawn>();
        
        public class ChimeraSpawn
        {
            public float detonationTime;
            public Vector3 impactPos;
        }

        public float impactDelay = 1f;
        public float blastRadius = 8f;
        public float blastDamageCoefficient = 3f;
        public float blastForce = 4000f;
        public DamageTypeCombo blastDamageType;

        private void Awake()
        {
            if (!NetworkServer.active) return;
            
            golemSpawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/LunarGolem/cscLunarGolem.asset").WaitForCompletion();
            wispSpawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/LunarWisp/cscLunarWisp.asset").WaitForCompletion();
         
            warningEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Meteor/MeteorStrikePredictionEffect.prefab").WaitForCompletion();
            impactEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Meteor/MeteorStrikeImpact.prefab").WaitForCompletion();
        }

        private void OnEnable()
        {
            if (!NetworkServer.active) return;
            
            //add meteors at each birthright because i am evil <3., 
            foreach (PurchaseInteraction birthrightPurchaseInteraction in PrimalBirthrightObjectiveToken.instanceList)
            {
                ChimeraSpawn chimeraSpawn = new ChimeraSpawn();
                chimeraSpawn.impactPos = birthrightPurchaseInteraction.gameObject.transform.position;
                    
                Vector3 origin = chimeraSpawn.impactPos + Vector3.up * 6f;
                Vector3 onUnitSphere = Random.onUnitSphere;
                onUnitSphere.y = -1f;
                if (Physics.Raycast(origin, onUnitSphere, out RaycastHit hitInfo, 12f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    chimeraSpawn.impactPos = hitInfo.point;
                }
                else if (Physics.Raycast(chimeraSpawn.impactPos, Vector3.down, out hitInfo, float.PositiveInfinity, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    chimeraSpawn.impactPos = hitInfo.point;
                }
                    
                SpawnChimera(chimeraSpawn);
            }
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active) return;
            
            for (int i = chimeraSpawns.Count - 1; i >= 0; i--)
            {
                ChimeraSpawn chimeraSpawn = chimeraSpawns[i];
                if (chimeraSpawn.detonationTime < Run.instance.time)
                {
                    chimeraSpawns.RemoveAt(i);
                    SpawnChimera(chimeraSpawn);
                }
            }
            
            waveTimer -= Time.fixedDeltaTime;
            if (waveTimer <= 0)
            {
                waveTimer = Random.Range(PrimalBirthright.chimeraWaitTime - PrimalBirthright.chimeraWaitTimeVariance, PrimalBirthright.chimeraWaitTime + PrimalBirthright.chimeraWaitTimeVariance);
                if (PrimalBirthrightObjectiveToken.instanceList.Count > 0 && aliveChimeras < 15)
                {
                    waveTimer *= 1.15f; // players probably taking a bit to clear them lets be slightly nice ,.., 
                }
                else if (PrimalBirthrightObjectiveToken.instanceList.Count == 0)
                {
                    waveTimer *= 1.4f; // take longer between waves if the player got all birthrights ,.,. 
                }
                
                foreach (CharacterBody body in PlayerCharacterMasterController._instancesReadOnly.Select(pcmc => pcmc.master?.GetBody()))
                {
                    int spawnCount = Random.Range(1, 4);
                    
                    for (int i = 0; i < spawnCount; i++)
                    {
                        ChimeraSpawn chimeraSpawn = new ChimeraSpawn();
                        if (body)
                        {
                            //once i had one of these spawn inside one of the arches in titanic plains .,., no clue how ,. ,.,. https://files.catbox.moe/v1as12.mp4 ,,.,
                            int randomVariance = 40;
                            Vector3 nodeFindPos = new Vector3(body.corePosition.x + Random.Range(-randomVariance, randomVariance), body.corePosition.y + Random.Range(-randomVariance, randomVariance), body.corePosition.z + Random.Range(-randomVariance, randomVariance));
                            SceneInfo.instance.groundNodes.GetNodePosition(SceneInfo.instance.groundNodes.FindClosestNode(nodeFindPos, HullClassification.Golem), out chimeraSpawn.impactPos);
                        }
                        
                        EffectManager.SpawnEffect(warningEffectPrefab, new EffectData
                        {
                            origin = chimeraSpawn.impactPos,
                            scale = blastRadius
                        }, transmit: true);
                        
                        chimeraSpawn.detonationTime = Run.instance.time + impactDelay;
                        chimeraSpawns.Add(chimeraSpawn);
                    }
                }
            }
        }
        
        private void SpawnChimera(ChimeraSpawn chimeraSpawn)
        {
            SS2Log.Debug("spawning evil ,., ");

            if (aliveChimeras < 15)
            {
                CharacterSpawnCard spawnCard = golemSpawnCard;
                if (PrimalBirthright.lunarWispStageCount <= Run.instance.stageClearCount + 1 && Run.instance.spawnRng.RangeFloat(0, 100) < PrimalBirthright.lunarWispChance)
                {
                    spawnCard = wispSpawnCard;
                }
                
                DirectorSpawnRequest spawnRequest = new DirectorSpawnRequest(
                    spawnCard,
                    new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Direct,
                        position = chimeraSpawn.impactPos
                    },
                    RoR2Application.rng
                );
                spawnRequest.teamIndexOverride = PrimalBirthright.useLunarTeam ? TeamIndex.Lunar : TeamIndex.Monster;
                spawnRequest.onSpawnedServer += result =>
                {
                    // i had a single NRE on client while testing i dont trust spawnedserver anymore ,.. 
                    if (!NetworkServer.active) return;
                    
                    CharacterMaster golemMaster = result.spawnedInstance?.GetComponent<CharacterMaster>();
                    if (!golemMaster) return;
                    
                    golemMaster.inventory.GiveItemPermanent(SS2Content.Items.BirthrightChimeraHelper.itemIndex);
                    
                    //basically just copy how combat director rewards stuff .,,. 
                    DeathRewards deathRewards = golemMaster.GetBodyObject()?.GetComponent<DeathRewards>();
                    if (!deathRewards) return;
                    
                    float total = 6f; //this is around 12 gold which isnt much but shouldnt be rewarded too much ,.,. 
                    deathRewards.spawnValue = (int)Mathf.Max(1f, total);
             
                    deathRewards.expReward = (uint)Mathf.Max(1f, total * Run.instance.compensatedDifficultyCoefficient);
                    deathRewards.goldReward = (uint)Mathf.Max(1f, total * 2f * Run.instance.compensatedDifficultyCoefficient);
                };
                
                DirectorCore.instance.TrySpawnObject(spawnRequest);
            }
     
            EffectData effectData = new EffectData
            {
                origin = chimeraSpawn.impactPos
            };
            EffectManager.SpawnEffect(impactEffectPrefab, effectData, transmit: true);
            
            //lunar golem base damage + level damage .,., not magic numbers i promise !!
            uint damage = 35 + 7 * TeamManager.instance.GetTeamLevel(TeamIndex.Monster);
            BlastAttack blastAttack = new BlastAttack
            {
                inflictor = base.gameObject,
                baseDamage = blastDamageCoefficient * damage,
                baseForce = blastForce,
                attackerFiltering = AttackerFiltering.Default,
                crit = false,
                falloffModel = BlastAttack.FalloffModel.Linear,
                attacker = null,
                bonusForce = Vector3.zero,
                damageColorIndex = PrimalBirthright.primalStormDamageColor,
                position = chimeraSpawn.impactPos,
                procChainMask = default,
                procCoefficient = 1f,
                teamIndex = PrimalBirthright.useLunarTeam ? TeamIndex.Lunar : TeamIndex.Monster,
                radius = blastRadius,
                damageType = blastDamageType
            };
            blastAttack.Fire();
        }
    }
}