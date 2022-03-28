using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Wayfarer
{
    class FireChains : BaseSkillState
    {
        public static float baseDuration = 2.5f;
        public static float damageCoefficient = 4.0f;
        public static float force = 10.0f;
        public static float radius = 15.0f;
        public static GameObject explosionPrefab = Resources.Load<GameObject>("prefabs/effects/omnieffect/OmniExplosionVFX");

        private Animator animator;
        private float duration;
        private EffectData effectData;
        private BlastAttack attack;
        private ChildLocator locator;
        private bool hasAttackedL;
        private bool hasAttackedR;

        private GameObject chainPrefab;

        public override void OnEnter()
        {
            base.OnEnter();

            animator = GetModelAnimator();
            duration = baseDuration / attackSpeedStat;
            effectData = new EffectData();
            effectData.scale = radius;

            PlayCrossfade("FullBody, Override", "Melee", "Melee.playbackRate", duration, 0.2f);

            //attack = new BlastAttack();
            //attack.attacker = base.gameObject;
            //attack.inflictor = base.gameObject;
            //attack.baseDamage = damageStat * damageCoefficient;
            //attack.baseForce = force;
            //attack.radius = radius;
            //attack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);

            locator = GetModelTransform().GetComponent<ChildLocator>();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (animator)
            {
                if (!hasAttackedL && animator.GetFloat("MeleeL.active") > 0.5)
                {
                    hasAttackedL = true;
                    DoAttack("LanternL");
                }
                else if (!hasAttackedR && animator.GetFloat("MeleeR.active") > 0.5)
                {
                    hasAttackedR = true;
                    DoAttack("LanternR");
                }
            }

            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        private void DoAttack(string childName)
        {
            //Vector3 orig = locator.FindChild(childName).position;
            //effectData.origin = orig;
            //EffectManager.SpawnEffect(explosionPrefab, effectData, true);
            Util.PlayAttackSpeedSound(EntityStates.GravekeeperBoss.FireHook.soundString, gameObject, attackSpeedStat);
            if (isAuthority)
            {
                ProjectileManager.instance.FireProjectile(chainPrefab, locator.FindChild(childName).position, Util.QuaternionSafeLookRotation(GetAimRay().direction), gameObject,
                    damageStat * damageCoefficient, force, Util.CheckRoll(critStat, characterBody.master));
                //Debug.Log(orig);
                //attack.position = orig;
                //attack.Fire();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
