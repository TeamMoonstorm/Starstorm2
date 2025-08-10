using Mono.Cecil.Cil;
using MonoMod.Cil;
using SS2.Components;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using R2API;
using RoR2.ContentManagement;
using RoR2.Orbs;
using R2API.Utils;
using MSU;

namespace SS2.Survivors
{
    public sealed class NemExecutioner : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acNemExecutioner", SS2Bundle.NemExecutioner);

        public static DamageAPI.ModdedDamageType fearOnHit;
        public static DamageAPI.ModdedDamageType healNovaOnKill;
        public static DeployableSlot Ghoul;
        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
        public override void Initialize()
        {
            SetupDefaultBody(CharacterPrefab);

            fearOnHit = DamageAPI.ReserveDamageType(); // A lot of fear code is in Executioner2. i dont rly like this
            healNovaOnKill = DamageAPI.ReserveDamageType();

            Ghoul = DeployableAPI.RegisterDeployableSlot(GetMaxGhouls);

            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;

            healNovaEffectPrefab = SS2Assets.LoadAsset<GameObject>("HealNovaOnKillEffect", SS2Bundle.NemExecutioner);
            fearEffectPrefab = SS2Assets.LoadAsset<GameObject>("FearEffectRed", SS2Bundle.NemExecutioner);
            ionOrbEffectPrefab = SS2Assets.LoadAsset<GameObject>("NemExeIonOrbEffect", SS2Bundle.NemExecutioner);

            GameObject fearProjectile = SS2Assets.LoadAsset<GameObject>("FearProjectile", SS2Bundle.NemExecutioner);
            if(fearProjectile)
            {
                fearProjectile.GetComponent<RoR2.Projectile.ProjectileDamage>().damageType.AddModdedDamageType(fearOnHit);
            }
        }

        private static int GetMaxGhouls(CharacterMaster master, int deployableCountMultiplier)
        {
            return master.GetBody().skillLocator.secondary.maxStock;
        }

        #region Events
        public static float healNovaRadius = 32f;
        public static float healNovaPercentHeal = 0.2f;
        public static float healNovaDamageCoefficient = 0.1f;
        public static float healOrbSpeed = 60f;
        public static float healOrbMinDuration = 0.25f;
        public static GameObject healNovaEffectPrefab;
        // TODO: HEAL ORB EFFECT!!!!!!!!!!!!!!!!!
        private void OnCharacterDeathGlobal(DamageReport damageReport)
        {
            if (damageReport.damageInfo.HasModdedDamageType(healNovaOnKill))
            {
                Vector3 origin = damageReport.damageInfo.position;
                if(healNovaEffectPrefab)
                {
                    EffectData data = new EffectData
                    {
                        origin = origin,
                        scale = healNovaRadius,
                    };
                    EffectManager.SpawnEffect(healNovaEffectPrefab, data, true);
                }

                SphereSearch search = new SphereSearch();
                search.mask = LayerIndex.entityPrecise.mask;
                search.radius = healNovaRadius;
                search.origin = origin;
                search.queryTriggerInteraction = QueryTriggerInteraction.Ignore;
                TeamMask mask = TeamMask.none;
                mask.AddTeam(damageReport.attackerTeamIndex);
                search.RefreshCandidates();
                search.FilterCandidatesByHurtBoxTeam(mask);
                search.FilterCandidatesByDistinctHurtBoxEntities();

                List<HealthComponent> targets = new List<HealthComponent>();
                foreach (HurtBox hurtBox in search.GetHurtBoxes())
                {
                    if (hurtBox && hurtBox.healthComponent && !targets.Contains(hurtBox.healthComponent))
                    {
                        targets.Add(hurtBox.healthComponent);
                    }
                }
                foreach (HealthComponent healthComponent in targets)
                {
                    float amount = healNovaPercentHeal * healthComponent.fullHealth + healNovaDamageCoefficient * damageReport.damageDealt;
                    HealOrb healOrb = new HealOrb();
                    healOrb.origin = origin;
                    healOrb.target = healthComponent.body.mainHurtBox;
                    healOrb.healValue = amount;

                    float distance = Vector3.Distance(origin, healthComponent.body.mainHurtBox.transform.position);
                    float duration = Mathf.Max(distance / healOrbSpeed, healOrbMinDuration);
                    healOrb.overrideDuration = duration;

                    OrbManager.instance.AddOrb(healOrb);
                }
                
            }
        }

