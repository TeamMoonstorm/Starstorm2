using Moonstorm.Starstorm2;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Wayfarer
{
    class FireChains : BaseSkillState
    {
        public float baseDuration = 2.5f;
        public float damageCoefficient = 0.5f;
        public float force = 10.0f;
        public float radius = 15.0f;
        public GameObject explosionPrefab = Resources.Load<GameObject>("prefabs/effects/omnieffect/OmniExplosionVFX");

        private Animator animator;
        private float duration;
        private EffectData effectData;
        private BlastAttack attack;
        private ChildLocator locator;
        private bool hasAttackedL;
        private bool hasAttackedR;
        private bool hasAttacked;

        public static GameObject projectilePrefab;

        private GameObject projectileObject = Object.Instantiate(SS2Assets.LoadAsset<GameObject>("WayfarerBullet"));

        public override void OnEnter()
        {
            base.OnEnter();

            animator = GetModelAnimator();
            duration = baseDuration / attackSpeedStat;
            effectData = new EffectData();
            effectData.scale = radius;

            PlayCrossfade("FullBody, Override", "Melee", "Melee.playbackRate", duration, 0.2f);

            locator = GetModelTransform().GetComponent<ChildLocator>();
            
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!hasAttackedL && fixedAge >= duration * 0.32f) //we love hard coded timing!
            {
                hasAttackedL = true;
                DoAttack("LanternL");
            }

            if (!hasAttackedR && fixedAge >= duration * 0.7f)
            {
                hasAttackedR = true;
                DoAttack("LanternR");
            }

            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        private void DoAttack(string childName)
        {
            for (int i = 0; i < 8; i++)
            {
                Util.PlayAttackSpeedSound(EntityStates.GravekeeperBoss.FireHook.soundString, gameObject, attackSpeedStat);
                if (isAuthority)
                {
                    Vector3 forward = Util.ApplySpread(GetAimRay().direction, 2.5f, 2.5f, 2.5f, 2.5f, (Random.Range(-15f, 15f) / 2f), (Random.Range(-15f, 15f) / 2f));
                    ProjectileManager.instance.FireProjectile(projectileObject, locator.FindChild(childName).position, Util.QuaternionSafeLookRotation(forward), gameObject,
                        damageStat * damageCoefficient, force, Util.CheckRoll(critStat, characterBody.master));
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
