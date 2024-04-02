using RoR2;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Equipments
{
    public sealed class Magnet : SS2Equipment
    {
        private const string token = "SS2_EQUIP_MAGNET_DESC";
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("Magnet", SS2Bundle.Equipments);

        public static GameObject magnetPrefab = SS2Assets.LoadAsset<GameObject>("PickupMagnetController", SS2Bundle.Equipments);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Range at which Simple Magnet can pull pickups, in meters.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float magnetRadius = 150f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Percent chance for Simple Magnet to unearth treasure.")]
        [TokenModifier(token, StatTypes.MultiplyByN, 1, "100")]
        public static float treasureChance = .1f;

        public static float pullSpeed = 75f;

        public static float pullDuration = 3f;
        //distance away from player that pickups will land
        public static float destinationRadius = 8f;

        public override bool FireAction(EquipmentSlot slot)
        {
            NetworkServer.Spawn(GameObject.Instantiate<GameObject>(magnetPrefab, slot.characterBody.corePosition + (Vector3.up * 3f), Quaternion.identity));
            return true;
        }
    }
}