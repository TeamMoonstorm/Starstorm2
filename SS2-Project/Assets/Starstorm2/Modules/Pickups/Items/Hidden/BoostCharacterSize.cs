using MSU;
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections.Generic;
using RoR2.Items;
using RoR2.CharacterAI;
namespace SS2.Items
{
    // body is x% larger
    public sealed class BoostCharacterSize : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemDef>("BoostCharacterSize", SS2Bundle.Items);

        private static bool modifySkillDrivers = true;

        //  FUCKING WITH HULL SIZES AND COLLIDERS COULD BE COOL. WILL RE-EVALUATE ONCE THIS GETS USED MORE
        // AND PROJECTILES MAYBE?? WHO KNOWS
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
            private static ItemDef GetItemDef() => SS2Content.Items.BoostCharacterSize;

            private float currentStack;
            private Transform modelTransform;

            private AISkillDriver[] skillDriverCache = null;
            public void Start()
            {
                UpdateScale(stack);
            }

            private void FixedUpdate()
            {
                if (stack != currentStack)
                {
                    UpdateScale(stack);                    
                }
            }
            private void UpdateScale(float stack)
            {
                float scale = 1 + stack * 0.01f;
                float oldScale = 1 + currentStack * 0.01f;
                float deltaScale = scale / oldScale;
                currentStack = stack;

                if (deltaScale == 1) return;

                if (!modelTransform && body.modelLocator)
                {
                    modelTransform = body.modelLocator.modelTransform;
                }
                if(modelTransform)
                {
                    body.radius *= deltaScale;
                    modelTransform.localScale *= deltaScale;
                }

                if(modifySkillDrivers && body.master)
                {                    
                    ModifySkillDrivers(deltaScale);
                }

                
            }

            private void ModifySkillDrivers(float deltaScale)
            {
                if(skillDriverCache == null)
                {
                    skillDriverCache = body.master.aiComponents[0].skillDrivers;
                }

                foreach(AISkillDriver driver in skillDriverCache)
                {
                    driver.maxDistance *= deltaScale;
                    driver.minDistance *= deltaScale;
                }
            }

            private void OnDestroy()
            {
                UpdateScale(0);
            }
        }
    }
}
