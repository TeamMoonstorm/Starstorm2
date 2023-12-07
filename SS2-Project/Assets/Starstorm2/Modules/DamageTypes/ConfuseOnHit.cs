using R2API;
using RoR2;
using UnityEngine;
using static R2API.DamageAPI;

namespace Moonstorm.Starstorm2.DamageTypes
{
    public sealed class ConfuseOnHit : DamageTypeBase
    {
        public override ModdedDamageType ModdedDamageType { get; protected set; }

        public static float duration = 4f;

        public static ModdedDamageType confuseDamageType;

        public override void Initialize()
        {
            confuseDamageType = ModdedDamageType;
        }
        public override void Delegates()
        {
            GlobalEventManager.onServerDamageDealt += ApplyConfusion;
        }
        private void ApplyConfusion(DamageReport report)
        {
            var victimBody = report.victimBody;
            var damageInfo = report.damageInfo;
            if (DamageAPI.HasModdedDamageType(damageInfo, ModdedDamageType))
            {
                victimBody.AddTimedBuffAuthority(SS2Content.Buffs.BuffChirrConfuse.buffIndex, duration);
            }
        }
    }
}
