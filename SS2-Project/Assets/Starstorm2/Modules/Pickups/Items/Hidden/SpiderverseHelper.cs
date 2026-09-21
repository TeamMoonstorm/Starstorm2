using MSU;
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections.Generic;
using RoR2.Items;

namespace SS2.Items
{
    // plays back body animations at lower framerate
    // like spiderverse movie
    public sealed class SpiderverseHelper : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemDef>("SpiderverseHelper", SS2Bundle.Items);

        public override void Initialize()
        {          
        }
        

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class BodyBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.SpiderverseHelper;

            public Animator animator;
            public int fps = 8;

            private float timer;

            public new void Awake()
            {
                base.Awake();

                if (body.modelLocator != null && body.modelLocator.modelTransform != null && body.modelLocator.modelTransform.TryGetComponent(out Animator anim))
                {
                    animator = anim;
                }
            }

            private void Update()
            {
                if (animator)
                {
                    timer += Time.deltaTime;
                    var updateTime = 1f / fps;
                    animator.speed = 0;

                    if (timer > updateTime)
                    {
                        timer -= updateTime;
                        animator.speed = updateTime / Time.deltaTime;
                    }
                }
            }

            private void OnDestroy()
            {
                if (animator)
                {
                    animator.speed = 1f;
                }
            }
        }
    }
}
