using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace SS2.Survivors
{
    public sealed class Lancer : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acLancer", SS2Bundle.Indev);

        public static BuffDef bdIonField;

        // Ion field tuning
        public static float ionFieldDuration = 5f;
        public static int ionFieldMaxStacks = 5;

        public static DamageAPI.ModdedDamageType TipperIonFieldDamageType { get; set; }

        // Code-created projectile prefabs
        private static GameObject lancerSpearProjectile;
        private static GameObject lancerIonFistProjectile;

        public override bool IsAvailable(ContentPack contentPack)
        {
            return SS2Config.enableBeta && base.IsAvailable(contentPack);
        }

        public override void Initialize()
        {
            bdIonField = AssetCollection.FindAsset<BuffDef>("bdIonField");

            if (bdIonField)
            {
                bdIonField.canStack = true;
                bdIonField.maxStacks = ionFieldMaxStacks;
            }

            TipperIonFieldDamageType = DamageAPI.ReserveDamageType();
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;

            CreateProjectiles();
            AssignEffects();
            ModifyPrefab();
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            base.ModifyContentPack(contentPack);
            contentPack.projectilePrefabs.Add(new GameObject[] { lancerSpearProjectile, lancerIonFistProjectile });
        }

        private void CreateProjectiles()
        {
            CreateSpearProjectile();
            CreateIonFistProjectile();
        }

        private void CreateSpearProjectile()
        {
            var original = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2ShivProjectile.prefab").WaitForCompletion();
            lancerSpearProjectile = PrefabAPI.InstantiateClone(original, "LancerSpearProjectile", true);

            // Remove vanilla impact/stick behavior — LancerSpearProjectile handles everything
            if (lancerSpearProjectile.TryGetComponent(out ProjectileStickOnImpact stick))
                Object.DestroyImmediate(stick);
            else
                Debug.LogError("[Lancer] Bandit2ShivProjectile clone missing ProjectileStickOnImpact");

            if (lancerSpearProjectile.TryGetComponent(out ProjectileSingleTargetImpact singleTarget))
                Object.DestroyImmediate(singleTarget);
            else
                Debug.LogError("[Lancer] Bandit2ShivProjectile clone missing ProjectileSingleTargetImpact");

            if (lancerSpearProjectile.TryGetComponent(out RoR2.EntityLogic.DelayedEvent delayedEvent))
                Object.DestroyImmediate(delayedEvent);

            if (lancerSpearProjectile.TryGetComponent(out RoR2.EventFunctions eventFunctions))
                Object.DestroyImmediate(eventFunctions);

            // Configure ProjectileSimple
            if (lancerSpearProjectile.TryGetComponent(out ProjectileSimple ps))
            {
                ps.desiredForwardSpeed = 80f;
                ps.lifetime = 30f;
                ps.updateAfterFiring = false;
            }
            else
            {
                Debug.LogError("[Lancer] Spear projectile missing ProjectileSimple");
            }

            // Disable gravity for linear travel
            if (lancerSpearProjectile.TryGetComponent(out Rigidbody rb))
            {
                rb.useGravity = false;
            }

            // Add our custom spear behavior
            lancerSpearProjectile.AddComponent<Components.LancerSpearProjectile>();

            EnsureNetworkIdentity(lancerSpearProjectile);

            Debug.Log("[Lancer] Created LancerSpearProjectile prefab");
        }

        private void CreateIonFistProjectile()
        {
            var original = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Seeker/SpiritPunchProjectile.prefab").WaitForCompletion();
            lancerIonFistProjectile = PrefabAPI.InstantiateClone(original, "LancerIonFistProjectile", true);

            // Keep all Seeker VFX (ghost, dissipate, impact) — reusing vanilla assets
            // Keep ProjectileOverlapAttack, trigger collider, HitBox child, fireFrequency=60
            // Speed and lifetime already good (70 speed, 0.75s lifetime)

            EnsureNetworkIdentity(lancerIonFistProjectile);

            Debug.Log("[Lancer] Created LancerIonFistProjectile prefab (SpiritPunch clone)");
        }

        private void AssignEffects()
        {
            // Cache shared loads
            var muzzleflashFMJ = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/MuzzleflashFMJ.prefab").WaitForCompletion();
            var omniImpactVFXSlash = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniImpactVFXSlash.prefab").WaitForCompletion();

            // ThrowSpear
            EntityStates.Lancer.ThrowSpear.projectilePrefab = lancerSpearProjectile;
            EntityStates.Lancer.ThrowSpear.muzzleflashEffectPrefab = muzzleflashFMJ;

            // IonFistPunch
            EntityStates.Lancer.IonFistPunch.projectilePrefab = lancerIonFistProjectile;
            EntityStates.Lancer.IonFistPunch.muzzleflashEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Seeker/SpiritPunchMuzzleFlashVFX.prefab").WaitForCompletion();

            // IonDetonation
            EntityStates.Lancer.IonDetonation.novaEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Vagrant/VagrantNovaExplosion.prefab").WaitForCompletion();
            EntityStates.Lancer.IonDetonation.novaImpactEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/OmniImpactVFXLoader.prefab").WaitForCompletion();

            // IonFieldBehavior tick effect
            Components.IonFieldBehavior.tickEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniExplosionVFXQuick.prefab").WaitForCompletion();

            // SpearThrust (bridging static → instance fields in OnEnter)
            EntityStates.Lancer.SpearThrust.spearSwingEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlash.prefab").WaitForCompletion();
            EntityStates.Lancer.SpearThrust.spearHitEffectPrefab = omniImpactVFXSlash;

            Debug.Log("[Lancer] All VFX/projectile references assigned");
        }

        private static void EnsureNetworkIdentity(GameObject prefab)
        {
            if (!prefab.GetComponent<NetworkIdentity>())
                prefab.AddComponent<NetworkIdentity>();
        }

        private static void OnServerDamageDealt(DamageReport damageReport)
        {
            if (DamageAPI.HasModdedDamageType(damageReport.damageInfo, TipperIonFieldDamageType))
            {
                if (damageReport.victimBody)
                {
                    damageReport.victimBody.AddTimedBuff(bdIonField, ionFieldDuration);
                }
            }
        }

        private void ModifyPrefab()
        {
            var cb = CharacterPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/SurvivorPod/SurvivorPod.prefab").WaitForCompletion();
        }
    }
}
