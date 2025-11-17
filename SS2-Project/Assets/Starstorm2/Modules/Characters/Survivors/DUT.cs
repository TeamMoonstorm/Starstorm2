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

namespace SS2.Survivors
{

    public sealed class DUT : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acDUT", SS2Bundle.Indev);
        public static ModdedDamageType DUTDamageType { get; private set; }

        public static GameObject _dutOrbRed;
        public static GameObject _dutOrbGreen;
        public override void Initialize()
        {
            ModifyPrefab();
            DUTDamageType = ReserveDamageType();

            _dutOrbRed = AssetCollection.FindAsset<GameObject>("DUTOrbEffectRed");
            _dutOrbGreen = AssetCollection.FindAsset<GameObject>("DUTOrbEffectGreen");

            On.RoR2.CharacterBody.RecalculateStats += DUTDrift;
            R2API.RecalculateStatsAPI.GetStatCoefficients += DUTDriftBoost;
            GlobalEventManager.onServerDamageDealt += CheckDUT;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        private void DUTDrift(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self.HasBuff(SS2Content.Buffs.bdDUTDrift))
            {
                self.acceleration /= 6;
            }
        }

        private void DUTDriftBoost(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (!sender.HasBuff(SS2Content.Buffs.bdDUTDrift))
                return;

            args.moveSpeedMultAdd += 1.5f;
        }

        private void CheckDUT(DamageReport report)
        {
            var victimBody = report.victimBody;
            var attackerBody = report.attackerBody;
            var damageInfo = report.damageInfo;
            // TO-DO: i think this never worked, or did work with extreme issues.
            // reimplement orbs. theyre just visual orbs. i think they mightve been 
            /* if (damageInfo.HasModdedDamageType(DUTDamageType))
            {
                DUTRedOrb orb = new DUTRedOrb();
                orb.origin = victimBody.transform.position;
                orb.target = Util.FindBodyMainHurtBox(attackerBody);
                OrbManager.instance.AddOrb(orb);
                //Debug.Log("DUT Damage Type");
            }*/
        }

        public void ModifyPrefab()
        {
            var cb = CharacterPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/RoboCratePod.prefab").WaitForCompletion();
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
        }
    }
}