using Moonstorm.Components;
using RoR2;
using UnityEngine.Networking;
using UnityEngine;
using RoR2.Projectile;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class AffixPurple : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdElitePurple", SS2Bundle.Indev);

        public sealed class Behavior : BaseBuffBodyBehavior, IOnDamageDealtServerReceiver
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdElitePurple;

            private float prevInterval = 0.0f;

            private static float projectileLaunchInterval = 10f;
            
            // Fields for launching projectiles
            private int radius = 2;
            private float damage;

            public void OnDamageDealtServer(DamageReport damageReport)
            {
                if (NetworkServer.active)
                {
                    var victim = damageReport.victim;
                    var attacker = damageReport.attacker;

                    if (victim.gameObject != attacker && damageReport.damageInfo.procCoefficient > 0)
                    {
                        victim.body.AddTimedBuffAuthority(SS2Content.Buffs.bdPoisonBuildup.buffIndex, 10f);
                    }

                    if (victim.gameObject != attacker && victim.body.GetBuffCount(SS2Content.Buffs.bdPoisonBuildup) >= 3)
                    {
                        victim.body.AddTimedBuffAuthority(SS2Content.Buffs.bdPurplePoison.buffIndex, 10f);
                    }
                }
            }

            private void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    prevInterval += Time.fixedDeltaTime;

                    if (prevInterval >= projectileLaunchInterval)
                    {
                        int num = 3 + radius;
                        prevInterval = 0f;
                        float num2 = 360f / num;
                        Vector3 val = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
                        Vector3 normalized = val.normalized;
                        Vector3 val2 = Vector3.RotateTowards(Vector3.up, normalized, 0.43633232f, float.PositiveInfinity);
                        for (int i = 0; i < num; i++)
                        {
                            Vector3 forward = Quaternion.AngleAxis(num2 * i, Vector3.up) * val2;
                            ProjectileManager.instance.FireProjectile(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/PoisonOrbProjectile"), this.body.corePosition, Util.QuaternionSafeLookRotation(forward), this.gameObject, damage * 1f, 0f, Util.CheckRoll(this.body.crit, this.body.master));
                        }
                    }
                }
            }
        }
    }
}
