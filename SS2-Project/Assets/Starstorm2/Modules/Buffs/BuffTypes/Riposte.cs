using Moonstorm.Components;
using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    //[DisabledContent]
    public sealed class Riposte : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffRiposte", SS2Bundle.NemMercenary);
        
        ///////////
        public override Material OverlayMaterial => SS2Assets.LoadAsset<Material>("matReactorBuffOverlay", SS2Bundle.Items);

        public static float invincibilityDuration;

        public sealed class Behavior : BaseBuffBodyBehavior, RoR2.IOnIncomingDamageServerReceiver
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffRiposte;
            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if(damageInfo.attacker)
                {
                    damageInfo.rejected = true;
                    base.GetComponent<CharacterBody>().AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 0.33f);
                }
                
                //vfx
                //sound (would have to be part of vfx)
            }
        }
    }
}
