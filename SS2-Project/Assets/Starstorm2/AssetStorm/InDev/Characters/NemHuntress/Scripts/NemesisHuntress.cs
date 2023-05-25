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

        GameObject footstepDust { get; set; } = Resources.Load<GameObject>("Prefabs/GenericFootstepDust");

        public override void Initialize()
        {
            base.Initialize();
            if (Starstorm.ScepterInstalled)
            {
                //ScepterCompat();
            }

            On.RoR2.Projectile.ProjectileSingleTargetImpact.OnProjectileImpact += PSTI_OPI;
        }

        private void PSTI_OPI(On.RoR2.Projectile.ProjectileSingleTargetImpact.orig_OnProjectileImpact orig, RoR2.Projectile.ProjectileSingleTargetImpact self, ProjectileImpactInfo impactInfo)
        {
            //Debug.Log("Processing Nemesis Huntress crit.");
            if (self.projectileDamage.damageType == DamageType.WeakPointHit)
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
                            //Debug.Log("Sniper target found + attempting to alter damage to crit.");
                            self.projectileDamage.crit = true;
                        }
                    }
                }
            }

            orig(self, impactInfo);
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
