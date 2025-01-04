using System;
using System.Collections.Generic;
using System.Linq;
using RoR2;
using UnityEngine;
using R2API;
namespace EntityStates.Cyborg2
{
    public class RisingStar : BaseSkillState
    {
        public static float baseScanDuration = 0.3f;
        public static float baseScanInterval = 0.075f;
        public static float scanDistance = 128f;
        
        public static float scanAngle = 30f;

        private static float damageCoefficient = 1.5f;
        public static float procCoefficient = 1f;
        public static float baseFireDuration = 0.5f;
        public static float force = 150f;
        public static float recoil = 1;
        public static float bloom = .4f;
        private static float bulletRadius = .1f;
        public static float bulletDistance = 150f;

        public static float blastRadius = 5f;

        private static float baseExitDuration = 0.3f;
        public static int maxShots = 3;

        public static float walkSpeedPenaltyCoefficient = 0.33f;

        public static GameObject explosionEffectPrefab;
        public static GameObject tracerPrefab;
        public static GameObject hitEffectPrefab;
        public static GameObject muzzleFlashPrefab;
        public static string fireSoundString = "Play_lunar_golem_attack1_launch"; //"Play_MULT_m1_snipe_shoot"

        private HurtBox[] targets;
        private Dictionary<HurtBox, IndicatorInfo> targetIndicators;

        private int indicatorsAdded;
        private float scanInterval;
        private float scanStopwatch;
        private float scanDuration;
        private int shotsFired;
        private float fireInterval;
        private float fireStopwatch;
        private float exitTimer;

        private string muzzleString = "CannonL";
        private Animator animator;
        private Vector3 aimDirection;

