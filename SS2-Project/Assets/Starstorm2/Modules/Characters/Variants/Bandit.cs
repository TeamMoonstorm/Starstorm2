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
using System.Linq;

namespace SS2.Survivors
{
    public class Bandit : SS2VanillaSurvivor
    {
        public override SS2AssetRequest<VanillaSurvivorAssetCollection> assetRequest => SS2Assets.LoadAssetAsync<VanillaSurvivorAssetCollection>("acBandit2", SS2Bundle.Indev);

        public static DamageAPI.ModdedDamageType TranqDamageType { get; set; }
        public static BuffDef bdBanditTranquilizer;
        public static BuffDef bdBanditSleep;

        public static GameObject tranqMuzzleFlashPrefab;
        public static GameObject tranqTracerEffectPrefab;
        public static GameObject tranqHitEffectPrefab;

        public static float tranqDuration = 5f;
        public static float _confuseSlowAmount = 0.2f;
        public static float _confuseAttackSpeedSlowAmount = 0.2f;
        public static float _maxDebuffAmount = 0.5f;
        public static float _sleepCountThreshold = 3;
        public static float _sleepDuration = 6;
        public static float _armorLoseAmount = 25;

        public override void Initialize()
        {
            SkillDef sdTranquilizerGun = assetCollection.FindAsset<SkillDef>("sdTranquilizerGun");
            Sprite texSleepIcon = assetCollection.FindAsset<Sprite>("texSleepIcon");
            Sprite texTranqIcon = assetCollection.FindAsset<Sprite>("texTranqDebuff");

            //bdBanditTranquilizer = CreateBuff("bdBanditTranquilizer", texTranqIcon, Color.white, true, true);
            //bdBanditSleep = CreateBuff("bdBanditSleep", texSleepIcon, Color.white, true, true);

            RegisterTranquilizer();
            R2API.RecalculateStatsAPI.GetStatCoefficients += ModifyStats;

            GameObject banditBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2Body.prefab").WaitForCompletion();
            tranqMuzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/MuzzleflashBandit2.prefab").WaitForCompletion();
            tranqTracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/TracerBandit2Rifle.prefab").WaitForCompletion();
            tranqHitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/HitsparkBandit.prefab").WaitForCompletion();

            SkillLocator skillLocator = banditBodyPrefab.GetComponent<SkillLocator>();
            SkillFamily skillFamilyPrimary = skillLocator.primary.skillFamily;

            AddSkill(skillFamilyPrimary, sdTranquilizerGun);

            Debug.Log("DEBUGGER Is texSleepIcon null?");
            Debug.Log(texSleepIcon);

            Debug.Log("DEBUGGER Is texTranqIcon null?");
            Debug.Log(texTranqIcon);
        }

        private void RegisterTranquilizer()
        {
            TranqDamageType = R2API.DamageAPI.ReserveDamageType();
            GlobalEventManager.onServerDamageDealt += ApplyTranq;
        }

        private void ModifyStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(SS2Content.Buffs.bdBandit2Tranq))
            {
                // TODO: Might need to do some sort of scaling, but this works for now.
                var buffCount = sender.GetBuffCount(SS2Content.Buffs.bdBandit2Tranq);
                args.moveSpeedMultAdd -= Math.Min(_confuseSlowAmount * buffCount, _maxDebuffAmount);
                args.attackSpeedMultAdd -= Math.Min(_confuseAttackSpeedSlowAmount * buffCount, _maxDebuffAmount);

                if (buffCount >= _sleepCountThreshold)
                {
                    if (NetworkServer.active)
                    {
                        // TODO: half sleep on bosses
                        sender.AddTimedBuff(SS2Content.Buffs.bdBandit2Sleep, _sleepDuration);
                        sender.RemoveBuff(SS2Content.Buffs.bdBandit2Tranq);
                    }
                }
            }

            if (sender.HasBuff((SS2Content.Buffs.bdBandit2Sleep)))
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
                victimBody.AddTimedBuffAuthority(SS2Content.Buffs.bdBandit2Tranq.buffIndex, tranqDuration);
            }
        }
        public override void ModifyContentPack(ContentPack contentPack)
        {
            // TODO: Add but I forget the [] syntax for C#, fuck this language and im tired
            //contentPack.buffDefs.Append(bdBanditTranquilizer);
            //contentPack.buffDefs.Append(bdBanditSleep);
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
