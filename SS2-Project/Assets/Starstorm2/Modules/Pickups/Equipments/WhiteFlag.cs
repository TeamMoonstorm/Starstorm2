using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Equipments
{
    public sealed class WhiteFlag : SS2Equipment, IContentPackModifier
    {
        private const string token = "SS2_EQUIP_WHITEFLAG_DESC";

        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<EquipmentAssetCollection>("acWhiteFlag", SS2Bundle.Equipments);

        private GameObject _flagObject;
        public static SkillDef disabledSkill;// SS2Assets.LoadAsset<SkillDef>("DisabledSkill", SS2Bundle.Items);


        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Radius of the White Flag's effect, in meters.")]
        [FormatToken(token, 0)]
        public static float flagRadius = 25f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Duration of White Flag when used, in seconds.")]
        [FormatToken(token, 1)]
        public static float flagDuration = 15f;

        public override bool Execute(EquipmentSlot slot)
        {            
            GameObject gameObject = Object.Instantiate(_flagObject, slot.characterBody.corePosition, Quaternion.identity);
            BuffWard buffWard = gameObject.GetComponent<BuffWard>();
            buffWard.expireDuration = flagDuration;
            buffWard.radius = flagRadius;
            gameObject.GetComponent<TeamFilter>().teamIndex = slot.teamComponent.teamIndex;
            NetworkServer.Spawn(gameObject);
            return true;
        }

        public override void Initialize()
        {
            disabledSkill = SS2Assets.LoadAsset<SkillDef>("DisabledSkill", SS2Bundle.Base);
            BuffDef buffSurrender = AssetCollection.FindAsset<BuffDef>("BuffSurrender");
            _flagObject = AssetCollection.FindAsset<GameObject>("WhiteFlagWard");
            Material overlay = AssetCollection.FindAsset<Material>("matSurrenderOverlay");
            BuffOverlays.AddBuffOverlay(buffSurrender, overlay);
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
                if (characterBody.skillLocator)
                {
                    GenericSkill primary = characterBody.skillLocator.primary;
                    if (primary && !(primary.skillDef is CaptainOrbitalSkillDef))// && !primary.skillDef.isCombatSkill)
                        primary.SetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    GenericSkill secondary = characterBody.skillLocator.secondary;
                    if (secondary && !(secondary.skillDef is CaptainOrbitalSkillDef))// && !secondary.skillDef.isCombatSkill)
                        secondary.SetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    GenericSkill utility = characterBody.skillLocator.utility;
                    if (utility && !(utility.skillDef is CaptainOrbitalSkillDef))// && !utility.skillDef.isCombatSkill)
                        utility.SetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    GenericSkill special = characterBody.skillLocator.special;
                    if (special && !(special.skillDef is CaptainOrbitalSkillDef))// && !special.skillDef.isCombatSkill)
                        special.SetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
                }
            }
            private void OnDisable()
            {
                if (characterBody.skillLocator)
                {
                    if (characterBody.skillLocator.primary)
                        characterBody.skillLocator.primary.UnsetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    if (characterBody.skillLocator.secondary)
                        characterBody.skillLocator.secondary.UnsetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    if (characterBody.skillLocator.utility)
                        characterBody.skillLocator.utility.UnsetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    if (characterBody.skillLocator.special)
                        characterBody.skillLocator.special.UnsetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
                }
            }
        }
    }
}
