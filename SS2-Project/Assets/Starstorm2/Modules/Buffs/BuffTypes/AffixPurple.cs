using RoR2;
using UnityEngine.Networking;
using UnityEngine;
using RoR2.Projectile;
namespace SS2.Buffs
{
    //TODO: move this to the equipment class.
    /*
    public sealed class AffixPurple : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdElitePurple", SS2Bundle.Indev);

        public sealed class Behavior : BaseBuffBodyBehavior, IOnDamageDealtServerReceiver
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
                if (NetworkServer.active)
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
                            ProjectileManager.instance.FireProjectile(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/PoisonOrbProjectile"), this.body.corePosition, Util.QuaternionSafeLookRotation(forward), this.gameObject, damage * 1f, 0f, Util.CheckRoll(this.body.crit, this.body.master));
                        }
                    }
                }
            }
        }
    }*/
}
