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
using R2API;

namespace SS2.Items
{
    public sealed class HuntersSigil : SS2Item, IContentPackModifier
    {
        private const string token = "SS2_ITEM_HUNTERSSIGIL_DESC";
        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;

        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

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

        public Material _matOverlay; //SS2Assets.LoadAsset<Material>("matSigilBuffOverlay", SS2Bundle.Items);
        private BuffDef _sigilBuff; //SS2Assets.LoadAsset<BuffDef>("BuffSigil", SS2Bundle.Items);
        private BuffDef _sigilBuffHidden; //SS2Assets.LoadAsset<BuffDef>("BuffSigilHidden", SS2Bundle.Items);

        public override void Initialize()
        {
            BuffOverlays.AddBuffOverlay(_sigilBuff, _matOverlay);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "HuntersSigil" - Items
             * GameObject - "SigilWard" - Items
             * GameObject - "SigilEffect" - Items
             * BuffDef - "BuffSigil" - Items
             * BuffDef - "BuffSigilHidden" - Items
             */
            yield break;
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.buffDefs.Add(new BuffDef[]
            {
                _sigilBuff,
                _sigilBuffHidden
            });
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

        public sealed class BuffSigilBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffSigil;
            public void OnDestroy()
            {
                CharacterBody.SetBuffCount(SS2Content.Buffs.BuffSigilHidden.buffIndex, 0);
            }
        }
        public sealed class BuffSigilHiddenBehavior : BaseBuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffSigilHidden;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.armorAdd += HuntersSigil.baseArmor * BuffCount;
                args.damageMultAdd += HuntersSigil.baseDamage * BuffCount;
            }
        }
    }
}
