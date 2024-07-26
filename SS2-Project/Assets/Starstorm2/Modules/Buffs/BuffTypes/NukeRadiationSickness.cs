using Moonstorm.Components;
using R2API;
using R2API.ScriptableObjects;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm.Starstorm2.Buffs
{
    public class NukeRadiationSickness : BuffBase
    {
        public override BuffDef BuffDef => SS2Assets.LoadAsset<BuffDef>("bdRadiationSickness", SS2Bundle.Indev);

        public static DotController.DotIndex DotIndex { get; private set; }
        public override void Initialize()
        {
            var damageColor = SS2Assets.LoadAsset<SerializableDamageColor>("NukeDamageColor", SS2Bundle.Indev);

            var dotDef = new DotController.DotDef();
            dotDef.resetTimerOnAdd = true;
            dotDef.associatedBuff = BuffDef;
            dotDef.damageCoefficient = 0.2f;
            dotDef.interval = 0.1f;
            dotDef.damageColorIndex = damageColor.DamageColorIndex;

            DotIndex = DotAPI.RegisterDotDef(dotDef);
        }

        public class NukeRadiationSicknessBehaviour : BaseBuffBodyBehavior, IBodyStatArgModifier, IOnIncomingDamageServerReceiver
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => SS2Content.Buffs.bdRadiationSickness;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.levelMultAdd -= buffStacks / 10;
            }

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if(damageInfo != null)
                {
                    var dmg = damageInfo.damage;
                    damageInfo.damage += (dmg * (buffStacks / 10));
                }
            }
        }
    }
}
