using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;

namespace SS2.Survivors
{
    public sealed class Duke : SS2Survivor, IContentPackModifier
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acDuke", SS2Bundle.Indev);

        public static ModdedProcType RicochetProc { get; private set; }

        public static BuffDef bdDukeStun;

        // Ricochet tuning
        private static float ricochetDamageCoefficient = 1f;
        private static float ricochetRange = 30f;
        private static float ricochetProcCoefficient = 0.5f;

        // Ricochet VFX
        public static GameObject ricochetTracerPrefab;
        public static GameObject ricochetHitPrefab;

        public override bool IsAvailable(ContentPack contentPack)
        {
            return SS2Config.enableBeta && base.IsAvailable(contentPack);
        }

        public override void Initialize()
        {
            RicochetProc = ProcTypeAPI.ReserveProcType();

            bdDukeStun = AssetCollection.FindAsset<BuffDef>("bdDukeStun");
            if (!bdDukeStun)
                Debug.LogError("[Duke] Failed to find bdDukeStun BuffDef in asset collection. Stun and ricochet-on-stun will not work.");

            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;

            AssignEffects();
            ModifyPrefab();
        }


        private void ModifyPrefab()
        {
            if (CharacterPrefab.TryGetComponent(out CharacterBody cb))
            {
                cb.preferredPodPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/SurvivorPod/SurvivorPod.prefab").WaitForCompletion();
                cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2Crosshair.prefab").WaitForCompletion();
            }
            else
            {
                Debug.LogError("[Duke] ModifyPrefab: CharacterPrefab missing CharacterBody component.");
            }
        }

        private void AssignEffects()
        {
            // M1 Deagle VFX
            EntityStates.Duke.FireDeagle.muzzleEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/MuzzleflashFMJ.prefab").WaitForCompletion();
            EntityStates.Duke.FireDeagle.tracerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/TracerCommandoDefault.prefab").WaitForCompletion();
            EntityStates.Duke.FireDeagle.hitPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/HitsparkCommando.prefab").WaitForCompletion();

            // ADS Shot VFX
            EntityStates.Duke.FireADSShot.muzzleEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/MuzzleflashRailgun.prefab").WaitForCompletion();
            EntityStates.Duke.FireADSShot.tracerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/TracerCommandoDefault.prefab").WaitForCompletion();
            EntityStates.Duke.FireADSShot.hitPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/HitsparkCommando.prefab").WaitForCompletion();

            // ADS weakpoint crosshair — clone StandardCrosshair and add weakpoint visualization (Pilot pattern)
            var baseCrosshair = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion();
            var dukeCrosshair = UnityEngine.Object.Instantiate(baseCrosshair);
            dukeCrosshair.name = "DukeADSCrosshair";
            var visualizerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerSniperTargetVisualizerLight.prefab").WaitForCompletion();
            dukeCrosshair.AddComponent<RoR2.UI.PointViewer>(); // required by SniperTargetViewer
            var targetViewer = dukeCrosshair.AddComponent<SniperTargetViewer>();
            targetViewer.visualizerPrefab = visualizerPrefab;
            EntityStates.Duke.AimDownSights.crosshairOverridePrefab = dukeCrosshair;

            // Stun cone VFX
            EntityStates.Duke.StunCone.effectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniExplosionVFXQuick.prefab").WaitForCompletion();

            // Ricochet VFX
            ricochetTracerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/TracerCommandoDefault.prefab").WaitForCompletion();
            ricochetHitPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/HitsparkCommando.prefab").WaitForCompletion();
        }

        private void OnServerDamageDealt(DamageReport damageReport)
        {
            if (!NetworkServer.active) return;

            DamageInfo damageInfo = damageReport.damageInfo;
            CharacterBody attackerBody = damageReport.attackerBody;
            CharacterBody victimBody = damageReport.victimBody;

            if (!attackerBody || !victimBody) return;

            // Only Duke bodies trigger ricochet
            if (!attackerBody.TryGetComponent(out Components.DukeController _)) return;

            // Prevent ricochet chain (n=1 only)
            if (damageInfo.procChainMask.HasModdedProc(RicochetProc)) return;

            // Trigger conditions: crit OR victim has Duke stun debuff
            bool shouldRicochet = damageInfo.crit || (bdDukeStun && victimBody.HasBuff(bdDukeStun));
            if (!shouldRicochet) return;

            // Find nearest enemy to the victim
            HurtBox targetHurtBox = FindRicochetTarget(attackerBody, victimBody);
            if (!targetHurtBox) return;

            FireRicochet(damageInfo, attackerBody, victimBody, targetHurtBox);
        }

        private HurtBox FindRicochetTarget(CharacterBody attackerBody, CharacterBody victimBody)
        {
            var search = new BullseyeSearch();
            search.searchOrigin = victimBody.corePosition;
            search.searchDirection = Vector3.zero;
            search.teamMaskFilter = TeamMask.GetEnemyTeams(attackerBody.teamComponent.teamIndex);
            search.filterByLoS = true;
            search.sortMode = BullseyeSearch.SortMode.Distance;
            search.maxDistanceFilter = ricochetRange;
            search.filterByDistinctEntity = true;
            search.RefreshCandidates();

            // Exclude the victim itself
            return search.GetResults()
                .Where(h => h && h.healthComponent && h.healthComponent.alive && h.healthComponent != victimBody.healthComponent)
                .FirstOrDefault();
        }

        private void FireRicochet(DamageInfo originalDamage, CharacterBody attackerBody, CharacterBody victimBody, HurtBox target)
        {
            Vector3 origin = victimBody.corePosition;
            Vector3 direction = (target.transform.position - origin).normalized;

            // Offset origin slightly toward the target to avoid re-hitting the victim's own collider
            origin += direction * (victimBody.radius + 0.5f);

            ProcChainMask procMask = originalDamage.procChainMask;
            procMask.AddModdedProc(RicochetProc);

            DamageTypeCombo damageType = DamageType.Generic;

            var bulletAttack = new BulletAttack
            {
                aimVector = direction,
                origin = origin,
                damage = originalDamage.damage * ricochetDamageCoefficient,
                damageType = damageType,
                damageColorIndex = DamageColorIndex.Default,
                minSpread = 0f,
                maxSpread = 0f,
                falloffModel = BulletAttack.FalloffModel.None,
                maxDistance = ricochetRange,
                force = 100f,
                isCrit = originalDamage.crit,
                owner = attackerBody.gameObject,
                muzzleName = "",
                smartCollision = true,
                procChainMask = procMask,
                procCoefficient = ricochetProcCoefficient,
                radius = 0.5f,
                weapon = attackerBody.gameObject,
                tracerEffectPrefab = ricochetTracerPrefab,
                hitEffectPrefab = ricochetHitPrefab
            };
            bulletAttack.Fire();

            Debug.Log("[Duke] Ricochet fired from " + victimBody.name + " toward " + target.healthComponent.body.name);
        }
    }
}
