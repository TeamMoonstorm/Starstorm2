using RoR2;
using UnityEngine;
using RoR2.ContentManagement;
using RoR2.Items;
using R2API;
namespace SS2.Items
{
    public sealed class Bleedout : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acBleedout", SS2Bundle.Items);
        public override bool IsAvailable(ContentPack contentPack) => true;

        public static DotController.DotIndex bleedout;
        public override void Initialize()
        {
            bleedout = DotAPI.RegisterDotDef(0.25f, 1f, DamageColorIndex.Bleed, SS2Assets.LoadAsset<BuffDef>("BuffBleedout", SS2Bundle.Items));
        }

        public class BleedoutBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = false)]
            private static ItemDef GetItemDef() => SS2Content.Items.Bleedout;
            private GameObject indicatorInstance;
            private float rechargeTimer;

            private void OnEnable()
            {
                this.body.AddBuff(SS2Content.Buffs.BuffBleedoutReady);
            }

            private void OnDisable()
            {
                body.RemoveBuff(SS2Content.Buffs.BuffBleedoutReady);
            }

            private void FixedUpdate()
            {
                if (!body.HasBuff(SS2Content.Buffs.BuffBleedoutReady))
                {
                    this.rechargeTimer += Time.fixedDeltaTime;
                    float cooldown = 240f * Util.ConvertAmplificationPercentageIntoReductionPercentage(20f * (stack - 1));
                    if (this.rechargeTimer >= cooldown)
                    {
                        this.body.AddBuff(SS2Content.Buffs.BuffBleedoutReady);
                    }
                }
                else
                {
                    rechargeTimer = 0f;
                }

                //
                // BLEEDOUT VFX HERE
                //
            }
        }
    }
}
