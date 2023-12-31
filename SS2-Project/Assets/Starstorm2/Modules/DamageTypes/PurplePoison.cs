using Moonstorm;
using Moonstorm.Starstorm2.API;
using Moonstorm.Starstorm2.Buffs;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
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
            // if (NetworkServer.active)
            //{
                SS2DebugUtil.print("DEBUGGER ApplyPoison called!!");
                var victimBody = report.victimBody;
                var attackerBody = report.attackerBody;
                var damageInfo = report.damageInfo;
                var buildUpBuffCount = victimBody.GetBuffCount(SS2Content.Buffs.bdPoisonBuildup);

                if (DamageAPI.HasModdedDamageType(damageInfo, ModdedDamageType))
                {
                    var dotInfo = new InflictDotInfo()
                    {
                        attackerObject = attackerBody.gameObject,
                        victimObject = victimBody.gameObject,
                        dotIndex = Buffs.PurplePoisonDebuff.index,
                        duration = Duration + buildUpBuffCount,
                        damageMultiplier = 2,
                        maxStacksFromAttacker = 5,
                    };

                    DotController.InflictDot(ref dotInfo);
                }
           // }
        }
    }
}
