using Moonstorm.Components;
using R2API;
using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    //[DisabledContent]
    public sealed class ReactorBuff : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffReactor", SS2Bundle.Items);
        public override Material OverlayMaterial => SS2Assets.LoadAsset<Material>("matReactorBuffOverlay", SS2Bundle.Items);

        //To-Do: Maybe better invincibility implementation. Projectile deflection for cool points?
        public sealed class Behavior : BaseBuffBodyBehavior, RoR2.IOnIncomingDamageServerReceiver, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffReactor;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.moveSpeedMultAdd += 1f;
            }

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (damageInfo.damageType != DamageType.VoidDeath)
                    damageInfo.rejected = true;
            }
        }
    }
}
