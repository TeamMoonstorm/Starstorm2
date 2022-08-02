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

        public static float Duration = 3;

        public static ModdedDamageType gougeDamageType;

        public override void Initialize()
        {
            ModdedDamageType = gougeDamageType;
            DotIndex = Buffs.Gouge.index;
        }

        public override void Delegates()
        {
            GlobalEventManager.onServerDamageDealt += ApplyGouge;
            DotController.onDotInflictedServerGlobal += RefreshGouge;
        }

        //What the hell does this do? I tried commenting this out, but it still refreshes Gouge. - mal
        private void RefreshGouge(DotController dotController, ref InflictDotInfo inflictDotInfo)
        {
            if (inflictDotInfo.dotIndex == DotIndex)
            {
                int i = 0;
                int count = dotController.dotStackList.Count;

                while (i < count)
                {
                    if (dotController.dotStackList[i].dotIndex == DotIndex)
                    {
                        dotController.dotStackList[i].timer = Mathf.Max(dotController.dotStackList[i].timer, Duration);
                    }
                    i++;
                }
            }
        }

        private void ApplyGouge(DamageReport report)
        {
            var victimBody = report.victimBody;
            var attackerBody = report.attackerBody;
            var damageInfo = report.damageInfo;
            if (DamageAPI.HasModdedDamageType(damageInfo, ModdedDamageType))
            {
                var gougeDamage = (damageInfo.crit ? 2f : 1f) * 1.2f;
                var dotInfo = new InflictDotInfo()
                {
                    attackerObject = attackerBody.gameObject,
                    victimObject = victimBody.gameObject,
                    dotIndex = Buffs.Gouge.index,
                    duration = 3,
                    damageMultiplier = gougeDamage,
                };
                DotController.InflictDot(ref dotInfo);
            }
        }
    }
}
