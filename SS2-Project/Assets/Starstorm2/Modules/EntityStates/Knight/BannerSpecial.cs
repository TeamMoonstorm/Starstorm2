using UnityEngine;
using RoR2;
using RoR2.Skills;
using UnityEngine.Networking;

namespace EntityStates.Knight
{
    public class BannerSpecial : BaseState
    {
        public static SkillDef buffedSkillRef;
        public static GameObject knightBannerWard;
        public static GameObject slowBuffWard;

        private GameObject bannerObject;
        private GameObject slowBuffWardInstance;

        public float rollSpeed = 1.2f;
        private Vector3 forwardDirection;
        public float hopVelocity = 25f;

        public float minimumY = 0.05f;
        public float airControl = 0.15f;
        public float aimVelocity = 4f;
        public float upwardVelocity = 7f;
        public float forwardVelocity = 3f;


        private float previousAirControl;

        private void FireImpact()
        {
            PlayAnimation("FullBody, Override", "SpecialLeapEnd", "Special.playbackRate", 1f);


            if (base.isAuthority)
            {
                new BlastAttack
                {
                    attacker = base.gameObject,
                    baseDamage = damageStat,
                    baseForce = 20f,
                    bonusForce = Vector3.up,
                    crit = false,
                    damageType = DamageType.Generic,
                    falloffModel = BlastAttack.FalloffModel.Linear,
                    procCoefficient = 0.1f,
                    radius = 5f,
                    position = base.characterBody.footPosition,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    impactEffect = EffectCatalog.FindEffectIndexFromPrefab(SS2.Survivors.Knight.KnightImpactEffect),
                    teamIndex = base.teamComponent.teamIndex,
                }.Fire();
            }

            if (NetworkServer.active)
            {
                Vector3 position = inputBank.aimOrigin - (inputBank.aimDirection);
                bannerObject = UnityEngine.Object.Instantiate(knightBannerWard, position, Quaternion.identity);

                bannerObject.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
                NetworkServer.Spawn(bannerObject);

                slowBuffWardInstance = UnityEngine.Object.Instantiate(slowBuffWard, position, Quaternion.identity);
                slowBuffWardInstance.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
                slowBuffWardInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(bannerObject);
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("FullBody, Override", "SpecialLeapStart", "Special.playbackRate", 1);

            base.OnEnter();
            previousAirControl = base.characterMotor.airControl;
            base.characterMotor.airControl = airControl;
            Vector3 direction = GetAimRay().direction;
            if (base.isAuthority)
            {
                base.characterBody.isSprinting = true;
                direction.y = Mathf.Max(direction.y, minimumY);
                Vector3 vector = direction.normalized * aimVelocity * moveSpeedStat;
                Vector3 vector2 = Vector3.up * upwardVelocity;
                Vector3 vector3 = new Vector3(direction.x, 0f, direction.z).normalized * forwardVelocity;
                base.characterMotor.Motor.ForceUnground();
                base.characterMotor.velocity = vector + vector2 + vector3;
            }
            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            GetModelTransform().GetComponent<AimAnimator>().enabled = true;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.isAuthority && base.characterMotor)
            {
                base.characterMotor.moveDirection = base.inputBank.moveVector;
                if (base.isGrounded)
                {
                    FireImpact();
                    outer.SetNextStateToMain();
                }
            }
        }

        public override void OnExit()
        {
            outer.SetNextStateToMain();
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }

}