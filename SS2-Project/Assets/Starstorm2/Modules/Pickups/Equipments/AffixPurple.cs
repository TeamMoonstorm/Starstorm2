using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static MSU.BaseBuffBehaviour;

namespace SS2.Equipments
{
#if DEBUG
    public class AffixPurple : SS2EliteEquipment
    {
        public static DotController.DotIndex index;
        public override SS2AssetRequest<EliteAssetCollection> AssetRequest()
        {
            return SS2Assets.LoadAssetAsync<EliteAssetCollection>("acAffixPurple", SS2Bundle.Equipments);
        }
        public override void Initialize()
        {
            // TODO: Do I need a separate dot for poisondebuff buffdef? idk
            index = DotAPI.RegisterDotDef(0.25f, 0.18f, DamageColorIndex.DeathMark, SS2Content.Buffs.bdPoisonBuildup);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            yield break;
        }

        public override bool Execute(EquipmentSlot slot)
        {
            return false;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        // TODO: Test if we actually need this
        //public class PurplePoisonBuildup : BaseBuffBehaviour
        //{
        //    [BuffDefAssociation]
        //    private static BuffDef GetBuffDef() => SS2Content.Buffs.bdPoisonBuildup;
        //}

        //public class PurplePoisonDebuff : BaseBuffBehaviour
        //{
        //    [BuffDefAssociation]
        //    private static BuffDef GetBuffDef() => SS2Content.Buffs.bdPurplePoison;
        //}

        public sealed class AffixPurpleBehavior : BaseBuffBehaviour, IOnDamageDealtServerReceiver
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdElitePurple;

            private float prevInterval = 0.0f;

            private static float projectileLaunchInterval = 30f;

            // Fields for launching projectiles
            private int radius = 4;
            private static int numProjectiles = 2;
            private float damage;
            private static int duration = 5;

            public void OnDamageDealtServer(DamageReport damageReport)
            {
                if (HasAnyStacks && NetworkServer.active)
                {
                    var victim = damageReport.victim;
                    var attacker = damageReport.attacker;
                    var victimBody = damageReport.victimBody;
                    var attackerBody = damageReport.attackerBody;
                    var buildUpBuffCount = victimBody.GetBuffCount(SS2Content.Buffs.bdPoisonBuildup);

                    // Add stacks of buildup
                    if (victim.gameObject != attacker && damageReport.damageInfo.procCoefficient > 0)
                    {
                        victim.body.AddTimedBuffAuthority(SS2Content.Buffs.bdPoisonBuildup.buffIndex, 9f);

                        // increase buildup buff count since the victim just got one inflicted
                        // and doing this would be a cheaper call then GetBuffCount
                        buildUpBuffCount += 1;
                    }

                    if (victim.gameObject != attacker && buildUpBuffCount >= 3)
                    {
                        // We remove at least one instance of buiuldup to balance things a bit
                        victimBody.RemoveBuff(SS2Content.Buffs.bdPoisonBuildup.buffIndex);

                        // Duration will depend on how many buildups the victim has accured
                        var dotDuration = duration + (buildUpBuffCount - 1);

                        // Apply DOT!
                        var dotInfo = new InflictDotInfo()
                        {
                            attackerObject = attackerBody.gameObject,
                            victimObject = victimBody.gameObject,
                            dotIndex = DotController.DotIndex.Poison,
                            duration = dotDuration,
                            damageMultiplier = 1,
                            maxStacksFromAttacker = 5,
                        };
                        DotController.InflictDot(ref dotInfo);

                        // Add a poison buff just to reflect the length of the poison dot in the UI
                        victim.body.AddTimedBuffAuthority(SS2Content.Buffs.bdPurplePoison.buffIndex, dotDuration);
                    }
                }
            }

            private void FixedUpdate()
            {
                // HasAnyStacks isnt needed here because epheremal nature of buffs in MSU
                // means FixedUpdate isnt called if the buff isnt active
                if (NetworkServer.active)
                {
                    prevInterval += Time.fixedDeltaTime;

                    // Periodically launch projectiles that create poison AOE fields
                    if (prevInterval >= projectileLaunchInterval)
                    {
                        prevInterval = 0f;
                        float num2 = 360f / numProjectiles;
                        Vector3 val = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
                        Vector3 normalized = val.normalized;
                        Vector3 val2 = Vector3.RotateTowards(Vector3.up, normalized, 0.43633232f, float.PositiveInfinity);
                        for (int i = 0; i < numProjectiles; i++)
                        {
                            Vector3 forward = Quaternion.AngleAxis(num2 * i, Vector3.up) * val2;
                            ProjectileManager.instance.FireProjectile(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/PoisonOrbProjectile"), this.CharacterBody.corePosition, Util.QuaternionSafeLookRotation(forward), this.gameObject, damage * 1f, 0f, Util.CheckRoll(this.CharacterBody.crit, this.CharacterBody.master));
                        }
                    }
                }
            }
        }
    }
#endif
}
