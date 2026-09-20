using MSU;
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections.Generic;
using RoR2.Items;
using UnityEngine.Networking;
using RoR2.CharacterAI;

namespace SS2.Items
{
    public sealed class ToyHelper : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acToyHelper", SS2Bundle.Items);
        public static Texture toyMandoSprite;

        public override void Initialize()
        {
            BuffOverlays.AddBuffOverlay(AssetCollection.FindAsset<BuffDef>("bdToy"), AssetCollection.FindAsset<Material>("matToyOverlay"));
            toyMandoSprite = AssetCollection.FindAsset<Texture>("texToyCommandoIcon");

            On.RoR2.CharacterBody.OnDeathStart += CharacterBody_OnDeathStart;
        }

        private void CharacterBody_OnDeathStart(On.RoR2.CharacterBody.orig_OnDeathStart orig, CharacterBody self)
        {
            // to-do:
            // get body ragdoll & freeze joint rotations so they just fall to ground static
            // ideally after very small delay so they're in a mid-death pose

            orig(self);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class BodyBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.ToyHelper;

            private float oldAmount;
            private AISkillDriver[] skillDriverCache = null;
            private Transform modelTransform;
            private Texture oldIcon = null;

            public void Start()
            {
                UpdateScale(55f);

                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                if (BodyCatalog.FindBodyIndex(body) == BodyCatalog.FindBodyIndex("CommandoBody"))
                {
                    oldIcon = body.portraitIcon;
                    body.portraitIcon = ToyHelper.toyMandoSprite;
                }
                
                if (NetworkServer.active)
                {
                    body.AddBuff(SS2Content.Buffs.bdToy);
                }
            }

            private void UpdateScale(float amount)
            {
                float scale = 1 - amount * 0.01f;
                float oldScale = 1 - oldAmount * 0.01f;
                float deltaScale = scale / oldScale;
                oldAmount = amount;

                if (deltaScale == 1) return;

                if (!modelTransform && body.modelLocator)
                {
                    modelTransform = body.modelLocator.modelTransform;
                }
                if (modelTransform)
                {
                    body.radius *= deltaScale;
                    modelTransform.localScale *= deltaScale;
                }

                if (NetworkServer.active && body.master)
                {
                    ModifySkillDrivers(deltaScale);
                }
            }

            private void ModifySkillDrivers(float deltaScale)
            {
                if (body == null || body.master.aiComponents.Length < 1)
                {
                    return;
                }

                if (skillDriverCache == null)
                {
                    skillDriverCache = body.master.aiComponents[0].skillDrivers;
                }

                foreach (AISkillDriver driver in skillDriverCache)
                {
                    driver.maxDistance *= deltaScale;
                    driver.minDistance *= deltaScale;
                }
            }

            private void OnDestroy()
            {
                if (NetworkServer.active && body.HasBuff(SS2Content.Buffs.bdToy))
                {
                    body.SetBuffCount(SS2Content.Buffs.bdToy.buffIndex, 0);
                }

                // ??????????
                if (oldIcon != null)
                {
                    body.portraitIcon = oldIcon;
                }

                if (body.healthComponent.alive)
                    UpdateScale(0);
            }
        }
    }
}
