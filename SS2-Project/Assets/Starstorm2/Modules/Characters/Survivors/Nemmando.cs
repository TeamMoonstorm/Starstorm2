using RoR2;
using UnityEngine;
using RoR2.Skills;
using System.Runtime.CompilerServices;

namespace Moonstorm.Starstorm2.Survivors
{
    public sealed class Nemmando : SurvivorBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemmandoBody");
        public override GameObject MasterPrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemmandoMonsterMaster");
        public override SurvivorDef SurvivorDef { get; } = SS2Assets.LoadAsset<SurvivorDef>("SurvivorNemmando");
        GameObject footstepDust { get; set; } = Resources.Load<GameObject>("Prefabs/GenericFootstepDust");

        public override void Initialize()
        {
            base.Initialize();
            if (Starstorm.ScepterInstalled)
            {
                ScepterCompat();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void ScepterCompat()
        {
            //AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemmandoScepterSubmission"), "NemmandoBody", SkillSlot.Special, 0);
            //AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemmandoScepterBossAttack"), "NemmandoBody", SkillSlot.Special, 1);
            // sorry
        }

        public override void ModifyPrefab()
        {
            var cb = BodyPrefab.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/MageCrosshair");
            //cb.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = footstepDust;
        }
    }
}
