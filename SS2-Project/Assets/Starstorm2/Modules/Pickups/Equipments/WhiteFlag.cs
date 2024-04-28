using RoR2;
using RoR2.Audio;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Equipments
{
    public sealed class WhiteFlag : EquipmentBase
    {
        private const string token = "SS2_EQUIP_WHITEFLAG_DESC";
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("WhiteFlag", SS2Bundle.Equipments);
        public GameObject FlagObject { get; } = SS2Assets.LoadAsset<GameObject>("WhiteFlagWard", SS2Bundle.Equipments);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Radius of the White Flag's effect, in meters.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float flagRadius = 25f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Duration of White Flag when used, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float flagDuration = 8f;

        public override bool FireAction(EquipmentSlot slot)
        {
            //To do: make better placement system
            GameObject gameObject = Object.Instantiate(FlagObject, slot.characterBody.corePosition, Quaternion.identity);
            BuffWard buffWard = gameObject.GetComponent<BuffWard>();
            buffWard.expireDuration = flagDuration;
            buffWard.radius = flagRadius;
            gameObject.GetComponent<TeamFilter>().teamIndex = slot.teamComponent.teamIndex;

            NetworkServer.Spawn(gameObject);
            return true;
        }
    }

}
