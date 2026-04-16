using MSU;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using RoR2.ContentManagement;
using RoR2.Projectile;
using SS2.Components;
using R2API;
using EntityStates.NemToolbot;

#if DEBUG
namespace SS2.Survivors
{
    public sealed class NemToolbot : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acNemToolbot", SS2Bundle.Indev);

        private GameObject grappleHookProjectile;
        private GameObject grenadeProjectile;
        private GameObject deployChargeProjectile;

        public override void Initialize()
        {
            ModifyPrefab();
            CreateProjectiles();
            AssignEffects();
        }

        private void ModifyPrefab()
        {
            if (!CharacterPrefab.TryGetComponent(out CharacterBody cb))
            {
                Debug.LogError("NemToolbot.ModifyPrefab: Failed to get CharacterBody on " + CharacterPrefab.name);
                return;
            }

            cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion();
            cb.preferredPodPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/RoboCratePod.prefab").WaitForCompletion();

            if (cb.TryGetComponent(out ModelLocator modelLocator) && modelLocator.modelTransform != null)
            {
                if (modelLocator.modelTransform.TryGetComponent(out FootstepHandler footstepHandler))
                {
                    footstepHandler.footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
                }
            }
        }

        private static void EnsureNetworkIdentity(GameObject prefab)
        {
            if (!prefab.GetComponent<NetworkIdentity>())
            {
                prefab.AddComponent<NetworkIdentity>();
            }
        }

        private void CreateProjectiles()
        {
            CreateGrappleHookProjectile();
            CreateGrenadeProjectile();
            CreateDeployChargeProjectile();
        }

        private void CreateGrappleHookProjectile()
        {
            GameObject loaderHook = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/LoaderHook.prefab").WaitForCompletion();
            grappleHookProjectile = loaderHook.InstantiateClone("NemToolbotGrappleHook", true);
            EnsureNetworkIdentity(grappleHookProjectile);

            // Null out ropeEndTransform before destroy to prevent vanilla OnDestroy
            // from deleting the RopeEnd child we need for our controller
            ProjectileGrappleController loaderGrapple = grappleHookProjectile.GetComponent<ProjectileGrappleController>();
            if (loaderGrapple != null)
            {
                loaderGrapple.ropeEndTransform = null;
                Object.DestroyImmediate(loaderGrapple);
            }

            // Reconfigure the projectile's ESM before adding our controller
            // (RequireComponent won't duplicate it since one already exists from the clone)
            if (grappleHookProjectile.TryGetComponent(out EntityStateMachine esm))
            {
                NemToolbotGrappleController.ConfigureProjectileESM(esm);
            }
            else
            {
                Debug.LogError("NemToolbot: Cloned LoaderHook missing EntityStateMachine.");
            }

            // Add our grapple controller and configure
            NemToolbotGrappleController grappleController = grappleHookProjectile.AddComponent<NemToolbotGrappleController>();
            grappleController.ownerHookStateType = new EntityStates.SerializableEntityStateType(typeof(FireGrapplingHook));
            grappleController.hookStateMachineName = "Hook";
            grappleController.acceleration = 45f;
            grappleController.lookAcceleration = 35f;
            grappleController.lookAccelerationRampUpDuration = 0.5f;
            grappleController.initialLookImpulse = 0f;
            grappleController.initiallMoveImpulse = 15f;
            grappleController.moveAcceleration = 5f;
            grappleController.nearBreakDistance = 1f;
            grappleController.maxTravelDistance = 80f;
            grappleController.escapeForceMultiplier = 4f;
            grappleController.normalOffset = 1f;
            grappleController.yankMassLimit = 0f;
            grappleController.muzzleStringOnBody = "MuzzleLeft";
            grappleController.deductStockSlot = NemToolbotGrappleController.DeductSlot.Secondary;
            grappleController.lookAccelerationRampUpCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            grappleController.enterSoundString = "Play_loader_m2_travel_loop";
            grappleController.exitSoundString = "Stop_loader_m2_travel_loop";
            grappleController.hookDistanceRTPCstring = "loaderM2_grappleRemain";
            grappleController.minHookDistancePitchModifier = 3f;
            grappleController.maxHookDistancePitchModifier = 80f;

            // Find and assign the rope end transform from the cloned prefab
            Transform ropeEnd = grappleHookProjectile.transform.Find("RopeEnd");
            if (ropeEnd != null)
            {
                grappleController.ropeEndTransform = ropeEnd;
            }
            else
            {
                Debug.LogError("NemToolbot: Cloned LoaderHook missing RopeEnd child transform.");
            }
        }

