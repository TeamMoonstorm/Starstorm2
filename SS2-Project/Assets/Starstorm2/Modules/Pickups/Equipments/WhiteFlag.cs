using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS2.Equipments
{
    public sealed class WhiteFlag : SS2Equipment, IContentPackModifier
    {
        private const string token = "SS2_EQUIP_WHITEFLAG_DESC";

        public override SS2AssetRequest<EquipmentAssetCollection> AssetRequest<EquipmentAssetCollection>()
        {
            return SS2Assets.LoadAssetAsync<EquipmentAssetCollection>("acWhiteFlag", SS2Bundle.Equipments);
        }
        public override void OnAssetCollectionLoaded(AssetCollection assetCollection)
        {
            _flagObject = assetCollection.FindAsset<GameObject>("WhiteFlagWard");
            _overlay = assetCollection.FindAsset<Material>("matSurrenderOverlay");
        }

        private GameObject _flagObject;
        public static Material _overlay;// SS2Assets.LoadAsset<Material>("matSurrenderOverlay", SS2Bundle.Items);
        public static SkillDef disabledSkill;// SS2Assets.LoadAsset<SkillDef>("DisabledSkill", SS2Bundle.Items);


        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Radius of the White Flag's effect, in meters.")]
        [FormatToken(token, 0)]
        public static float flagRadius = 25f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Duration of White Flag when used, in seconds.")]
        [FormatToken(token, 1)]
        public static float flagDuration = 8f;

        public override bool Execute(EquipmentSlot slot)
        {            //To do: make better placement system
            GameObject gameObject = Object.Instantiate(_flagObject, slot.characterBody.corePosition, Quaternion.identity);
            BuffWard buffWard = gameObject.GetComponent<BuffWard>();
            buffWard.expireDuration = flagDuration;
            buffWard.radius = flagRadius;
            gameObject.GetComponent<TeamFilter>().teamIndex = slot.teamComponent.teamIndex;

            return true;
        }

        public override void Initialize()
        {
            BuffOverlays.AddBuffOverlay(SS2Content.Buffs.BuffSurrender, _overlay);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
        public override void OnEquipmentLost(CharacterBody CharacterBody)
        {
        }

        public override void OnEquipmentObtained(CharacterBody CharacterBody)
        {
        }

        public sealed class Behavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffSurrender;


            //captain is allowed to bomb mobs in the white flag zone because technically safe travels isnt in the zone :3
            private void OnEnable()
            {
                if (CharacterBody.skillLocator)
                {
                    GenericSkill primary = CharacterBody.skillLocator.primary;
                    if (primary && !(primary.skillDef is CaptainOrbitalSkillDef))// && !primary.skillDef.isCombatSkill)
                        primary.SetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    GenericSkill secondary = CharacterBody.skillLocator.secondary;
                    if (secondary && !(secondary.skillDef is CaptainOrbitalSkillDef))// && !secondary.skillDef.isCombatSkill)
                        secondary.SetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    GenericSkill utility = CharacterBody.skillLocator.utility;
                    if (utility && !(utility.skillDef is CaptainOrbitalSkillDef))// && !utility.skillDef.isCombatSkill)
                        utility.SetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    GenericSkill special = CharacterBody.skillLocator.special;
                    if (special && !(special.skillDef is CaptainOrbitalSkillDef))// && !special.skillDef.isCombatSkill)
                        special.SetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
                }
            }
            private void OnDisable()
            {
                if (CharacterBody.skillLocator)
                {
                    if (CharacterBody.skillLocator.primary)
                        CharacterBody.skillLocator.primary.UnsetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    if (CharacterBody.skillLocator.secondary)
                        CharacterBody.skillLocator.secondary.UnsetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    if (CharacterBody.skillLocator.utility)
                        CharacterBody.skillLocator.utility.UnsetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    if (CharacterBody.skillLocator.special)
                        CharacterBody.skillLocator.special.UnsetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
                }
            }
        }
    }
}
