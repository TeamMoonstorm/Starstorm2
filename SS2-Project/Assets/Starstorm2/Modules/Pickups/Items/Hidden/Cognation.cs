using RoR2;
using RoR2.Items;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class Cognation : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("Cognation", SS2Bundle.Items);

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.Cognation;

            private Material ghostMaterial = SS2Assets.LoadAsset<Material>("matCognation", SS2Bundle.Items);

            private CharacterModel model;

            private void Start()
            {
                if (body.teamComponent.teamIndex == TeamIndex.Player)
                {
                    body.inventory.RemoveItem(SS2Content.Items.Cognation, stack);
                    Destroy(this);
                }

                body.baseMaxHealth *= 3;
                body.baseMoveSpeed *= 1.25f;
                body.baseAttackSpeed *= 1.25f;
                body.baseDamage *= 0.9f;
                body.baseArmor -= 25;
                body.PerformAutoCalculateLevelStats();
                body.RecalculateStats();

                ModelLocator modelLoc = body.modelLocator;
                if (modelLoc)
                {
                    Transform modelTransform = modelLoc.modelTransform;
                    if (modelTransform)
                    {
                        model = modelTransform.GetComponent<CharacterModel>();
                    }
                }

                if (model)
                {
                    ModifyCharacterModel();
                }
            }

            private void ModifyCharacterModel()
            {
                for (int i = 0; i < model.baseRendererInfos.Length; i++)
                {
                    var mat = model.baseRendererInfos[i].defaultMaterial;
                    if (mat.shader.name.StartsWith("Hopoo Games/Deferred"))
                    {
                        mat = ghostMaterial;
                        model.baseRendererInfos[i].defaultMaterial = mat;
                    }
                }
            }

            private void OnDestroy()
            {
                if (model)
                {
                    var modelDestroyOnUnseen = model.GetComponent<DestroyOnUnseen>() ?? model.gameObject.AddComponent<DestroyOnUnseen>();
                    modelDestroyOnUnseen.cull = true;
                }
            }
        }
    }
}