        private void CreateGrenadeProjectile()
        {
            GameObject toolbotGrenade = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/ToolbotGrenadeLauncherProjectile.prefab").WaitForCompletion();
            grenadeProjectile = toolbotGrenade.InstantiateClone("NemToolbotGrenadeProjectile", true);
            EnsureNetworkIdentity(grenadeProjectile);

            // Enable gravity for arc trajectory
            if (grenadeProjectile.TryGetComponent(out Rigidbody rb))
            {
                rb.useGravity = true;
            }
            else
            {
                Debug.LogError("NemToolbot: Cloned grenade missing Rigidbody.");
            }

            // Set explosion VFX
            if (grenadeProjectile.TryGetComponent(out ProjectileImpactExplosion pie))
            {
                pie.impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/OmniExplosionVFXToolbotQuick.prefab").WaitForCompletion();
            }
        }

        private void CreateDeployChargeProjectile()
        {
            GameObject toolbotGrenade = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/ToolbotGrenadeLauncherProjectile.prefab").WaitForCompletion();
            deployChargeProjectile = toolbotGrenade.InstantiateClone("NemToolbotDeployCharge", true);
            EnsureNetworkIdentity(deployChargeProjectile);

            // Stationary: no forward movement, no gravity
            if (deployChargeProjectile.TryGetComponent(out ProjectileSimple ps))
            {
                ps.desiredForwardSpeed = 0f;
                ps.lifetime = 4f;
            }
            else
            {
                Debug.LogError("NemToolbot: Cloned deploy charge missing ProjectileSimple.");
            }

            if (deployChargeProjectile.TryGetComponent(out Rigidbody rb))
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            // Timed detonation: explode after lifetime, not on contact
            if (deployChargeProjectile.TryGetComponent(out ProjectileImpactExplosion pie))
            {
                pie.destroyOnWorld = false;
                pie.destroyOnEnemy = false;
                pie.timerAfterImpact = false;
                pie.lifetime = 4f;
                pie.impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/OmniExplosionVFXToolbotQuick.prefab").WaitForCompletion();
            }
            else
            {
                Debug.LogError("NemToolbot: Cloned deploy charge missing ProjectileImpactExplosion.");
            }
        }

        private void AssignEffects()
        {
            // Cache shared loads
            GameObject muzzleflashFMJ = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/MuzzleflashFMJ.prefab").WaitForCompletion();
            GameObject hitsparkBandit = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/HitsparkBandit.prefab").WaitForCompletion();
            GameObject loaderGroundSlam = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/LoaderGroundSlam.prefab").WaitForCompletion();

            // Shotgun
            FireShotgun.muzzleFlashPrefab = muzzleflashFMJ;
            FireShotgun.tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/TracerBandit2Shotgun.prefab").WaitForCompletion();
            FireShotgun.hitEffectPrefab = hitsparkBandit;

            // Rapid laser
            FireRapidLaser.muzzleFlashPrefab = muzzleflashFMJ;
            FireRapidLaser.tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/CaptainDefenseMatrix/TracerCaptainDefenseMatrix.prefab").WaitForCompletion();
            FireRapidLaser.hitEffectPrefab = hitsparkBandit;

            // Sniper laser
            FireSniperLaser.muzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/MuzzleflashRailgun.prefab").WaitForCompletion();
            FireSniperLaser.tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/TracerToolbotRebar.prefab").WaitForCompletion();
            FireSniperLaser.hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/ImpactRailgun.prefab").WaitForCompletion();

            // Grenade launcher
            FireGrenadeLauncher.muzzleFlashPrefab = muzzleflashFMJ;
            FireGrenadeLauncher.projectilePrefab = grenadeProjectile;

            // Deploy charges
            DeployCharges.muzzleFlashPrefab = muzzleflashFMJ;
            DeployCharges.chargeProjectilePrefab = deployChargeProjectile;

            // Grappling hook
            FireGrapplingHook.muzzleflashEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/MuzzleflashLoader.prefab").WaitForCompletion();
            FireGrapplingHook.projectilePrefab = grappleHookProjectile;

            // Ball boost
            BallBoost.startEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/ImpactToolbotDash.prefab").WaitForCompletion();
            BallBoost.impactEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/ImpactToolbotDashLarge.prefab").WaitForCompletion();

            // Ball slam
            BallSlam.blastEffectPrefab = loaderGroundSlam;
            BallSlam.blastImpactEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/OmniImpactVFXLoader.prefab").WaitForCompletion();
            BallSlam.slamEffectPrefab = loaderGroundSlam;
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            base.ModifyContentPack(contentPack);
            contentPack.projectilePrefabs.Add(new GameObject[]
            {
                grappleHookProjectile,
                grenadeProjectile,
                deployChargeProjectile,
            });
        }

        // NOTE: Must return true (or use SS2Config beta gate) for Initialize()
        // and ModifyContentPack() to run. MSU skips both when IsAvailable=false.
        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }
    }
}
#endif
