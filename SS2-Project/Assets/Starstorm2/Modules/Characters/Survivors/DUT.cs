using MSU;
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static R2API.DamageAPI;
using RoR2.ContentManagement;
using SS2.Orbs;
using RoR2.Orbs;
using static R2API.DamageAPI;

#if DEBUG
namespace SS2.Survivors
{

    public sealed class DUT : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acDUT", SS2Bundle.Indev);
        public static ModdedDamageType DUTDamageType { get; private set; }
        public override void Initialize()
        {
            ModifyPrefab();
            DUTDamageType = DamageAPI.ReserveDamageType();
            GlobalEventManager.onServerDamageDealt += CheckDUT;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        private void CheckDUT(DamageReport report)
        {
            var victimBody = report.victimBody;
            var attackerBody = report.attackerBody;
            var damageInfo = report.damageInfo;
            if (DamageAPI.HasModdedDamageType(damageInfo, DUTDamageType))
            {
                DUTChargeOrb orb = new DUTChargeOrb();
                orb.origin = victimBody.transform.position;
                orb.target = Util.FindBodyMainHurtBox(attackerBody);
                OrbManager.instance.AddOrb(orb);
                //Debug.Log("DUT Damage Type");
            }
        }

        public void ModifyPrefab()
        {
            var cb = CharacterPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/RoboCratePod.prefab").WaitForCompletion();
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
        }
    }
}
#endif