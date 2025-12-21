using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using RoR2.Projectile;

namespace EntityStates.AcidBug.Mine
{
    public class Arming : EntityState
    {
        private static float duration = 0.5f;
        private ProjectileStickOnImpact stickOnImpact;
        public override void OnEnter()
        {
            base.OnEnter();

            stickOnImpact = GetComponent<ProjectileStickOnImpact>();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (stickOnImpact && stickOnImpact.stuckBody != null)
            {
                outer.SetNextState(new PreDetonate());
            }
            else if (fixedAge >= duration)
            {
                outer.SetNextState(new WaitForTarget());
            }
        }
    }

    public class WaitForTarget : EntityState
    {
        private ProjectileSphereTargetFinder targetFinder;
        private ProjectileTargetComponent targetComponent;
        private static float triggerRadius = 7f;
        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                targetComponent = GetComponent<ProjectileTargetComponent>();
                targetFinder = GetComponent<ProjectileSphereTargetFinder>();
                targetFinder.enabled = true;
                targetFinder.lookRange = triggerRadius;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (NetworkServer.active)
            {
                if (targetComponent.target != null)
                {
                    outer.SetNextState(new PreDetonate());
                }
            }
        }
        public override void OnExit()
        {
            if (targetFinder)
            {
                targetFinder.enabled = false;
            }
            base.OnExit();
        }
    }

    public class PreDetonate : BaseState
    {
        private static float warningRadius = 9f;
        private static float duration = 0.75f;
        private static string enterSoundString = ""; 
        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Base", "Detonate", "Detonate.playbackRate", duration);
            Util.PlaySound(enterSoundString, gameObject);

            var warningTransform = FindModelChild("Warning");
            if (warningTransform)
            {
                warningTransform.localScale = Vector3.one * warningRadius;
                if (warningTransform.TryGetComponent(out ObjectScaleCurve objectScaleCurve))
                {
                    objectScaleCurve.timeMax = duration;
                }
                warningTransform.gameObject.SetActive(true);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (NetworkServer.active && fixedAge >= duration)
            {
                outer.SetNextState(new Detonate());
            }
        }
    }

    public class Detonate : EntityState
    {
        public static GameObject explosionEffectPrefab;

        private static float blastRadius = 9f;
        private static float damageCoefficient = 1f;
        private static float force = 300f;
        public override void OnEnter()
        {
            base.OnEnter();

            if (NetworkServer.active)
            {
                Explode();
            }
        }
        private void Explode()
        {
            ProjectileDamage projectileDamage = base.GetComponent<ProjectileDamage>();

            BlastAttack blastAttack = new BlastAttack();
            blastAttack.procChainMask = projectileController.procChainMask;
            blastAttack.procCoefficient = projectileController.procCoefficient;
            blastAttack.attacker = projectileController.owner;
            blastAttack.inflictor = gameObject;
            blastAttack.teamIndex = projectileController.teamFilter.teamIndex;
            blastAttack.procCoefficient = projectileController.procCoefficient;
            blastAttack.baseDamage = projectileDamage.damage * damageCoefficient;
            blastAttack.baseForce = force;
            blastAttack.falloffModel = BlastAttack.FalloffModel.None;
            blastAttack.crit = projectileDamage.crit;
            blastAttack.radius = blastRadius;
            blastAttack.position = transform.position;
            blastAttack.damageColorIndex = projectileDamage.damageColorIndex;
            blastAttack.damageType.damageSource = DamageSource.Primary;
            blastAttack.Fire();

            if (explosionEffectPrefab)
            {
                EffectManager.SpawnEffect(explosionEffectPrefab, new EffectData
                {
                    origin = base.transform.position,
                    rotation = base.transform.rotation,
                    scale = blastRadius
                }, true);
            }

            Destroy(base.gameObject);
        }

    }
}
