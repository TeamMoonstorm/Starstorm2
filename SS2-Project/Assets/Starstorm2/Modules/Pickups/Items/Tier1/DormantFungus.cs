using SS2.Components;
using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;
using MSU;
using System.Collections.Generic;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace SS2.Items
{
    public sealed class DormantFungus : SS2Item
    {
        private const string token = "SS2_ITEM_DORMANTFUNGUS_DESC";
        public override SS2AssetRequest<ItemAssetCollection> AssetRequest<ItemAssetCollection>()
        {
            return SS2Assets.LoadAssetAsync<ItemAssetCollection>("acDormantFungus", SS2Bundle.Items);
        }
        public override void OnAssetCollectionLoaded(AssetCollection assetCollection)
        {
            _dungusTrailEffect = assetCollection.FindAsset<GameObject>("DungusTrailEffect");
            _dungusTrailEffectAlt = assetCollection.FindAsset<GameObject>("DungusTrailEffectAlt");
        }

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Base amount of healing. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float baseHealPercentage = 0.015f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Amount of healing per stack. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float stackHealPercentage = 0.015f;

        private static GameObject _dungusTrailEffect;
        private static GameObject _dungusTrailEffectAlt;
        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.DormantFungus;

            private float timer;
            private float sprintTimer;

            private ModifiedFootstepHandler footstepHandler;
            private Mesh[] mesh = new Mesh[] { };
            private bool hasFootsteps = false;

            public void Start()
            {
                if (body.modelLocator.modelTransform.GetComponent<FootstepHandler>())
                {
                    footstepHandler = body.modelLocator.modelTransform.gameObject.AddComponent<ModifiedFootstepHandler>();
                    footstepHandler.footstepEffect = _dungusTrailEffect;
                    footstepHandler.enableFootstepDust = false;
                    hasFootsteps = true;
                }
            }

            public void FixedUpdate()
            {
                if (body.isSprinting)
                {
                    if (footstepHandler && sprintTimer > 1f)
                        footstepHandler.enableFootstepDust = true;
                    timer += Time.fixedDeltaTime;
                    sprintTimer += Time.fixedDeltaTime;
                    if (timer >= 1f)
                    {
                        if (NetworkServer.active)
                            body.healthComponent.HealFraction(baseHealPercentage + (stackHealPercentage * (stack - 1)), default);
                        timer = 0;
                        if (hasFootsteps)
                        {
                            if (Run.instance.runRNG.nextBool)
                                footstepHandler.footstepEffect = _dungusTrailEffect;
                            else
                                footstepHandler.footstepEffect = _dungusTrailEffectAlt;
                        }
                    }
                }
                else
                {
                    if (hasFootsteps)
                        footstepHandler.enableFootstepDust = false;
                    timer = 0;
                    sprintTimer = 0;
                }
            }

            public void OnDestroy()
            {
                Destroy(footstepHandler);
            }
        }

    }
}
