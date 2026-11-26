using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;
using MSU;
using System.Collections.Generic;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using System;
using R2API;
using RoR2.Orbs;

namespace SS2.Items
{
    public sealed class Revolver : SS2Item, IContentPackModifier
    {
        private const string token = "SS2_ITEM_REVOLVER_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acRevolver", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Critical chance gained from the first stack of this item. (1 = 1%)")]
        [FormatToken(token, 0)]
        public static float critChance = 5;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Chance for the item to proc on crit. (1 = 1%)")]
        [FormatToken(token, 1)]
        public static float procChance = 20f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Damage coefficient of each revolver shot. (1 = 1%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float damageCoefficient = 0.5f;

        public static int minShots = 1;
        public static int maxShots = 6;
        public static float procCoefficient = 1f;
        public static int maxTurrets = 3;
        private static R2API.ModdedProcType revolverProcType;

        private static GameObject revolverTurretPrefab;
        private static Vector3 revolverSpawnOffset = new Vector3(-2.5f, 0.3f, 0f);
        public override void Initialize()
        {
            revolverProcType = R2API.ProcTypeAPI.ReserveProcType();

            revolverTurretPrefab = SS2Assets.LoadAsset<GameObject>("RevolverTurret", SS2Bundle.Items);

            R2API.RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
        }

        private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory && sender.inventory.GetItemCountEffective(SS2Content.Items.Revolver) > 0)
            {
                args.critAdd += critChance;
            }
        }

        private void OnServerDamageDealt(DamageReport damageReport)
        {
            var damageInfo = damageReport.damageInfo;
            var attackerBody = damageReport.attackerBody;
            var victimBody = damageReport.victimBody;

            if (!attackerBody || !attackerBody.inventory)
            {
                return;
            }

            if (damageInfo.crit && !damageInfo.procChainMask.HasModdedProc(revolverProcType))
            {
                int itemCount = attackerBody.inventory.GetItemCountEffective(SS2Content.Items.Revolver);
                if (itemCount > 0)
                {
                    if (RevolverTurretController.GetTurretCountForObject(attackerBody.gameObject) >= maxTurrets)
                    {
                        return;
                    }

                    float procChance = Revolver.procChance * damageInfo.procCoefficient;
                    if (Util.CheckRoll(procChance, damageReport.attackerMaster))
                    {
                        float damageValue = damageInfo.damage * (damageCoefficient * itemCount);
                        ProcChainMask procChainMask = damageInfo.procChainMask;
                        procChainMask.AddModdedProc(revolverProcType);

                        Vector3 forward = attackerBody.transform.forward;
                        if (attackerBody.characterDirection)
                        {
                            forward = attackerBody.characterDirection.forward;
                        }
                        Quaternion rotation = Util.QuaternionSafeLookRotation(forward);
                        Vector3 offset = rotation * revolverSpawnOffset;

                        RevolverTurretController controller = GameObject.Instantiate(revolverTurretPrefab, attackerBody.corePosition + offset, rotation).GetComponent<RevolverTurretController>();
                        controller.owner = attackerBody.gameObject;
                        controller.damageValue = damageValue;
                        controller.procCoefficient = procCoefficient;
                        controller.procChainMask = procChainMask;
                        controller.crit = true;

                        float luck = attackerBody.master ? attackerBody.master.luck : 0;
                        controller.shotCount = RollShotCount(luck, attackerBody.master);

                        NetworkServer.Spawn(controller.gameObject);
                    }
                }
            }
        }

        public static int RollShotCount(float luck, CharacterMaster effectOriginMaster = null)
        {
            // use luck to roll shot count
            int shotCount = UnityEngine.Random.Range(minShots, maxShots + 1);
            int initialShotCount = shotCount;

            int luckRolls = Mathf.CeilToInt(Mathf.Abs(luck));
            for (int i = 0; i < luckRolls; i++)
            {
                int rerolledShotCount = UnityEngine.Random.Range(minShots, maxShots + 1);
                if (luck > 0f) // take higher roll
                {
                    shotCount = Mathf.Max(shotCount, rerolledShotCount);
                }
                else // take lower roll
                {
                    shotCount = Mathf.Min(shotCount, rerolledShotCount);
                }
            }

            // if lucky
            if (shotCount > initialShotCount && effectOriginMaster)
            {
                GameObject bodyObject = effectOriginMaster.GetBodyObject();
                if (bodyObject)
                {
                    CharacterBody component = bodyObject.GetComponent<CharacterBody>();
                    if (component)
                    {
                        component.wasLucky = true;
                    }
                }
            }
            return shotCount;
        }
    }
}