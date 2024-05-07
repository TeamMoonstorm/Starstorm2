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
    public sealed class NemHuntress : SS2Survivor, IContentPackModifier
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acNemHuntress", SS2Bundle.Indev);

        public static BodyIndex bodyIndex;
        public static GameObject crosshairPrefab;
        public static ModdedDamageType weakPointProjectile;
        public static GameObject miniCritPrefab;
        private GameObject arrowProjectile;
        private GameObject explosiveArrowProjectile;

        

        GameObject footstepDust { get; set; } = Resources.Load<GameObject>("Prefabs/GenericFootstepDust");

        public override void OnAssetCollectionLoaded(AssetCollection assetCollection)
        {
            miniCritPrefab = assetCollection.FindAsset<GameObject>("CritsparkMini");
            arrowProjectile = assetCollection.FindAsset<GameObject>("NemHuntressArrowProjectile");
            explosiveArrowProjectile = assetCollection.FindAsset<GameObject>("NemHuntressArrowExplode");
        }

        public override void Initialize()
        {
            CharacterBody body = CharacterPrefab.GetComponent<CharacterBody>();
            bodyIndex = body.bodyIndex;
            crosshairPrefab = body.defaultCrosshairPrefab;

            weakPointProjectile = DamageAPI.ReserveDamageType();
            On.RoR2.Projectile.ProjectileSingleTargetImpact.OnProjectileImpact += PSTI_OPI;

            ModifyProjectiles();
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        private void CrosshairManager_UpdateCrosshair(On.RoR2.UI.CrosshairManager.orig_UpdateCrosshair orig, RoR2.UI.CrosshairManager self, CharacterBody characterBody, Vector3 crosshairWorldPosition, Camera uiCamera)
        {
            //Use indices people :sob: -N
            if (characterBody.baseNameToken == "SS2_NEMHUNTRESS2_BODY_NAME" && characterBody.isSprinting)
            {
                self.currentCrosshairPrefab = characterBody.defaultCrosshairPrefab;
                return;
            }

            orig(self, characterBody, crosshairWorldPosition, uiCamera);
        }

        private void PSTI_OPI(On.RoR2.Projectile.ProjectileSingleTargetImpact.orig_OnProjectileImpact orig, ProjectileSingleTargetImpact self, ProjectileImpactInfo impactInfo)
        {
            //i tried checking this better but i give up!
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
            damageAPIComponent = explosiveArrowProjectile.AddComponent<ModdedDamageTypeHolderComponent>();
            damageAPIComponent.Add(weakPointProjectile);
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.projectilePrefabs.Add(new GameObject[]
            {
                arrowProjectile,
                explosiveArrowProjectile,
            });           
        }
    }
}
