using R2API;
using RoR2;
using RoR2.Items;
using RoR2.CharacterAI;
using EntityStates.AI.Walker;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class ChirrFriendHelper : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("ChirrFriendHelper", SS2Bundle.Chirr);

        public static GameObject jitterEffectPrefab = SS2Assets.LoadAsset<GameObject>("FriendJitterEffect", SS2Bundle.Chirr);

        public static float attackSpeedToCooldownConversion = 1f;
        public static float aimSpeedCoefficient = 3f;

        public static float maxEnemyTargetDistance = 80f;
        public static float leashDistance = 160f;
        public static float leashTeleportOffset = 10f;

        public static float timeUntilMaxCurse = 100f;

        // Converts attackspeed to CDR
        // teleports back to chirr if too far away
        public sealed class BodyBehavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.ChirrFriendHelper;
            private float leashTimer = 2f;
            private float curseTimer;
            private float desiredHealthFraction = 1f;

            private GameObject jitterBonesEffect;
            private void Start()
            {
                if(NetworkServer.active)
                    base.body.AddBuff(SS2Content.Buffs.BuffChirrFriend);

                CharacterModel model = body.modelLocator.modelTransform.GetComponent<CharacterModel>();
                SkinnedMeshRenderer renderer = model.mainSkinnedMeshRenderer;
                this.jitterBonesEffect = UnityEngine.Object.Instantiate<GameObject>(jitterEffectPrefab, model.transform);
                if (renderer)
                {
                    JitterBones[] components = this.jitterBonesEffect.GetComponents<JitterBones>();
                    for (int i = 0; i < components.Length; i++)
                    {
                        components[i].skinnedMeshRenderer = renderer;
                    }
                }
            }
            private void OnDestroy()
            {
                if (this.jitterBonesEffect) Destroy(this.jitterBonesEffect);
                if (NetworkServer.active && base.body.enabled) base.body.RemoveBuff(SS2Content.Buffs.BuffChirrFriend);              
            }
            private void FixedUpdate()
            {
                //#imwither

                // doesn t fucking work
                /// im so fucking stupid
                //if(NetworkServer.active)
                //{
                //    float percentDecayPerSecond = 100f / timeUntilMaxCurse;
                //    desiredHealthFraction = Mathf.Max(desiredHealthFraction - percentDecayPerSecond/100f * Time.fixedDeltaTime, .01f);
                //    this.curseTimer -= Time.fixedDeltaTime;
                //    if (this.curseTimer <= 0)
                //    {
                //        this.curseTimer += 1 / percentDecayPerSecond; // increment decay 1% every time
                //        float currentCurse = base.body.cursePenalty;
                //        float desiredCurse = 1 / desiredHealthFraction;
                //        int curseToAdd = Mathf.FloorToInt(desiredCurse - currentCurse);

                //        for (int i = 0; i < curseToAdd; i++)
                //        {
                //            this.body.AddBuff(RoR2Content.Buffs.PermanentCurse);
                //        }
                //    }
                //}                

                // get owner
                CharacterMaster master = base.body.master;
                CharacterMaster ownerMaster = master ? master.minionOwnership.ownerMaster : null;
                GameObject ownerBodyObject = ownerMaster ? ownerMaster.GetBodyObject() : null;
                if (!ownerBodyObject) return;

                // if we are too far away from chirr, "respawn" next to her
                Vector3 ownerPosition = ownerBodyObject.transform.position;
                float distanceBetween = (ownerPosition - base.transform.position).magnitude;
                if(distanceBetween >= leashDistance)
                {
                    leashTimer -= Time.fixedDeltaTime;
                    if(leashTimer <= 0)
                    {
                        leashTimer += 2f;
                        Vector3 offset = UnityEngine.Random.insideUnitCircle * leashTeleportOffset;
                        Vector3 safePosition = TeleportHelper.FindSafeTeleportDestination(ownerPosition + offset, base.body, RoR2Application.rng) ?? ownerPosition + Vector3.up * 10;
                        TeleportHelper.TeleportBody(base.body, safePosition);
                        EntityStateMachine body = EntityStateMachine.FindByCustomName(base.gameObject, "Body");
                        if(body)
                        {
                            body.SetNextState(EntityStateCatalog.InstantiateState(body.initialStateType)); // doesnt work every time? idk why
                        }
                    }
                    
                }
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                // 1 - 1/(1+cdr)     
                float cooldownReduction = Mathf.Max((body.attackSpeed - 1) * attackSpeedToCooldownConversion, 0);
                args.cooldownMultAdd *= Util.ConvertAmplificationPercentageIntoReductionPercentage(cooldownReduction * 100f) / 100f;               
            }                                       
        }

        // makes AI machine use FollowLeader to follow chirr around
        // makes AI more competent in general hopefully
        public sealed class MasterBehavior : BaseItemMasterBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.ChirrFriendHelper;

            private GameObject ownerBodyObject;
            private EntityStateMachine stateMachine;
            private BaseAI ai;
            bool wasFullVision;

            private void OnEnable()
            {
                if (master.aiComponents.Length > 0)
                {
                    this.ai = master.aiComponents[0];
                    ai.aimVectorMaxSpeed *= aimSpeedCoefficient;
                    this.stateMachine = ai.stateMachine;
                    this.stateMachine.nextStateModifier += ModifyNextState;                   
                    ai.scanState = new EntityStates.SerializableEntityStateType(typeof(FollowLeader));
                    wasFullVision = ai.fullVision;
                    ai.fullVision = true;
                    ai.currentEnemy.Reset();
                    ai.customTarget.Reset();                    
                    ai.buddy.Reset();
                    ai.ForceAcquireNearestEnemyIfNoCurrentEnemy();
                }
            }

            private void FixedUpdate()
            {
                //get chirr's and our own body objects
                CharacterMaster ownerMaster = master.minionOwnership.ownerMaster;
                this.ownerBodyObject = ownerMaster ? ownerMaster.GetBodyObject() : null;
                GameObject bodyObject = master.GetBodyObject();

                if (!ai || !bodyObject) return;
            
                this.ResetTargetIfDead();

                // if target is too far away, start following owner again
                Vector3 targetPosition = bodyObject.transform.position;
                ai.currentEnemy.GetBullseyePosition(out targetPosition);
                float distanceBetween = (targetPosition - bodyObject.transform.position).magnitude;
                if(distanceBetween > maxEnemyTargetDistance && this.stateMachine.state is Combat)
                {
                    this.stateMachine.SetNextState(new FollowLeader { leader = this.ownerBodyObject });
                }

            }
            private void ModifyNextState(EntityStateMachine machine, ref EntityStates.EntityState nextState)
            {
                if (nextState is LookBusy || nextState is Wander)
                {
                    nextState = new FollowLeader { leader = this.ownerBodyObject };
                }
            }
            private void ResetTargetIfDead()
            {
                BaseAI.Target target = ai.currentEnemy;
                if (!target.gameObject || (target.healthComponent && !target.healthComponent.alive))
                {
                    ai.currentEnemy.Reset();
                    HurtBox hurtBox = ai.FindEnemyHurtBox(float.PositiveInfinity, true, true);
                    if (hurtBox && hurtBox.healthComponent)
                    {
                        ai.currentEnemy.gameObject = hurtBox.healthComponent.gameObject;
                        ai.currentEnemy.bestHurtBox = hurtBox;
                    }
                }
            }
            private void OnDisable()
            {
                if (master.aiComponents.Length > 0 && master.aiComponents[0].enabled)
                {
                    BaseAI ai = master.aiComponents[0];
                    ai.aimVectorMaxSpeed /= aimSpeedCoefficient;
                    ai.stateMachine.nextStateModifier -= ModifyNextState;
                    ai.scanState = new EntityStates.SerializableEntityStateType(typeof(Wander)); // this should be fine. all ai uses wander.
                    ai.fullVision = wasFullVision;
                    ai.currentEnemy.Reset();
                    ai.customTarget.Reset();
                    ai.buddy.Reset();
                    ai.enemyAttention = 0f;
                    ai.ForceAcquireNearestEnemyIfNoCurrentEnemy();
                }
            }
        }

        
    }
}
