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

        public float initialSpeedCoefficient = 6f;
        public float finalSpeedCoefficient = 2.5f;


        public float rollSpeed = 1f;
        private Vector3 forwardDirection;
        public float hopVelocity = 10f;

        private void FireImpact()
        {
            PlayAnimation("FullBody, Override", "SpecialLeapEnd", "Special.playbackRate", 1 * 0.8f);


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

            forwardDirection = (inputBank.moveVector == Vector3.zero ? characterDirection.forward : inputBank.moveVector).normalized;

            if (characterMotor && characterDirection)
            {
                characterMotor.velocity = forwardDirection * rollSpeed;
                SmallHop(characterMotor, hopVelocity);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.isGrounded)
            {
                FireImpact();
                outer.SetNextStateToMain();
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