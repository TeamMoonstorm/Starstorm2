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
    public sealed class HuntersSigil : SS2Item
    {
        private const string token = "SS2_ITEM_HUNTERSSIGIL_DESC";
        public override SS2AssetRequest<ItemAssetCollection> AssetRequest<ItemAssetCollection>()
        {
            return SS2Assets.LoadAssetAsync<ItemAssetCollection>("acHuntersSigil", SS2Bundle.Items);
        }
        public override void OnAssetCollectionLoaded(AssetCollection assetCollection)
        {
            _effect = assetCollection.FindAsset<GameObject>("SigilEffect");
            _sigilWard = assetCollection.FindAsset<GameObject>("SigilWard");
        }

        private static GameObject _effect;
        private static GameObject _sigilWard;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Base amount of extra armor added.")]
        [FormatToken(token, 0)]
        public static float baseArmor = 20;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Amount of extra armor added per stack.")]
        [FormatToken(token, 1)]
        public static float stackArmor = 10;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Base amount of extra damage added. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 2, 100)]
        public static float baseDamage = .2f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Amount of extra damage added per stack. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 3, 100)]
        public static float stackDamage = .10f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Radius the effect is applied in.")]
        [FormatToken(token, 0)]
        public static float radius = 8f;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class Behavior : BaseItemBodyBehavior//, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.HuntersSigil;
            private bool sigilActive = false;
            private GameObject sigilInstance;

            public void FixedUpdate()
            {
                if (!NetworkServer.active) return;
                if (body.notMovingStopwatch > 1f)
                {
                    if (!sigilActive)
                    {
                        EffectManager.SimpleEffect(_effect, body.aimOrigin + new Vector3(0, 0f), Quaternion.identity, true);
                        Vector3 position = body.corePosition;
                        //float radius = 13f;

                        if (sigilInstance != null)
                            NetworkServer.Destroy(sigilInstance);

                        sigilInstance = Object.Instantiate(_sigilWard, position, Quaternion.identity);
                        sigilInstance.GetComponent<TeamFilter>().teamIndex = body.teamComponent.teamIndex;
                        sigilInstance.GetComponent<BuffWard>().radius = radius;
                        WardUtils wu = sigilInstance.GetComponent<WardUtils>();
                        wu.body = body;
                        wu.radius = radius;
                        wu.buffCount = stack;
                        wu.amplifiedBuff = SS2Content.Buffs.BuffSigilHidden;

                        NetworkServer.Spawn(sigilInstance);

                        sigilActive = true;
                    }

                }
                else
                    sigilActive = false;
            }

            public void OnDestroy()
            {
                if (sigilInstance != null)
                    Destroy(sigilInstance);
            }

            public void OnDisable()
            {
                if (sigilInstance != null)
                    Destroy(sigilInstance);
            }
        }
    }
}
