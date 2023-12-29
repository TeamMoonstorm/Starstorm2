using Moonstorm;
using R2API;
using RoR2;
using UnityEngine;
using static R2API.DamageAPI;

namespace Moonstorm.Starstorm2.DamageTypes
{
    public class PurplePoison : DamageTypeBase
    {
        public override ModdedDamageType ModdedDamageType { get; protected set; }

        public static DotController.DotIndex DotIndex;

        public static float Duration = 2;

        public static ModdedDamageType poisonDamageType;

        public override void Initialize()
        {
            poisonDamageType = ModdedDamageType;
            DotIndex = Buffs.PurplePoisonDebuff.index;
        }

        public override void Delegates()
        {
            GlobalEventManager.onServerDamageDealt += ApplyPoison;
        }

        private void ApplyPoison(DamageReport report)
        {
            var victimBody = report.victimBody;
            var attackerBody = report.attackerBody;
            var damageInfo = report.damageInfo;
            if (DamageAPI.HasModdedDamageType(damageInfo, ModdedDamageType))
            {
                var dotInfo = new InflictDotInfo()
                {
                    attackerObject = attackerBody.gameObject,
                    victimObject = victimBody.gameObject,
                    dotIndex = Buffs.Gouge.index,
                    duration = 2,
                    damageMultiplier = 1,
                };
                DotController.InflictDot(ref dotInfo);
            }
        }
    }
}
