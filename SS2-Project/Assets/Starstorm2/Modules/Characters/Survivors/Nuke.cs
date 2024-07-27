using MSU;
using R2API;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SS2.Survivors
{
    public class Nuke : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acNuke", SS2Bundle.Nuke);

        public static DamageAPI.ModdedDamageType NuclearDamageType { get; private set; }
        public static DotController.DotIndex NuclearSicknessDotIndex => _dbdNuclearSickness.DotIndex;
        private static DotBuffDef _dbdNuclearSickness;
        public override void Initialize()
        {
            FindAndInitAssets();
            NuclearDamageType = DamageAPI.ReserveDamageType();
            GlobalEventManager.onServerDamageDealt += InflictNuclearSickness;
            R2API.RecalculateStatsAPI.GetStatCoefficients += StatChanges;
            ModifyPrefabs();
        }

        private void StatChanges(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var irradiatedBuffCount = (float)sender.GetBuffCount(SS2Content.Buffs.bdIrradiated);
            var sicknessBuffCount = (float)sender.GetBuffCount(SS2Content.Buffs.dbdNuclearSickness);

            float finalCount = 0;

            if(irradiatedBuffCount > 0 && sicknessBuffCount > 0)
            {
                finalCount = (irradiatedBuffCount + sicknessBuffCount) / 2;
            }
            else
            {
                finalCount += sicknessBuffCount;
                finalCount += irradiatedBuffCount;
            }
            args.levelMultAdd -= finalCount / 10;
            args.armorAdd -= sicknessBuffCount * 10;
        }

        private void FindAndInitAssets()
        {
            var damageColor = AssetCollection.FindAsset<SerializableDamageColor>("NukeDamageColor");
            ColorsAPI.AddSerializableDamageColor(damageColor);
            damageColor = AssetCollection.FindAsset<SerializableDamageColor>("NukeSelfDamageColor");
            ColorsAPI.AddSerializableDamageColor(damageColor);

            _dbdNuclearSickness = AssetCollection.FindAsset<DotBuffDef>("dbdNuclearSickness");
            _dbdNuclearSickness.Init();
        }

        private void ModifyPrefabs()
        {
            var cb = CharacterPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
            var ctp = CharacterPrefab.GetComponent<CameraTargetParams>();
            ctp.cameraParams = Addressables.LoadAssetAsync<CharacterCameraParams>("RoR2/Base/Croco/ccpCroco.asset").WaitForCompletion();

            AssetCollection.FindAsset<GameObject>("NukeSludgeProjectile").AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(NuclearDamageType);
            AssetCollection.FindAsset<GameObject>("NukePoolDOT").AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(NuclearDamageType);
        }

        private void InflictNuclearSickness(DamageReport report)
        {
            var victimBody = report.victimBody;
            var attackerBody = report.attackerBody;
            var damageInfo = report.damageInfo;

            if (victimBody && attackerBody && damageInfo.HasModdedDamageType(NuclearDamageType))
            {
                if (attackerBody.HasBuff(SS2Content.Buffs.bdNukeSpecial))
                {
                    var dotInfo = new InflictDotInfo
                    {
                        attackerObject = attackerBody.gameObject,
                        damageMultiplier = 1,
                        dotIndex = _dbdNuclearSickness.DotIndex,
                        duration = 10,
                        victimObject = victimBody.gameObject
                    };
                    DotController.InflictDot(ref dotInfo);
                }
                else
                {
                    victimBody.AddTimedBuff(SS2Content.Buffs.bdIrradiated, 2, 10);
                }
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public interface IChargeableState
        {
            float currentCharge { get; }
            float chargeCoefficientSoftCap { get; }
            float chargeCoefficientHardCap { get; }
        }

        public interface IChargedState
        {
            float charge { get; set; }
        }
    }
}