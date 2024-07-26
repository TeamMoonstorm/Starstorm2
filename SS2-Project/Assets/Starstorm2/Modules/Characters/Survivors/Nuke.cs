using R2API;
using R2API.ScriptableObjects;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Moonstorm.Starstorm2.Survivors
{
    public class Nuke : SurvivorBase
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
    }
}