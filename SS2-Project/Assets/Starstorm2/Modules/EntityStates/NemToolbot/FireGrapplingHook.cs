using RoR2;
using SS2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.NemToolbot
{
    /// <summary>
    /// Ball form secondary. Fires a grappling hook projectile that pulls NemToolbot
    /// toward the impact point, following Loader's FireHook pattern. The hook projectile
    /// handles the actual pull physics; this state manages animation and lifetime.
    /// Hold the skill button to grapple; Loader's projectile retracts on release.
    /// </summary>
    public class FireGrapplingHook : BaseSkillState
    {
        public static GameObject projectilePrefab;

        public static float damageCoefficient = 1f;
        public static string fireSoundString = "";
        public static GameObject muzzleflashEffectPrefab;
        public static string muzzleString = "Muzzle";
        public static float maxDuration = 8f;

        /// <summary>
        /// Reference to the live hook projectile. Set by the projectile via SetHookReference().
        /// When this becomes null (projectile destroyed), the state exits.
        /// </summary>
        public GameObject hookInstance;

        protected ProjectileStickOnImpact hookStickOnImpact;
        private bool isStuck;
        private bool hadHookInstance;

        // private static readonly int FireHookIntroHash = Animator.StringToHash("FireHookIntro");
        // private static readonly int FireHookLoopHash = Animator.StringToHash("FireHookLoop");
        // private static readonly int FireHookExitHash = Animator.StringToHash("FireHookExit");

        public override void OnEnter()
        {
            base.OnEnter();

            if (isAuthority)
            {
                Ray ray = GetAimRay();
                if (projectilePrefab != null)
                {
                    TrajectoryAimAssist.ApplyTrajectoryAimAssist(ref ray, projectilePrefab, gameObject);

                    FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                    fireProjectileInfo.position = ray.origin;
                    fireProjectileInfo.rotation = Quaternion.LookRotation(ray.direction);
                    fireProjectileInfo.crit = characterBody.RollCrit();
                    fireProjectileInfo.damage = damageStat * damageCoefficient;
                    fireProjectileInfo.force = 0f;
                    fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
                    fireProjectileInfo.damageTypeOverride = DamageTypeCombo.GenericSecondary;
                    fireProjectileInfo.procChainMask = default(ProcChainMask);
                    fireProjectileInfo.projectilePrefab = projectilePrefab;
                    fireProjectileInfo.owner = gameObject;

                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
                else
                {
                    SS2Log.Error("NemToolbot FireGrapplingHook: projectilePrefab is null.");
                    outer.SetNextStateToMain();
                    return;
                }
            }

            if (muzzleflashEffectPrefab != null)
            {
                EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, gameObject, muzzleString, transmit: false);
            }
            // Util.PlaySound(fireSoundString, gameObject);

            // PlayAnimation("Grapple", FireHookIntroHash);
        }

        /// <summary>
        /// Called by the hook projectile to register itself with this state.
        /// The projectile finds its owner's FireGrapplingHook state and hands back a reference.
        /// </summary>
        public void SetHookReference(GameObject hook)
        {
            hookInstance = hook;
            if (hook.TryGetComponent(out ProjectileStickOnImpact stickComponent))
            {
                hookStickOnImpact = stickComponent;
            }
            else
            {
                SS2Log.Error("NemToolbot FireGrapplingHook: Hook projectile missing ProjectileStickOnImpact on " + hook.name);
            }
            hadHookInstance = true;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            // Transition animation when hook sticks
            if (hookStickOnImpact != null)
            {
                if (hookStickOnImpact.stuck && !isStuck)
                {
                    // PlayAnimation("Grapple", FireHookLoopHash);
                }
                isStuck = hookStickOnImpact.stuck;
            }

            if (!isAuthority)
                return;

            if (fixedAge >= maxDuration)
            {
                outer.SetNextStateToMain();
                return;
            }

            // Exit when hook projectile existed and is now destroyed
            if (!hookInstance && hadHookInstance)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            // PlayAnimation("Grapple", FireHookExitHash);
            if (muzzleflashEffectPrefab != null)
            {
                EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, gameObject, muzzleString, transmit: false);
            }

            // Clean up hook if still alive when state is forcibly exited
            if (hookInstance != null)
            {
                if (NetworkServer.active)
                {
                    EntityState.Destroy(hookInstance);
                }
            }

            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
