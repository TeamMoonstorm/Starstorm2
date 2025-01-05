using SS2;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2;
using MSU;
using RoR2.Skills;
using RoR2.ContentManagement;
using R2API;
using UnityEngine.Networking;

namespace SS2.Survivors
{
    public class Bandit : SS2VanillaSurvivor
    {
        public override SS2AssetRequest<VanillaSurvivorAssetCollection> assetRequest => SS2Assets.LoadAssetAsync<VanillaSurvivorAssetCollection>("acBandit2", SS2Bundle.Indev);

        public static DamageAPI.ModdedDamageType TranqDamageType { get; set; }
        public static BuffDef _bdBanditTranquilizer;
        public static BuffDef _bdBanditSleep;

        public static float tranqDuration = 5f;
        public static float _confuseSlowAmount = 0.2f;
        public static float _confuseAttackSpeedSlowAmount = 0.2f;
        public static float _maxDebuffAmount = 0.5f;
        public static float _sleepCountThreshold = 3;
        public static float _sleepDuration = 6;
        public static float _armorLoseAmount = 25;

        public override void Initialize()
        {
            _bdBanditTranquilizer = assetCollection.FindAsset<BuffDef>("bdBanditTranquilizer");
            _bdBanditSleep = assetCollection.FindAsset<BuffDef>("bdBanditSleep");

            RegisterTranquilizer();
            R2API.RecalculateStatsAPI.GetStatCoefficients += ModifyStats;

            SkillDef sdTranquilizerGun = assetCollection.FindAsset<SkillDef>("sdTranquilizerGun");

            GameObject banditBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2Body.prefab").WaitForCompletion();

            SkillLocator skillLocator = banditBodyPrefab.GetComponent<SkillLocator>();
            SkillFamily skillFamilyPrimary = skillLocator.primary.skillFamily;

            AddSkill(skillFamilyPrimary, sdTranquilizerGun);
        }

        private void RegisterTranquilizer()
        {
            TranqDamageType = R2API.DamageAPI.ReserveDamageType();
            GlobalEventManager.onServerDamageDealt += ApplyTranq;
        }

        private void ModifyStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(_bdBanditTranquilizer))
            {
                // TODO: Might need to do some sort of scaling, but this works for now.
                var buffCount = sender.GetBuffCount(_bdBanditTranquilizer);
                args.moveSpeedMultAdd -= Math.Min(_confuseSlowAmount * buffCount, _maxDebuffAmount);
                args.attackSpeedMultAdd -= Math.Min(_confuseAttackSpeedSlowAmount * buffCount, _maxDebuffAmount);

                if (buffCount >= _sleepCountThreshold)
                {
                    if (NetworkServer.active)
                    {
                        // TODO: half sleep on bosses
                        sender.AddTimedBuff(_bdBanditSleep, _sleepDuration);
                        sender.RemoveBuff(_bdBanditTranquilizer);
                    }
                }
            }

            if (sender.HasBuff((_bdBanditSleep)))
            {
                // Stun the enemy, thanks orbeez for the code
                SetStateOnHurt setStateOnHurt = sender.GetComponent<SetStateOnHurt>();
                if (setStateOnHurt) setStateOnHurt.SetStun(_sleepDuration);

                // Make them vulnerable and take more damage
                args.armorAdd -= _armorLoseAmount;

            }
        }

        private void ApplyTranq(DamageReport obj)
        {
            var victimBody = obj.victimBody;
            var damageInfo = obj.damageInfo;

            if (DamageAPI.HasModdedDamageType(damageInfo, TranqDamageType))
            {
                victimBody.AddTimedBuffAuthority(_bdBanditTranquilizer.buffIndex, tranqDuration);
            }
        }
        public override void ModifyContentPack(ContentPack contentPack)
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
