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
        private DotBuffDef _dbdNuclearSickness;
        public override void Initialize()
        {
            FindAndInitAssets();
            NuclearDamageType = DamageAPI.ReserveDamageType();
            GlobalEventManager.onServerDamageDealt += InflictNuclearSickness;
        }

        private void FindAndInitAssets()
        {
            var damageColor = AssetCollection.FindAsset<SerializableDamageColor>("NukeDamageColor");
            ColorsAPI.AddSerializableDamageColor(damageColor);

            /*_dbdNuclearSickness = AssetCollection.FindAsset<DotBuffDef>("dbdNuclearSickness");
            _dbdNuclearSickness.Init();*/
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
                victimBody.AddTimedBuff(SS2Content.Buffs.bdIrradiated, 2, 10);
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public class NukeRadiationSicknessBehaviour : BaseBuffBehaviour, IBodyStatArgModifier, IOnIncomingDamageOtherServerReciever
        {
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (!HasAnyStacks)
                    return;

                args.levelMultAdd -= BuffCount / 10;
            }

            public void OnIncomingDamageOther(HealthComponent victimHealthComponent, DamageInfo damageInfo)
            {
                if (!HasAnyStacks)
                    return;

                var dmg = damageInfo.damage;
                damageInfo.damage += (dmg * (BuffCount / 10));
            }
        }
    }
    /*public class Nuke : SurvivorBase
    {
        public override SurvivorDef SurvivorDef => SS2Assets.LoadAsset<SurvivorDef>("Nuke", SS2Bundle.Indev);

        public override GameObject BodyPrefab => SS2Assets.LoadAsset<GameObject>("NukeBody", SS2Bundle.Indev);

        public override GameObject MasterPrefab => null;
        public override void Initialize()
        {
            base.Initialize();
            var selfDamage = SS2Assets.LoadAsset<BuffDef>("bdNukeSelfDamage", SS2Bundle.Indev);
            var immune = SS2Assets.LoadAsset<BuffDef>("bdNukeSpecial", SS2Bundle.Indev);

            HG.ArrayUtils.ArrayAppend(ref SS2Content.Instance.SerializableContentPack.buffDefs, selfDamage);
            HG.ArrayUtils.ArrayAppend(ref SS2Content.Instance.SerializableContentPack.buffDefs, immune);
        }

        public override void ModifyPrefab()
        {
            base.ModifyPrefab();

            var cb = BodyPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
            var ctp = BodyPrefab.GetComponent<CameraTargetParams>();
            ctp.cameraParams = Addressables.LoadAssetAsync<CharacterCameraParams>("RoR2/Base/Croco/ccpCroco.asset").WaitForCompletion();

            SS2Assets.LoadAsset<GameObject>("NukeSludgeProjectile", SS2Bundle.Indev).AddComponent<R2API.DamageAPI.ModdedDamageTypeHolderComponent>().Add(DamageTypes.Nuclear.NuclearDamageType);
            SS2Assets.LoadAsset<GameObject>("NukePoolDOT", SS2Bundle.Indev).AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(DamageTypes.Nuclear.NuclearDamageType);
        }
    }*/
}