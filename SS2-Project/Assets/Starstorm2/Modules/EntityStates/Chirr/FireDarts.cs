using Moonstorm;
using RoR2;
using RoR2.Projectile;
using System;
using System.Linq;
using UnityEngine;

namespace EntityStates.Chirr
{
    public class FireDarts : BaseSkillState
    {
        [TokenModifier("SS2_CHIRR_DARTS_DESCRIPTION", StatTypes.Percentage, 0)]
        public static float damageCoefficient = 0.9f;
        public static float procCoefficient = 0.3333f;
        public static float baseDuration = 0.7f;
        public static float baseBurstDuration = 0.15f;
        public static float baseEarlyExitTime = 0.1f;
        public static float recoil = 1f;
        public static float randomPos;
        public static bool targetSnapping = true;
        public static bool snapAlliesOnly = false;
        public static float snapRadius = 20f;
        public static float snapRange = 350f;
        public static GameObject dartProjectile;
        public static string muzzleString;

        public GameObject target;

        private bool firstShot = false;
        private bool secondShot = false;
        private bool thirdShot = false;
        private float duration;
        private float burstDuration;
        private float earlyExitTime;
        private BullseyeSearch bullseye = new BullseyeSearch();
        private Animator animator;
        private Transform muzzleTransform;

        public override void OnEnter()
        {
            // Setup
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            burstDuration = baseBurstDuration / attackSpeedStat;
            earlyExitTime = baseEarlyExitTime / attackSpeedStat;
            characterBody.SetAimTimer(2f);
            animator = GetModelAnimator();
            muzzleTransform = FindModelChild(muzzleString);

            InitialTarget();

            // Play attack animations
            PlayAnimation("Gesture, Additive", "Primary", "Primary.playbackRate", burstDuration * 3);
            PlayAnimation("Gesture, Additive", "Primary", "Primary.playbackRate", burstDuration * 3);
        }

        public void InitialTarget()
        {
            // If no target, do a similar technique to bullet attacks to get the target you're aiming at (hopefully).
            if (target == null)
            {
                Ray aimRay = GetAimRay();
                LayerMask mask = BulletAttack.defaultHitMask;
                RaycastHit[] hits = Physics.SphereCastAll(aimRay, 1f, snapRange, mask, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (DoesHitMatch(hits[i]))
                    {
                        target = GetTarget(hits[i]);
                        break;
                    }
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            //Fire each shot, 1 by 1.
            if ((fixedAge >= burstDuration) && !firstShot)
            {
                FireTrackShot();
                firstShot = true;
            }
            if ((fixedAge >= burstDuration * 2) && !secondShot)
            {
                FireTrackShot();
                secondShot = true;
            }
            if ((fixedAge >= burstDuration * 3) && !thirdShot)
            {
                FireTrackShot();
                thirdShot = true;
            }

            // If holding down button when skill end, send 'target' to the next cast of this skill.
            if (fixedAge >= (duration - earlyExitTime) && isAuthority && inputBank.skill1.down)
            {
                RepeatState();
            }

            // Otherwise just do normal stuff
            if (fixedAge >= duration && isAuthority && !inputBank.skill1.down)
            {
                outer.SetNextStateToMain();
            }
        }

        // Fire a dart that tracks a bit.
        private void FireTrackShot()
        {
            characterBody.AddSpreadBloom(0.1f);
            Ray aimRay = GetAimRay();

            if (isAuthority)
            {
                if (dartProjectile)
                {
                    Vector3 posRandomised = (muzzleTransform ? muzzleTransform.position : aimRay.origin) + MakeRandomVector(randomPos);
                    FireProjectileInfo fireProjectile = new FireProjectileInfo
                    {
                        projectilePrefab = dartProjectile,
                        position = posRandomised,
                        rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                        owner = base.gameObject,
                        damage = damageStat * damageCoefficient,
                        force = 0f,
                        crit = Util.CheckRoll(critStat, characterBody.master),
                        damageColorIndex = DamageColorIndex.Default,
                        target = target,
                        speedOverride = -1f
                    };
                    ProjectileManager.instance.FireProjectile(fireProjectile);
                }
            }
        }

        private Vector3 MakeRandomVector(float range)
        {
            float rx = UnityEngine.Random.Range(-range, range);
            float ry = UnityEngine.Random.Range(-range, range);
            float rz = UnityEngine.Random.Range(-range, range);
            return new Vector3(rx, ry, rz);
        }

        // Get the hit collider's base gameobject.
        private GameObject GetTarget(RaycastHit inHit)
        {
            if (inHit.collider)
            {
                var hb = inHit.collider.GetComponent<HurtBox>();
                if (hb)
                {
                    return hb.healthComponent.body.gameObject;
                }
            }
            return null;
        }

        // Check if the raycast is hitting the type of target we want (enemy or ally)
        private bool DoesHitMatch(RaycastHit inHit)
        {
            if (inHit.collider)
            {
                var hurtBox = inHit.collider.GetComponent<HurtBox>();
                if (hurtBox)
                {
                    var health = hurtBox.healthComponent;
                    if (health)
                    {
                        var body = health.body;
                        if (body)
                        {
                            return ((body.teamComponent.teamIndex != TeamIndex.None) && (body != healthComponent.body) && (snapAlliesOnly ? body.teamComponent.teamIndex == teamComponent.teamIndex : true));
                        }
                    }
                }
            }
            return false;
        }

        // Sets the next state to FireDarts with target info (if that info is still valid), so button can be held and only need to do rough aiming.
        private void RepeatState()
        {
            CheckForTarget(snapRadius);
            GameObject tempTarget = target ? target : null;

            var obj = (FireDarts)Activator.CreateInstance(this.activatorSkillSlot.activationState.stateType);
            obj.target = tempTarget ? tempTarget : null;
            obj.activatorSkillSlot = this.activatorSkillSlot;
            this.outer.SetNextState(obj);
        }

        // Checks if the target is within a radius of the crosshair. If not, target is lost.
        private void CheckForTarget(float radius)
        {
            if (!target) return;

            Ray aimRay = GetAimRay();
            bullseye.teamMaskFilter = TeamMask.all;
            bullseye.filterByLoS = false;
            bullseye.sortMode = BullseyeSearch.SortMode.Angle;
            bullseye.maxDistanceFilter = snapRange;
            bullseye.maxAngleFilter = radius;
            bullseye.searchOrigin = aimRay.origin;
            bullseye.searchDirection = aimRay.direction;
            bullseye.RefreshCandidates();

            var tempTarget = bullseye.GetResults().SingleOrDefault(tt => tt.healthComponent.body.gameObject == target.gameObject);

            if (tempTarget == null) target = null;
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
