using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using static R2API.DamageAPI;

namespace Moonstorm.Starstorm2.DamageTypes
{
    public sealed class WeakPointProjectile : DamageTypeBase
    {
        public override DamageAPI.ModdedDamageType ModdedDamageType { get; protected set; }

        public static ModdedDamageType weakPointProjectile;
        public static GameObject miniCritPrefab = SS2Assets.LoadAsset<GameObject>("CritsparkMini", SS2Bundle.Indev);

        public override void Initialize()
        {
            weakPointProjectile = ModdedDamageType;
        }

        public override void Delegates()
        {
            On.RoR2.Projectile.ProjectileSingleTargetImpact.OnProjectileImpact += PSTI_OPI;
        }


        private void PSTI_OPI(On.RoR2.Projectile.ProjectileSingleTargetImpact.orig_OnProjectileImpact orig, ProjectileSingleTargetImpact self, ProjectileImpactInfo impactInfo)
        {
            //Debug.Log("Processing Nemesis Huntress crit.");
            //Debug.Log(self.projectileDamage.damageType + " : damagetype");

            //i tried checking this better but i give up!
            if (self.projectileController.gameObject.name.Contains("NemHuntress"))
            {
                //Debug.Log("Arrow is WeakPointHit");
                Collider collider = impactInfo.collider;
                if (collider)
                {
                    //Debug.Log("Collider found");
                    HurtBox component = collider.GetComponent<HurtBox>();
                    if (component && component.hurtBoxGroup)
                    {
                        //Debug.Log("Hurtbox found");
                        if (component.isSniperTarget)
                        {
                            //Debug.Log("Sniper target found + attempting to alter damage to minicrit.");
                            //self.projectileDamage.crit = true;
                            self.projectileDamage.damage *= 1.5f;
                            self.projectileDamage.damageColorIndex = DamageColorIndex.WeakPoint;
                            EffectManager.SimpleImpactEffect(miniCritPrefab, impactInfo.estimatedPointOfImpact, impactInfo.estimatedImpactNormal, true);
                            self.hitSoundString = "NemHuntressHeadshot";

                            if (self.projectileController.gameObject.GetComponent<ProjectileExplosion>() != null)
                            {
                                ProjectileExplosion pe = self.projectileController.gameObject.GetComponent<ProjectileExplosion>();
                                pe.Detonate();
                            }
                        }
                    }
                }
            }

            orig(self, impactInfo);
        }
    }
}
