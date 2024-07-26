using R2API;
using RoR2;

namespace Moonstorm.Starstorm2.DamageTypes
{
    public class Nuclear : DamageTypeBase
    {
        public override DamageAPI.ModdedDamageType ModdedDamageType { get; protected set; }
        public static DamageAPI.ModdedDamageType NuclearDamageType;

        public override void Initialize()
        {
            base.Initialize();
            NuclearDamageType = ModdedDamageType;
        }

        public override void Delegates()
        {
            GlobalEventManager.onServerDamageDealt += InflictNuclearSickness;
        }

        private void InflictNuclearSickness(DamageReport report)
        {
            var victimBody = report.victimBody;
            var attackerBody = report.attackerBody;
            var damageInfo = report.damageInfo;

            if(victimBody && attackerBody && damageInfo.HasModdedDamageType(NuclearDamageType))
            {
                if(attackerBody.HasBuff(SS2Content.Buffs.bdNukeSpecial))
                {
                    var dotInfo = new InflictDotInfo
                    {
                        attackerObject = attackerBody.gameObject,
                        damageMultiplier = 1,
                        dotIndex = Buffs.NukeRadiationSickness.DotIndex,
                        duration = 2,
                        victimObject = victimBody.gameObject
                    };
                    DotController.InflictDot(ref dotInfo);
                }
                else
                {
                    victimBody.AddTimedBuff(SS2Content.Buffs.bdRadiationSickness, 2, 10);
                }
            }
        }
    }
}