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
    public sealed class NemesisHuntress : SS2Survivor
    {
        public override SurvivorDef SurvivorDef => _survivorDef;
        private SurvivorDef _survivorDef;
        public override NullableRef<GameObject> MasterPrefab => null;

        public override GameObject CharacterPrefab => _characterPrefab;
        private GameObject _characterPrefab;

        public static BodyIndex bodyIndex;
        public static GameObject crosshairPrefab;
        public static ModdedDamageType weakPointProjectile;
        public static GameObject miniCritPrefab;

        GameObject footstepDust { get; set; } = Resources.Load<GameObject>("Prefabs/GenericFootstepDust");

        public override void Initialize()
        {
            bodyIndex = _characterPrefab.GetComponent<CharacterBody>().bodyIndex;
            crosshairPrefab = _characterPrefab.GetComponent<CharacterBody>().defaultCrosshairPrefab;

            weakPointProjectile = DamageAPI.ReserveDamageType();
            On.RoR2.Projectile.ProjectileSingleTargetImpact.OnProjectileImpact += PSTI_OPI;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            ParallelAssetLoadCoroutineHelper helper = new ParallelAssetLoadCoroutineHelper();
            helper.AddAssetToLoad<GameObject>("NemHuntress2Body", SS2Bundle.Indev);
            helper.AddAssetToLoad<SurvivorDef>("survivorNemHuntress2", SS2Bundle.Indev);
            helper.AddAssetToLoad<GameObject>("CritsparkMini", SS2Bundle.Indev);

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            _characterPrefab = helper.GetLoadedAsset<GameObject>("NemHuntress2Body");
            _survivorDef = helper.GetLoadedAsset<SurvivorDef>("survivorNemHuntress2");
            miniCritPrefab = helper.GetLoadedAsset<GameObject>("CritsparkMini");
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
    }
}
