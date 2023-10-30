using Moonstorm.Components;
using Moonstorm.Starstorm2.Items;
using R2API;
using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class CanJump : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdCanJump", SS2Bundle.Equipments);

        public sealed class Behavior : BaseBuffBodyBehavior

        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdCanJump;
            public void Start()
            {
                body.baseJumpCount++;
                body.characterMotor.onHitGroundAuthority += RemoveBuff;
            }

            public void FixedUpdate()
            {
                if (body.inputBank.jump.justPressed) // server doesnt see client inputs so the buff cant be removed by the server. not gonna bother. too lazy. nemmerc must release.
                {
                    EffectManager.SimpleEffect(SS2Assets.LoadAsset<GameObject>("canExhaust", SS2Bundle.Equipments), body.transform.position, body.transform.rotation, false);
                    body.RemoveBuff(SS2Content.Buffs.bdCanJump.buffIndex);
                }
            }

            private void RemoveBuff(ref CharacterMotor.HitGroundInfo hitGroundInfo)
            {
                body.RemoveBuff(SS2Content.Buffs.bdCanJump.buffIndex);
            }

            public void OnDestroy()
            {
                body.characterMotor.onHitGroundAuthority -= RemoveBuff;
                body.baseJumpCount--;
            }
        }
    }
}
