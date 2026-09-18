using RoR2;
using UnityEngine;
using System.Runtime.CompilerServices;
using MSU;
using System.Collections;
using static R2API.DamageAPI;
using R2API;
using RoR2.Projectile;
using RoR2.ContentManagement;

namespace SS2.Survivors
{
#if DEBUG
    public class NemHuntress : SS2Survivor, IContentPackModifier
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acNemHuntress", SS2Bundle.Indev);

        public static BodyIndex bodyIndex;
        public static GameObject crosshairPrefab;
        public static ModdedDamageType weakPointProjectile;
        public static GameObject miniCritPrefab;
        private GameObject arrowProjectile;
        private GameObject electricArrowProjectile;

        GameObject footstepDust { get; set; } = Resources.Load<GameObject>("Prefabs/GenericFootstepDust");


        public override void Initialize()
        {
            miniCritPrefab = AssetCollection.FindAsset<GameObject>("CritsparkMini");
            arrowProjectile = AssetCollection.FindAsset<GameObject>("NemHuntressArrowProjectile");
            electricArrowProjectile = AssetCollection.FindAsset<GameObject>("NemHuntressElectricArrowProjectile");

            CharacterBody body = CharacterPrefab.GetComponent<CharacterBody>();
            bodyIndex = body.bodyIndex;
            crosshairPrefab = body.defaultCrosshairPrefab;
            body.useSprintCrosshair = false;

            weakPointProjectile = DamageAPI.ReserveDamageType();
            On.RoR2.Projectile.ProjectileSingleTargetImpact.OnProjectileImpact += PSTI_OPI;

            ModifyProjectiles();
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        private void PSTI_OPI(On.RoR2.Projectile.ProjectileSingleTargetImpact.orig_OnProjectileImpact orig, ProjectileSingleTargetImpact self, ProjectileImpactInfo impactInfo)
        {
            //i tried checking this better but i give up!
            // TO-DO: past me we can do better. wtf is this
            // also should this be on authority? need to investigate if PSTI is authority (probably) or server or wat
            if (self.projectileController.gameObject.name.Contains("NemHuntress"))
            {
                Collider collider = impactInfo.collider;
                if (collider)
                {
                    HurtBox component = collider.GetComponent<HurtBox>();
                    if (component && component.hurtBoxGroup)
                    {
                        if (component.isSniperTarget)
                        {
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

        private void ModifyProjectiles()
        {
            var damageAPIComponent = arrowProjectile.AddComponent<ModdedDamageTypeHolderComponent>();
            damageAPIComponent.Add(weakPointProjectile);
            damageAPIComponent = electricArrowProjectile.AddComponent<ModdedDamageTypeHolderComponent>();
            damageAPIComponent.Add(weakPointProjectile);
        }

        // why

        //public override void ModifyContentPack(ContentPack contentPack)
        //{
        //    contentPack.projectilePrefabs.Add(new GameObject[]
        //    {
        //        arrowProjectile,
        //        explosiveArrowProjectile,
        //    });
        //}
    }
#endif
}
