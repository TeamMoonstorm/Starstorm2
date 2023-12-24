using Moonstorm.Components;
using Moonstorm.Starstorm2.Items;
using R2API;
using RoR2;
using System;
using UnityEngine;
using RoR2.CharacterAI;
using System.Linq;

namespace Moonstorm.Starstorm2.Buffs
{
    //[DisabledContent]

    // FIVE FUCKING BUFFS
    // this one is for visuals/icon only
    //★ not anymore!!!!
    public sealed class ChirrFriend : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffChirrFriend", SS2Bundle.Chirr);

        public override Material OverlayMaterial => SS2Assets.LoadAsset<Material>("matFriendOverlay", SS2Bundle.Chirr);

        public sealed class Behavior : BaseBuffBodyBehavior, IOnTakeDamageServerReceiver
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffChirrFriend;
            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (damageReport.attacker != damageReport.victim && !damageReport.isFallDamage)
                {
                    if (body.GetBuffCount(SS2Content.Buffs.BuffChirrWither) < 20f)
                        body.AddBuff(SS2Content.Buffs.BuffChirrWither);
                }
            }

            public void OnDestroy()
            {
                body.SetBuffCount(SS2Content.Buffs.BuffChirrWither.buffIndex, 0);
            }
        }
    }
}