        public static float fearDuration = 3f;
        private void OnServerDamageDealt(DamageReport damageReport)
        {
            if(damageReport.damageInfo.HasModdedDamageType(fearOnHit))
            {
                CharacterBody body = damageReport.victimBody;
                if(body)
                {
                    body.AddTimedBuff(SS2Content.Buffs.BuffFearRed, fearDuration);
                    // TODO: unify fear code
                    if (body.master && body.master.aiComponents.Length > 0 && body.master.aiComponents[0])
                    {
                        body.master.aiComponents[0].stateMachine.SetNextState(new EntityStates.AI.Walker.Fear { fearTarget = damageReport.attacker });
                    }
                }
            }

            if (damageReport.victim.gameObject != damageReport.attacker && (damageReport.victimBody.bodyFlags & CharacterBody.BodyFlags.Masterless) == 0)
            {
                var assistTrackers = InstanceTracker.GetInstancesList<AssistTracker>();
                foreach (var killCpt in assistTrackers)
                {
                    if (killCpt.attacker == damageReport.attacker)
                    {
                        killCpt.SetTimeAlive(0);
                        return;
                    }
                }
                //Else add a kill component
                var killComponent = damageReport.victim.gameObject.AddComponent<AssistTracker>();
                killComponent.attacker = damageReport.attacker;
            }
        }

        public static GameObject ionOrbEffectPrefab;
        public static float ionOrbSpeed = 50f;
        public static float ionOrbMaxDuration = 3f;
        public class AssistTracker : MonoBehaviour, IOnKilledServerReceiver
        {
            public GameObject attacker;

            public float duration;
            private float timeAlive;
            public void OnKilledServer(DamageReport damageReport)
            {
                int orbCount = Executioner2.GetIonCountFromBody(damageReport.victimBody);

                for (int i = 0; i < orbCount; i++)
                {
                    NemExecutionerIonOrb ionOrb = new NemExecutionerIonOrb();
                    ionOrb.origin = transform.position;
                    ionOrb.target = Util.FindBodyMainHurtBox(attacker);
                    OrbManager.instance.AddOrb(ionOrb);
                }
            }

            public void SetTimeAlive(float newTime)
            {
                timeAlive = newTime;
                bool shouldEnable = timeAlive < duration;
                enabled = shouldEnable;
            }
            private void FixedUpdate()
            {
                timeAlive += Time.fixedDeltaTime;
                if (timeAlive >= duration)
                {
                    enabled = false;
                }
            }

            private void OnEnable()
            {
                InstanceTracker.Add(this);
            }

            private void OnDisable()
            {
                InstanceTracker.Remove(this);
            }
        }
        public class NemExecutionerIonOrb : Orb
        {
            public override void Begin()
            {
                duration = Mathf.Min(distanceToTarget / ionOrbSpeed, ionOrbMaxDuration);

                if (target && ionOrbEffectPrefab)
                {
                    EffectData effectData = new EffectData
                    {
                        origin = origin,
                        genericFloat = duration
                    };
                    effectData.SetHurtBoxReference(target);
                    EffectManager.SpawnEffect(ionOrbEffectPrefab, effectData, true);
                }
            }

            public override void OnArrival()
            {
                if (target && target.healthComponent)
                {
                    if (target.healthComponent.body.skillLocator.secondary)
                    {
                        FriendManager.instance.RpcAddStock(target.healthComponent.gameObject, (int)SkillSlot.Secondary);
                    }
                }
            }
        }

        public static GameObject fearEffectPrefab;
        public sealed class FearBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffFearRed;

            private GameObject effectInstance;
            private static string activationSoundString = "Play_voidman_R_pop";
            private Collider bodyCollider;
            private int previousBuffCount;
            private void OnEnable()
            {
                previousBuffCount = buffCount;
                OnStackGained();
                if (!bodyCollider)
                    bodyCollider = base.GetComponent<Collider>();
            }
            private void OnDisable()
            {
                Destroy(effectInstance);
            }

            private void FixedUpdate()
            {
                if (buffCount > previousBuffCount)
                {
                    OnStackGained();
                }
                if (!base.characterBody.healthComponent.alive)
                    Destroy(this.effectInstance);
                previousBuffCount = buffCount;
            }

            private void OnStackGained()
            {
                if (effectInstance) Destroy(effectInstance);
                effectInstance = GameObject.Instantiate(fearEffectPrefab, characterBody.coreTransform.position, Quaternion.identity);
                Util.PlaySound(activationSoundString, gameObject);
            }

            private void Update()
            {
                if (effectInstance)
                {
                    Vector3 a = base.transform.position;
                    if (this.bodyCollider)
                    {
                        a = this.bodyCollider.bounds.center + new Vector3(0f, this.bodyCollider.bounds.extents.y, 0f);
                    }
                    effectInstance.transform.position = a;
                }
            }
        }
        #endregion
    }
}