using R2API;
using RoR2;
using UnityEngine;
using static R2API.DamageAPI;

namespace Moonstorm.Starstorm2.DamageTypes
{
    public sealed class Gouge : DamageTypeBase
    {
        public override ModdedDamageType ModdedDamageType { get; protected set; }

        public static DotController.DotIndex DotIndex;

        public static float Duration = 2;

        public static ModdedDamageType gougeDamageType;

        public override void Initialize()
        {
            gougeDamageType = ModdedDamageType;
            DotIndex = Buffs.Gouge.index;
        }

        
        public override void Delegates()
        {
            GlobalEventManager.onServerDamageDealt += ApplyGouge;
        }

        private void ApplyGouge(DamageReport report)
        {
            var victimBody = report.victimBody;
            var attackerBody = report.attackerBody;
            var damageInfo = report.damageInfo;
            if (DamageAPI.HasModdedDamageType(damageInfo, ModdedDamageType))
            {
                //deprecated by reimplementation in buff
                //var gougeDamage = (damageInfo.crit ? 2f : 1f) * 2.1f;
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
