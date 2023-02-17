using Moonstorm.Components;
using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class ExeCharge : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdExeCharge", SS2Bundle.Indev);

        public sealed class Behavior : BaseBuffBodyBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdExeCharge;
            private float timer;

            public void FixedUpdate()
            {
                if (!body.HasBuff(SS2Content.Buffs.bdExeMuteCharge))
                    timer += Time.fixedDeltaTime;
                if (timer >= 1.2f && body.skillLocator.secondary.stock < body.skillLocator.secondary.maxStock)
                {
                    timer = 0f;
                    body.skillLocator.secondary.AddOneStock();
                    if (body.skillLocator.secondary.stock < body.skillLocator.secondary.maxStock)
                        Util.PlaySound("ExecutionerGainCharge", gameObject);
                    if (body.skillLocator.secondary.stock >= body.skillLocator.secondary.maxStock)
                        Util.PlaySound("ExecutionerMaxCharge", gameObject);
                    body.SetAimTimer(1.6f);
                }
            }
        }
    }
}
