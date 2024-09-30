using SS2;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2;
using MSU;
using RoR2.Skills;
using RoR2.ContentManagement;
using R2API;

namespace SS2.Survivors
{
    public class Acrid : SS2VanillaSurvivor
    {
        public override SS2AssetRequest<VanillaSurvivorAssetCollection> assetRequest => SS2Assets.LoadAssetAsync<VanillaSurvivorAssetCollection>("acAcrid", SS2Bundle.Vanilla);

        public static DamageAPI.ModdedDamageType ArmorCorrison { get; set; }

        public static GameObject corrodingSpitProjectilePrefab;

        public static float armorCorrisonDuration = 1f;
        private static float armorLoseAmount = 5f;

        public override void Initialize()
        {
            /*RegisterArmorCorrison();
            R2API.RecalculateStatsAPI.GetStatCoefficients += ModifyStats;

            CreateProjectilePrefab();

            SkillDef sdCorrodingSpit = survivorAssetCollection.FindAsset<SkillDef>("sdCorrodingSpit");

            GameObject acridBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoBody.prefab").WaitForCompletion();

            SkillLocator skillLocator = acridBodyPrefab.GetComponent<SkillLocator>();
            SkillFamily skillFamily = skillLocator.primary.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = sdCorrodingSpit,
                viewableNode = new ViewablesCatalog.Node(sdCorrodingSpit.skillNameToken, false, null)
            };*/
        }

        private void CreateProjectilePrefab()
        {
            corrodingSpitProjectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoSpit.prefab").WaitForCompletion().InstantiateClone("CorrodingSpitProjectile");

            var damageAPIComponent = corrodingSpitProjectilePrefab.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            damageAPIComponent.Add(ArmorCorrison);
        }

        private void RegisterArmorCorrison()
        {
            ArmorCorrison = R2API.DamageAPI.ReserveDamageType();
            GlobalEventManager.onServerDamageDealt += ApplyArmorCorrison;
        }

        private void ModifyStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(SS2Content.Buffs.bdAcridArmorCorrison))
            {
                args.armorAdd -= armorLoseAmount;
            }
        }

        private void ApplyArmorCorrison(DamageReport obj)
        {
            var victimBody = obj.victimBody;
            var damageInfo = obj.damageInfo;

            if (DamageAPI.HasModdedDamageType(damageInfo, ArmorCorrison))
            {
                victimBody.AddTimedBuffAuthority(SS2Content.Buffs.bdAcridArmorCorrison.buffIndex, armorCorrisonDuration);
            }
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
