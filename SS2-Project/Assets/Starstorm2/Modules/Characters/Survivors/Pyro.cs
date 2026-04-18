using SS2.Components;
using RoR2;
using System;
using UnityEngine;
using MSU;
using R2API;
using RoR2.ContentManagement;
using static R2API.DamageAPI;
using RoR2.Projectile;
using UnityEngine.Networking;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace SS2.Survivors
{
    public sealed class Pyro : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acPyro", SS2Bundle.Indev);
        public static ModdedDamageType PyroIgniteOnHit { get; private set; }

        public static GameObject _pyroBody;
        public static GameObject _hotFireVFX;
        public static BuffDef _bdPyroManiac;

        public static float passiveHeatPerSecond = 1.7f;

        private BodyIndex pyroIndex;
        public override bool IsAvailable(ContentPack contentPack)
        {
            return SS2Config.enableBeta && base.IsAvailable(contentPack);
        }
        public override void Initialize()
        {
            ModifyPrefab();

            PyroIgniteOnHit = ReserveDamageType();

            _pyroBody = AssetCollection.FindAsset<GameObject>("PyroBody");

            _hotFireVFX = AssetCollection.FindAsset<GameObject>("PyroHotFireVFX");
            _bdPyroManiac = AssetCollection.FindAsset<BuffDef>("bdPyroManiac");

            IL.RoR2.DotController.EvaluateDotStacksForType += EvaluateDotStacksForType;
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
            // ss2content characterbodies NEVER!!!!!!!!!!!!
            BodyCatalog.availability.onAvailable += () => pyroIndex = BodyCatalog.FindBodyIndex(_pyroBody);
        }


        private static float pyroBurnDuration = 4f;
        
        const float igniteDamageCoefficient = 0.5f; // value from vanilla igniteonhit
        
        private void OnServerDamageDealt(DamageReport damageReport)
        {
            if (damageReport.damageInfo.HasModdedDamageType(PyroIgniteOnHit))
            {
                float targetTotalDamage = damageReport.damageInfo.damage * igniteDamageCoefficient;
                float damageMultiplier = GetDotDamageMultiplier(damageReport.attackerBody, targetTotalDamage, pyroBurnDuration, DotController.DotIndex.Burn);
                var dotInfo = new InflictDotInfo()
                {
                    attackerObject = damageReport.attacker,
                    victimObject = damageReport.victim.gameObject,
                    dotIndex = DotController.DotIndex.Burn,
                    duration = pyroBurnDuration,
                    damageMultiplier = damageMultiplier,
                };
                if (damageReport.attackerBody)
                {
                    StrengthenBurnUtils.CheckDotForUpgrade(damageReport.attackerBody.inventory, ref dotInfo);
                }
                DotController.InflictDot(ref dotInfo);
            }
        }

        // if you want a DoT to deal a specific amount of damage over a set duration, this method will return the necessary value for InflictDotInfo.damageMultiplier
        // using InflictDotInfo.totalDamage would make the DoT have a variable duration with a static damage-per-tick
        public static float GetDotDamageMultiplier(CharacterBody attackerBody, float desiredTotalDamage, float desiredDuration, DotController.DotIndex dotIndex)
        {
            var dotDef = DotController.GetDotDef(dotIndex);
            return (desiredTotalDamage * dotDef.interval) / (desiredDuration * dotDef.damageCoefficient) / attackerBody.damage;
        }

        public void ModifyPrefab()
        {
            var cb = CharacterPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
        }

        // Set burn DOT damage to 0 if target is Pyro
        private void EvaluateDotStacksForType(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);

            // Locate the following line and modify the damage
            // damageInfo.damage = list[i].totalDamage
            // We allow for gaps between the ldfld (totalDamage) and stfld (damageInfo)
            // in case someone has injected their own instructions. Exactly like we're doing!
            bool b = c.TryGotoNext(
                x => x.MatchLdfld<DotController.PendingDamage>(nameof(DotController.PendingDamage.totalDamage))
            ) && c.TryGotoNext(
                x => x.MatchStfld<DamageInfo>(nameof(DamageInfo.damage))
            );
            if (b)
            {
                c.Emit(OpCodes.Ldarg_0); // dotcontroller
                c.Emit(OpCodes.Ldarg_1); // dotindex
                c.EmitDelegate<Func<float, DotController, DotController.DotIndex, float>>((dmg, dc, dot) =>
                {
                    if (dc.victimBody.bodyIndex == pyroIndex && PyroUtil.IsFireDot(dot))
                    {
                        dmg = 0f;
                    }
                    return dmg;
                });
            }
            else
            {
                SS2Log.Fatal("Pyro.EvaluateDotStacksForType: ILHook failed.");
            }
        }
    }
}
namespace SS2
{
    // one source of truth for what is or isnt fire
    public static class PyroUtil
    {
        public static int GetBurnCountPyro(this CharacterBody body)
        {
            return body.GetBuffCount(RoR2Content.Buffs.OnFire) + body.GetBuffCount(DLC1Content.Buffs.StrongerBurn);
        }
        public static bool IsOnFirePyro(this CharacterBody body)
        {
            return body.GetBurnCountPyro() > 0;
        }
        public static bool IsFireDot(DotController.DotIndex dotIndex)
        {
            return dotIndex == DotController.DotIndex.Burn || dotIndex == DotController.DotIndex.PercentBurn || dotIndex == DotController.DotIndex.StrongerBurn;
        }
        public static bool IsFireDamageType(DamageTypeCombo damageType)
        {
            return damageType.HasDamageType(DamageType.IgniteOnHit) || damageType.HasDamageType(DamageType.PercentIgniteOnHit) || damageType.HasDamageType(DamageTypeExtended.FireNoIgnite) || damageType.HasModdedDamageType(Survivors.Pyro.PyroIgniteOnHit);
        }
        private static bool HasDamageType(this DamageTypeCombo damageType, DamageType damageTypeToCheck)
        {
            return (damageType.damageType & damageTypeToCheck) > DamageType.Generic;
        }
        private static bool HasDamageType(this DamageTypeCombo damageType, DamageTypeExtended damageTypeToCheck)
        {
            return (damageType.damageTypeExtended & damageTypeToCheck) > DamageTypeExtended.Generic;
        }
    }
}