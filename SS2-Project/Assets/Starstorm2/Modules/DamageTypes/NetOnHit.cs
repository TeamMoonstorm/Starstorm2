using R2API;
using RoR2;
using UnityEngine;
using static R2API.DamageAPI;

namespace Moonstorm.Starstorm2.DamageTypes
{
    public sealed class NetOnHit : DamageTypeBase
    {
        public override ModdedDamageType ModdedDamageType { get; protected set; }

        public static float duration = 5f;

        public static ModdedDamageType netDamageType;

        public override void Initialize()
        {
            netDamageType = ModdedDamageType;
        }

        
        public override void Delegates()
        {
            GlobalEventManager.onServerDamageDealt += ApplyNet;
        }

        private void ApplyNet(DamageReport report)
        {
            var victimBody = report.victimBody;
            var damageInfo = report.damageInfo;
            if (DamageAPI.HasModdedDamageType(damageInfo, ModdedDamageType))
            {
                victimBody.AddTimedBuffAuthority(SS2Content.Buffs.bdMULENet.buffIndex, duration);
            }
        }
    }
}
