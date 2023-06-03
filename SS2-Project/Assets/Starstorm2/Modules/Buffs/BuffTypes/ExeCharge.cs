using Moonstorm.Components;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class ExeCharge : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdExeCharge", SS2Bundle.Executioner2);

        public static GameObject plumeEffect = SS2Assets.LoadAsset<GameObject>("exePlume", SS2Bundle.Executioner2);
        public static GameObject plumeEffectLarge = SS2Assets.LoadAsset<GameObject>("exePlumeBig", SS2Bundle.Executioner2);

        public sealed class Behavior : BaseBuffBodyBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdExeCharge;
            private float timer;

            public void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    if (body.baseNameToken != "SS2_EXECUTIONER2_NAME" || body.HasBuff(SS2Content.Buffs.bdExeMuteCharge))
                        return;
                    else
                        timer += Time.fixedDeltaTime;

                    if (timer >= 1.2f && body.skillLocator.secondary.stock < body.skillLocator.secondary.maxStock)
                    {
                        timer = 0f;

                        body.skillLocator.secondary.AddOneStock();

                        if (body.skillLocator.secondary.stock < body.skillLocator.secondary.maxStock)
                        {
                            Util.PlaySound("ExecutionerGainCharge", gameObject);
                            EffectManager.SimpleMuzzleFlash(plumeEffect, gameObject, "ExhaustL", true);
                            EffectManager.SimpleMuzzleFlash(plumeEffect, gameObject, "ExhaustR", true);
                        }
                        if (body.skillLocator.secondary.stock >= body.skillLocator.secondary.maxStock)
                        {
                            Util.PlaySound("ExecutionerMaxCharge", gameObject);
                            EffectManager.SimpleMuzzleFlash(plumeEffectLarge, gameObject, "ExhaustL", true);
                            EffectManager.SimpleMuzzleFlash(plumeEffectLarge, gameObject, "ExhaustR", true);
                            EffectManager.SimpleEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/LightningFlash.prefab").WaitForCompletion(), body.corePosition, Quaternion.identity, true);
                        }

                        body.SetAimTimer(timer);
                    }
                }
            }
        }
    }
}
