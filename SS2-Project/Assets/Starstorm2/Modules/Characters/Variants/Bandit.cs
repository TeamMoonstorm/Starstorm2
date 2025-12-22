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


        // TODO Make these statics configurable
        public static float tranqDuration = 5f;

        [FormatToken("SS2_BANDIT_TRANQ_GUN_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float tranqDamageAmount = 2.7f;

        public static float tranqBulletRadius = 2.0f;


        public static float _confuseSlowAmount = 0.1f;
        public static float _confuseAttackSpeedSlowAmount = 0.1f;
        public static float _sleepCountThreshold = 5;
        public static float _bossSleepCountThreshold = 12;
        public static float _sleepDuration = 2;
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
                // Simple linear scaling for now. Used to be capped but I uncapped it so it remains usable late game
                var buffCount = sender.GetBuffCount(_bdBanditTranquilizer);
                args.moveSpeedReductionMultAdd += _confuseSlowAmount * buffCount;
                args.attackSpeedReductionMultAdd += _confuseAttackSpeedSlowAmount * buffCount;

                // Normal enemies go zzz
                if ((!sender.isBoss && !sender.isChampion) && buffCount >= _sleepCountThreshold)
                {
                    if (NetworkServer.active)
                    {
                        sender.AddTimedBuff(_bdBanditSleep, _sleepDuration);
                        sender.RemoveBuff(_bdBanditTranquilizer);
                    }
                }

                // Bosses and champions have a higher sleep threshold
                if ((sender.isBoss || sender.isChampion) && buffCount >= _bossSleepCountThreshold)
                {
                    if (NetworkServer.active)
                    {
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
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
