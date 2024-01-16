using RoR2;
using RoR2.Items;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class FieldAccelerator : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("FieldAccelerator", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Amount of charge to add to the teleporter on kill. (1 = 100%)")]
        [TokenModifier("SS2_ITEM_FIELDACCELERATOR_DESC", StatTypes.MultiplyByN, 0, "100")]
        public static float chargePerKill = 0.01f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Maximum Amount of Charge per kill. (1 = 100%)")]
        public static float maxChargePerKill = 0.05f;

        public sealed class Behavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.FieldAccelerator;
            public void OnKilledOtherServer(DamageReport damageReport)
            {
                var teleInstance = TeleporterInteraction.instance;
                if (teleInstance)
                {
                    bool flag = damageReport.attackerBody == body;
                    bool flag1 = teleInstance.activationState == TeleporterInteraction.ActivationState.Charging;
                    bool flag2 = teleInstance.monstersCleared;
                    bool flag3 = teleInstance.holdoutZoneController.IsBodyInChargingRadius(damageReport.victimBody);
                    if (flag && flag1 && flag2 && flag3)
                        teleInstance.holdoutZoneController.charge += MSUtil.InverseHyperbolicScaling(chargePerKill, chargePerKill, maxChargePerKill, stack);
                    //Holy fuck since when has .InverseHyperbolicScaling existed???
                    //Since not too long ago, i stolñe it from komrade, lmao
                }
            }
        }
    }
}
