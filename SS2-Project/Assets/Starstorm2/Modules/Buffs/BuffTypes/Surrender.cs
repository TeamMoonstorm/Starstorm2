using RoR2;
using RoR2.Skills;
using UnityEngine;
namespace SS2.Buffs
{
    public sealed class Surrender : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffSurrender", SS2Bundle.Items);

        public override Material OverlayMaterial => SS2Assets.LoadAsset<Material>("matSurrenderOverlay", SS2Bundle.Items);

        public static SkillDef disabledSkill = SS2Assets.LoadAsset<SkillDef>("DisabledSkill", SS2Bundle.Items);
        public sealed class Behavior : BaseBuffBodyBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffSurrender;


            //captain is allowed to bomb mobs in the white flag zone because technically safe travels isnt in the zone :3
            private void OnEnable()
            {
                if(body.skillLocator)
                {
                    GenericSkill primary = body.skillLocator.primary;
                    if (primary && !(primary.skillDef is CaptainOrbitalSkillDef))// && !primary.skillDef.isCombatSkill)
                        primary.SetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    GenericSkill secondary = body.skillLocator.secondary;
                    if (secondary && !(secondary.skillDef is CaptainOrbitalSkillDef))// && !secondary.skillDef.isCombatSkill)
                        secondary.SetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    GenericSkill utility = body.skillLocator.utility;
                    if (utility && !(utility.skillDef is CaptainOrbitalSkillDef))// && !utility.skillDef.isCombatSkill)
                        utility.SetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    GenericSkill special = body.skillLocator.special;
                    if (special && !(special.skillDef is CaptainOrbitalSkillDef))// && !special.skillDef.isCombatSkill)
                        special.SetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
                }
            }
            private void OnDisable()
            {
                if (body.skillLocator)
                {
                    if (body.skillLocator.primary)
                        body.skillLocator.primary.UnsetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    if (body.skillLocator.secondary)
                        body.skillLocator.secondary.UnsetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    if (body.skillLocator.utility)
                        body.skillLocator.utility.UnsetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    if (body.skillLocator.special)
                        body.skillLocator.special.UnsetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
                }
            }
        }

    }
}
