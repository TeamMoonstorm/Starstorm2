using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SS2.Items
{
    public sealed class CognationHelper : SS2Item
    {
        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        public override void Initialize()
        {
            On.RoR2.Util.GetBestBodyName += AddCognateName;
        }

        private string AddCognateName(On.RoR2.Util.orig_GetBestBodyName orig, GameObject bodyObject) //i love stealing
        {
            var result = orig(bodyObject);
            CharacterBody characterBody = bodyObject?.GetComponent<CharacterBody>();
            if (characterBody && characterBody.inventory && characterBody.inventory.GetItemCount(SS2Content.Items.Cognation) > 0)
            {
                result = Language.GetStringFormatted("SS2_ARTIFACT_COGNATION_PREFIX", result);
            }
            return result;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "CognationHelper" - Artifacts
             * Material - "matCognation" - Artifacts
             */
            yield break;
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.Cognation;

            private static Material ghostMaterial;

            private CharacterModel model;

            private void Start()
            {
                if (body.teamComponent.teamIndex == TeamIndex.Player)
                {
                    body.inventory.RemoveItem(SS2Content.Items.Cognation, stack);
                    Destroy(this);
                }

                if (body.inventory.GetItemCount(SS2Content.Items.TerminationHelper) > 0)
                {
                    body.inventory.RemoveItem(SS2Content.Items.TerminationHelper);
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
                    //SS2Log.Info("swapping shader");
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
                        //SS2Log.Info("swapping shader real " +mat.shader.name + " | " + ghostMaterial + "
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
