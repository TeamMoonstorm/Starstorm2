using RoR2;
using UnityEngine;
using RoR2.Skills;
using System.Runtime.CompilerServices;
using RoR2.Projectile;
using System;

namespace Moonstorm.Starstorm2.Survivors
{
    [DisabledContent]
    public sealed class NemesisHuntress : SurvivorBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemHuntress2Body", SS2Bundle.Indev);
        public override GameObject MasterPrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemmandoMonsterMaster", SS2Bundle.Nemmando);
        public override SurvivorDef SurvivorDef { get; } = SS2Assets.LoadAsset<SurvivorDef>("survivorNemHuntress2", SS2Bundle.Indev);

        public static BodyIndex bodyIndex;
        public static GameObject crosshairPrefab; 

        GameObject footstepDust { get; set; } = Resources.Load<GameObject>("Prefabs/GenericFootstepDust");

        public override void Initialize()
        {
            base.Initialize();
            if (Starstorm.ScepterInstalled)
            {
                //ScepterCompat();
            }

            bodyIndex = BodyPrefab.GetComponent<CharacterBody>().bodyIndex;
            crosshairPrefab = BodyPrefab.GetComponent<CharacterBody>().defaultCrosshairPrefab;

            //On.RoR2.Projectile.ProjectileSingleTargetImpact.OnProjectileImpact += PSTI_OPI;
            //On.RoR2.UI.CrosshairManager.UpdateCrosshair += CrosshairManager_UpdateCrosshair;
        }

        private void CrosshairManager_UpdateCrosshair(On.RoR2.UI.CrosshairManager.orig_UpdateCrosshair orig, RoR2.UI.CrosshairManager self, CharacterBody characterBody, Vector3 crosshairWorldPosition, Camera uiCamera)
        {
            if (characterBody.baseNameToken == "SS2_NEMHUNTRESS2_BODY_NAME" && characterBody.isSprinting)
            {
                //Debug.Log("overriding crosshair");
                self.currentCrosshairPrefab = characterBody.defaultCrosshairPrefab;
                return;
            }

            orig(self, characterBody, crosshairWorldPosition, uiCamera);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void ScepterCompat()
        {
            //AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemmandoScepterSubmission"), "NemmandoBody", SkillSlot.Special, 0);
            //AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemmandoScepterBossAttack"), "NemmandoBody", SkillSlot.Special, 1);
        }

        public override void ModifyPrefab()
        {
            var cb = BodyPrefab.GetComponent<CharacterBody>();
            //cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/NemHunt");
            //cb.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = footstepDust;
        }
    }
}
