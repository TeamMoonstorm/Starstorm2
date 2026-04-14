using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;

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

            ModifyPrefab();
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
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
        }
    }
}
