using Mono.Cecil.Cil;
using MonoMod.Cil;
using SS2.Components;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using MSU;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using R2API;
using RoR2.ContentManagement;
using static R2API.DamageAPI;

namespace SS2.Survivors
{
    public sealed class Pyro : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acPyro", SS2Bundle.Indev);
        public static ModdedDamageType FlamethrowerDamageType { get; private set; }
        public static ModdedDamageType FireballDamageType { get; private set; }


        public override void Initialize()
        {
            ModifyPrefab();

            FlamethrowerDamageType = ReserveDamageType();
            FireballDamageType = ReserveDamageType();

            GlobalEventManager.onServerDamageDealt += PyroDamageChecks;
        }


        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        private void PyroDamageChecks(DamageReport report)
        {
            var victimBody = report.victimBody;
            var attackerBody = report.attackerBody;
            var damageInfo = report.damageInfo;

            if (DamageAPI.HasModdedDamageType(damageInfo, FlamethrowerDamageType))
            {
                PyroController pc = attackerBody.GetComponent<PyroController>();
                if (pc != null)
                {
                    float distance = Vector3.Distance(victimBody.corePosition, attackerBody.corePosition);

                    if (distance > 17.5f)
                    {
                        Debug.Log("pre damage: " + damageInfo.damage);
                        damageInfo.damage *= 1.5f;
                        Debug.Log("post damage: " + damageInfo.damage);
                        damageInfo.damageColorIndex = DamageColorIndex.WeakPoint;
                        EffectManager.SimpleEffect(SS2Assets.LoadAsset<GameObject>("HotFireVFX", SS2Bundle.Indev), victimBody.transform.position, Quaternion.identity, true);
                        if (Util.CheckRoll(50f, attackerBody.master) && pc.heat >= 30f)
                            damageInfo.damageType = DamageType.IgniteOnHit;
                    }
                }
            }
        }


        public void ModifyPrefab()
        {
            var cb = CharacterPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
        }

    }
}