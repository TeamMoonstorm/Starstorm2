using EntityStates;
using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using SS2;
using SS2.Items;
using Starstorm2.Components;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using Random = UnityEngine.Random;

namespace SS2.Components
{
    public class BirthrightMeteorStormController : MeteorStormController
    {
        private int aliveChimeras => Util.GetItemCountGlobal(SS2Content.Items.BirthrightChimeraHelper.itemIndex, true);
        private int chimeraCount;
        private CharacterSpawnCard golemSpawnCard;
        private CharacterSpawnCard wispSpawnCard; 

        private void OnEnable()
        {
            golemSpawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/LunarGolem/cscLunarGolem.asset").WaitForCompletion();
            wispSpawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/LunarWisp/cscLunarWisp.asset").WaitForCompletion();
         
            warningEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Meteor/MeteorStrikePredictionEffect.prefab").WaitForCompletion();
            impactEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Meteor/MeteorStrikeImpact.prefab").WaitForCompletion();
        }

        private void Start()
        {
            if (NetworkServer.active)
            {
                meteorList = new List<Meteor>();
                waveList = new List<MeteorWave>();
            }
            
            //add meteors at each birthright because i am evil ., 
            foreach (PurchaseInteraction birthrightPurchaseInteraction in PrimalBirthrightObjectiveToken.instanceList)
            {
                MeteorStormController.Meteor meteor = new MeteorStormController.Meteor();
                meteor.impactPosition = birthrightPurchaseInteraction.gameObject.transform.position;
                    
                Vector3 origin = meteor.impactPosition + Vector3.up * 6f;
                Vector3 onUnitSphere = Random.onUnitSphere;
                onUnitSphere.y = -1f;
                if (Physics.Raycast(origin, onUnitSphere, out var hitInfo, 12f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    meteor.impactPosition = hitInfo.point;
                }
                else if (Physics.Raycast(meteor.impactPosition, Vector3.down, out hitInfo, float.PositiveInfinity, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    meteor.impactPosition = hitInfo.point;
                }
                    
                gameObject.GetComponent<BirthrightMeteorStormController>().DetonateMeteor(meteor);
            }
        }

        //copied version of MeteorWave.GetNextMeteor but to spawn several enemies instead of just 1 ,.,
        private Meteor GetNextMeteorPrimal(MeteorWave wave)
        {
            if (wave.currentStep >= wave.targets.Length * chimeraCount)
            {
                return null;
            }
            
            CharacterBody characterBody = wave.targets[wave.currentStep % wave.targets.Length];
            Meteor meteor = new Meteor();
            if (characterBody && Random.value < wave.hitChance)
            {
                meteor.impactPosition = characterBody.corePosition;
                Vector3 origin = meteor.impactPosition + Vector3.up * 6f;
                Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
                onUnitSphere.y = -1f;
                if (Physics.Raycast(origin, onUnitSphere, out var hitInfo, 12f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    meteor.impactPosition = hitInfo.point;
                }
                else if (Physics.Raycast(meteor.impactPosition, Vector3.down, out hitInfo, float.PositiveInfinity, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    meteor.impactPosition = hitInfo.point;
                }
            }
            else if (characterBody)
            {
                int randomVariance = 60;
                Vector3 nodeFindPos = new Vector3(characterBody.corePosition.x + Random.Range(-randomVariance, randomVariance), characterBody.corePosition.y + Random.Range(-randomVariance, randomVariance), characterBody.corePosition.z + Random.Range(-randomVariance, randomVariance));
                SceneInfo.instance.groundNodes.GetNodePosition(SceneInfo.instance.groundNodes.FindClosestNode(nodeFindPos, HullClassification.Golem), out meteor.impactPosition);
            }
            else
            {
                meteor.valid = false;
            }
            
            meteor.startTime = Run.instance.time;
            wave.currentStep++;
            return meteor;
        }
        
        //copied version of MeteorStormController.FixedUpdate with minor edits ,,.
        private void FixedUpdate()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            
            waveTimer -= Time.fixedDeltaTime;
            if (waveTimer <= 0f && waveList.Count == 0)
            {
                waveTimer = UnityEngine.Random.Range(PrimalBirthright.chimeraWaitTime - PrimalBirthright.chimeraWaitTimeVariance, PrimalBirthright.chimeraWaitTime + PrimalBirthright.chimeraWaitTimeVariance);
                chimeraCount = Random.Range(1, 4);
                if (PrimalBirthrightObjectiveToken.instanceList.Count > 0 && aliveChimeras < 15)
                {
                    waveTimer *= 1.15f; // players probably taking a bit to clear them lets be slightly nice ,.., 
                }
                else if (PrimalBirthrightObjectiveToken.instanceList.Count == 0)
                {
                    waveTimer *= 1.4f; // take longer between waves if the player got all birthrights ,.,. 
                }
                
                SS2Log.Debug("adding new wave ,.,.");
                
                //add meteor wave with array of alive players .,, .
                waveList.Add(new MeteorWave((from pcmc in PlayerCharacterMasterController.instances where pcmc.body select pcmc.body).ToArray(), base.transform.position));
            }

            for (int i = waveList.Count - 1; i >= 0; i--)
            {
                MeteorWave meteorWave = waveList[i];
                meteorWave.timer -= Time.fixedDeltaTime;
                if (meteorWave.timer <= 0f)
                {
                    meteorWave.timer = UnityEngine.Random.Range(0.25f, 0.75f);
                    Meteor nextMeteor = GetNextMeteorPrimal(meteorWave);
                    //SS2Log.Debug($"currentstep {meteorWave.currentStep}");
                    //SS2Log.Debug($"targets {meteorWave.targets.Length}");
                    
                    if (nextMeteor == null)
                    {
                        waveList.RemoveAt(i);
                    }
                    else if (nextMeteor.valid)
                    {
                        meteorList.Add(nextMeteor);
                        EffectManager.SpawnEffect(warningEffectPrefab, new EffectData
                        {
                            origin = nextMeteor.impactPosition,
                            scale = blastRadius
                        }, transmit: true);
                    }
                }
            }
            
            float num2 = Run.instance.time - impactDelay;
            float num3 = num2 - travelEffectDuration;
            for (int num4 = meteorList.Count - 1; num4 >= 0; num4--)
            {
                Meteor meteor = meteorList[num4];
                if (meteor.startTime < num3 && !meteor.didTravelEffect)
                {
                    DoMeteorEffect(meteor);
                }
                if (meteor.startTime < num2)
                {
                    meteorList.RemoveAt(num4);
                    DetonateMeteor(meteor);
                }
            }
        }
        
        //copied version of MeteorStormController.DetonateMeteor but spawns enemies alongside the meteor ,. .
        private void DetonateMeteor(Meteor meteor)
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
                        position = meteor.impactPosition
                    },
                    RoR2Application.rng
                );
                spawnRequest.teamIndexOverride = TeamIndex.Monster;
                spawnRequest.onSpawnedServer += result =>
                {
                    CharacterMaster golemMaster = result.spawnedInstance?.GetComponent<CharacterMaster>();
                    if (!golemMaster) return;
                    
                    golemMaster.inventory.GiveItemPermanent(SS2Content.Items.BirthrightChimeraHelper.itemIndex);
                };
                
                DirectorCore.instance.TrySpawnObject(spawnRequest);
            }
     
            EffectData effectData = new EffectData
            {
                origin = meteor.impactPosition
            };
            EffectManager.SpawnEffect(impactEffectPrefab, effectData, transmit: true);
            
            //lunar golem base damage + level damage .,., not magic numbers i promise !!
            uint damage = 35 + 7 * TeamManager.instance.GetTeamLevel(TeamIndex.Monster);
            SS2Log.Debug($"meteor damage {damage}");
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
                position = meteor.impactPosition,
                procChainMask = default,
                procCoefficient = 1f,
                teamIndex = TeamIndex.Monster,
                radius = blastRadius,
                damageType = blastDamageType
            };
            blastAttack.Fire();
        }
    }
}