        private AimAnimator aimAnimator;
        private AimAnimator.DirectionOverrideRequest animatorDirectionOverrideRequest;
        public override void OnEnter()
        {
            base.OnEnter();
            this.scanDuration = baseScanDuration / this.attackSpeedStat;
            this.scanInterval = baseScanInterval / this.attackSpeedStat;

            this.targets = GetTargets();
            base.StartAimMode();
            animator = base.GetModelAnimator();

            this.targetIndicators = new Dictionary<HurtBox, IndicatorInfo>();

            this.fireInterval = baseFireDuration / maxShots / this.attackSpeedStat;
            exitTimer = baseExitDuration / attackSpeedStat;
            base.characterMotor.walkSpeedPenaltyCoefficient = walkSpeedPenaltyCoefficient;

            this.aimDirection = base.GetAimRay().direction;
            aimAnimator = base.GetAimAnimator();
            if (aimAnimator)
            {
                animatorDirectionOverrideRequest = aimAnimator.RequestDirectionOverride(new Func<Vector3>(this.GetAimDirection));
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(base.fixedAge >= this.scanDuration && this.shotsFired < maxShots)
            {
                this.fireStopwatch -= Time.fixedDeltaTime;
                if (this.fireStopwatch <= 0)
                {
                    this.fireStopwatch += this.fireInterval;
                    if(this.targets.Length > 0)
                    {
                        int count = this.targets.Length;
                        int index = this.shotsFired % count;
                        this.FireAt(this.targets[index]);
                    }
                    else
                        this.FireAt(null);
                }
            }
            else if (base.isAuthority)
            {
                if(this.targets.Length > 0)
                {
                    this.CleanTargetsList();

                    this.scanStopwatch -= Time.fixedDeltaTime;
                    if (this.scanStopwatch <= 0 && this.indicatorsAdded < maxShots)
                    {
                        this.scanStopwatch += this.scanInterval;
                        this.AddIndicator(this.targets[this.indicatorsAdded]);
                        this.indicatorsAdded++;
                    }
                }                                              
                
            }
            if(this.shotsFired >= maxShots)
            {
                exitTimer -= Time.fixedDeltaTime;
                if(exitTimer <= 0)
                {
                    this.outer.SetNextStateToMain();
                    return;
                }         
            }
        }

        private Vector3 GetAimDirection()
        {           
            return this.aimDirection;
        }

        public override void Update()
        {
            base.Update();
            if(this.aimAnimator)
            {
                aimAnimator.AimImmediate();
            }
            if(this.animator) // bleh why doesnt vanilla do this
            {
                this.animator.SetFloat(AnimationParameters.walkSpeed, base.characterMotor.walkSpeed);
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (this.animatorDirectionOverrideRequest != null)
            {
                this.animatorDirectionOverrideRequest.Dispose();
            }
            base.characterMotor.walkSpeedPenaltyCoefficient = 1;

            if (this.targetIndicators != null)
            {
                foreach (KeyValuePair<HurtBox, IndicatorInfo> keyValuePair in this.targetIndicators)
                {
                    keyValuePair.Value.indicator.active = false;
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public void CleanTargetsList()
        {
            for (int i = this.targets.Length - 1; i >= 0; i--)
            {
                HurtBox hurtBox = this.targets[i];
                if (!hurtBox.healthComponent || !hurtBox.healthComponent.alive)
                {
                    IndicatorInfo indicatorInfo;
                    if (this.targetIndicators.TryGetValue(hurtBox, out indicatorInfo))
                    {
                        indicatorInfo.indicator.active = false;
                        this.targetIndicators.Remove(hurtBox);
                    }
                }
            }
        }

        private static float wawaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa = 1.6f;
        private void FireAt(HurtBox hurtBox)
        {
            //vfx
            //sound
            //anim
            this.shotsFired++;

            base.PlayAnimation("Gesture, Override", "Primary2");
            EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, base.gameObject, muzzleString, false);
            base.characterBody.AddSpreadBloom(bloom);
            base.AddRecoil(-1f * recoil, -1.5f * recoil, -1f * recoil, 1f * recoil);
            Util.PlayAttackSpeedSound(fireSoundString, base.gameObject, wawaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa);         

            Ray aimRay = base.GetAimRay();
            Vector3 direction = hurtBox != null ? (hurtBox.transform.position - aimRay.origin).normalized : aimRay.direction;
            this.aimDirection = direction;
            if (base.isAuthority)
            {

                bool hit = Physics.SphereCast(aimRay.origin, bulletRadius, direction, out RaycastHit hitInfo, bulletDistance, LayerIndex.CommonMasks.bullet.value, QueryTriggerInteraction.UseGlobal);
                Vector3 position = hit ? hitInfo.point : aimRay.GetPoint(bulletDistance);
                if (hurtBox)
                    this.RemoveIndicator(hurtBox);
                DamageTypeCombo damageType = DamageType.Generic;
                damageType.damageSource = DamageSource.Secondary;
                BulletAttack bulletAttack = new BulletAttack
                {
                    aimVector = direction,
                    origin = aimRay.origin,
                    owner = base.gameObject,
                    damage = damageStat * damageCoefficient,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = damageType,
                    falloffModel = BulletAttack.FalloffModel.None,
                    force = force,
                    HitEffectNormal = false,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = procCoefficient,
                    maxDistance = bulletDistance,
                    radius = bulletRadius,
                    isCrit = base.RollCrit(),
                    muzzleName = muzzleString,
                    minSpread = 0,
                    maxSpread = 0,
                    hitEffectPrefab = hitEffectPrefab,
                    smartCollision = true,
                    tracerEffectPrefab = tracerPrefab,
                    stopperMask = LayerIndex.world.mask,
                };
                bulletAttack.AddModdedDamageType(SS2.Survivors.Cyborg2.applyCyborgPrime);
                bulletAttack.Fire();
            }            
        }

        private void AddIndicator(HurtBox hurtBox)
        {
            Util.PlaySound(EntityStates.Engi.EngiMissilePainter.Paint.lockOnSoundString, base.gameObject);

            IndicatorInfo indicatorInfo;
            if (!this.targetIndicators.TryGetValue(hurtBox, out indicatorInfo))
            {
                indicatorInfo = new IndicatorInfo
                {
                    refCount = 0,
                    indicator = new RisingStarIndicator(base.gameObject, Resources.Load<GameObject>("Prefabs/EngiMissileTrackingIndicator"))
                };
                indicatorInfo.indicator.targetTransform = hurtBox.transform;
                indicatorInfo.indicator.active = true;
            }
            indicatorInfo.refCount++;
            indicatorInfo.indicator.missileCount = indicatorInfo.refCount;
            this.targetIndicators[hurtBox] = indicatorInfo;
        }
        public void RemoveIndicator(HurtBox hurtBox)
        {
            IndicatorInfo indicatorInfo;
            if (this.targetIndicators.TryGetValue(hurtBox, out indicatorInfo))
            {
                indicatorInfo.refCount--;
                indicatorInfo.indicator.missileCount = indicatorInfo.refCount;
                this.targetIndicators[hurtBox] = indicatorInfo;
                if (indicatorInfo.refCount == 0)
                {
                    indicatorInfo.indicator.active = false;
                    this.targetIndicators.Remove(hurtBox);
                }
            }
        }

        private HurtBox[] GetTargets()
        {
            Ray aimRay = base.GetAimRay();

            TeamMask filter = TeamMask.allButNeutral;
            filter.RemoveTeam(this.teamComponent.teamIndex);
            BullseyeSearch search = new BullseyeSearch
            {
                teamMaskFilter = filter,
                filterByLoS = true,
                searchOrigin = aimRay.origin,
                searchDirection = aimRay.direction,
                sortMode = BullseyeSearch.SortMode.Angle,
                maxDistanceFilter = scanDistance,
                maxAngleFilter = scanAngle,
            };
            search.RefreshCandidates();
            search.FilterOutGameObject(base.gameObject);

            HurtBox[] results = search.GetResults().ToArray();
            if (results.Length == 0) return Array.Empty<HurtBox>();

            
            HurtBox[] hurtBoxes = new HurtBox[maxShots];
            for(int i = 0; i < hurtBoxes.Length; i++)
            {
                int index = i % results.Length; //add repeat targets if search had less than max targets
                hurtBoxes[i] = results[index];
            }

            return hurtBoxes;
        }




        private struct IndicatorInfo
        {
            public int refCount;
            public RisingStarIndicator indicator;
        }
        private class RisingStarIndicator : Indicator
        {
            public override void UpdateVisualizer()
            {
                base.UpdateVisualizer();
                Transform transform = base.visualizerTransform.Find("DotOrigin");
                for (int i = transform.childCount - 1; i >= this.missileCount; i--)
                {
                    EntityState.Destroy(transform.GetChild(i).gameObject);
                }
                for (int j = transform.childCount; j < this.missileCount; j++)
                {
                    UnityEngine.Object.Instantiate<GameObject>(base.visualizerPrefab.transform.Find("DotOrigin/DotTemplate").gameObject, transform);
                }
                if (transform.childCount > 0)
                {
                    float num = 360f / (float)transform.childCount;
                    float num2 = (float)(transform.childCount - 1) * 90f;
                    for (int k = 0; k < transform.childCount; k++)
                    {
                        Transform child = transform.GetChild(k);
                        child.gameObject.SetActive(true);
                        child.localRotation = Quaternion.Euler(0f, 0f, num2 + (float)k * num);
                    }
                }
            }


            public RisingStarIndicator(GameObject owner, GameObject visualizerPrefab) : base(owner, visualizerPrefab)
            {
            }


            public int missileCount;
        }
    }
}
