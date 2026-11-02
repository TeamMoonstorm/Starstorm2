using HG;
using MSU;
using MSU.Config;
using RoR2;
using RoR2.Projectile;
using RoR2.ContentManagement;
using RoR2.Orbs;
using R2API;
using R2API.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using RoR2.Navigation;

namespace SS2.Items
{
    public sealed class NkotasHeritage : SS2Item
    {
        public const string token = "SS2_ITEM_NKOTASHERITAGE_DESC";

        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acNkotasHeritage", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Number of items given upon level up per stack.")]
        [FormatToken(token, 0)]
        public static int itemsPerStack = 1;

        //Either store this in a static var or do a dictionary checkup if you want to save all the assetbundle lookups in this code
        //Im lazy and this works so.......................
        private static bool holdit = false; //get some funny uhhhh ace attorney youtube link here :)

        private static CharacterMaster[] characterMasters = new CharacterMaster[0];
        public static GameObject chestPrefab;
        public static GameObject largeChestPrefab;
        public static GameObject goldChestPrefab;

        public static GameObject chestDropProjectilePrefab;

        public override void Initialize()
        {
            GlobalEventManager.onCharacterLevelUp += GlobalEventManager_onCharacterLevelUp;
            SceneExitController.onBeginExit += SceneExitController_onBeginExit;
            Stage.onStageStartGlobal += Stage_onServerStageBegin; //Cannot be onServerStageBegin because the players havent spawned by then!

            // RoR2/DLC1/Equipment/VendingMachine/VendingMachineProjectile.prefab
            chestDropProjectilePrefab = Addressables.LoadAssetAsync<GameObject>("da6e3d5e20e052f47a8e3ed42ff4107a").WaitForCompletion().InstantiateClone("NkotasHeritageProjectile");
            //if (chestDropProjectilePrefab && chestDropProjectilePrefab.TryGetComponent(out ProjectileInstantiateDeployable pid))
            //{
            //    pid.deployableSlot = DeployableSlot.None;
            //}

            // RoR2/DLC1/Equipment/VendingMachine/VendingMachineModel/VendingMachine.prefab
            GameObject debrisPrefab = Addressables.LoadAssetAsync<GameObject>("e69e4c37270ee1f4a8ecd2e60c03faad").WaitForCompletion().InstantiateClone("NkotasDebris");
            if (debrisPrefab)
            {
                if (debrisPrefab.TryGetComponent(out ModelLocator ml))
                {
                    GameObject.Destroy(ml.modelTransform);
                }

                foreach (Component component in debrisPrefab.GetComponents<Component>())
                {
                    GameObject.Destroy(component);
                }
            }


            // make mat array with mat to reassign to
            Material[] materials = new Material[1];
            // Assets/RoR2/Common/TrimSheets/matTrimSheetMetalGolden_opt.mat
            // i dont feel like cloning the mat in editor just to change the color zz
            Material goldMat = Addressables.LoadAssetAsync<Material>("8eeda62f8a7a7cb43be1334548f63f6d").WaitForCompletion();
            Material nkotaMat = new Material(goldMat.shader);
            nkotaMat.CopyMatchingPropertiesFromMaterial(goldMat);
            nkotaMat.SetColor("_EmColor", new Color(255, 0, 106));
            materials.SetValue(nkotaMat, 0);

            // RoR2/Base/Interactables/Chest1/Chest1.prefab
            chestPrefab = Addressables.LoadAssetAsync<GameObject>("19deacd9e155c044595f2c344e110dde").WaitForCompletion().InstantiateClone("NkotasChest1");
            NkotaifyChestPrefab(chestPrefab);

            // RoR2/Base/Interactables/Chest2/Chest2.prefab
            largeChestPrefab = Addressables.LoadAssetAsync<GameObject>("f460ead3fd64fe0408ec760850cf3301").WaitForCompletion().InstantiateClone("NkotasChest2");
            NkotaifyChestPrefab(largeChestPrefab);

            // RoR2/Base/Interactables/GoldChest/GoldChest.prefab
            goldChestPrefab = Addressables.LoadAssetAsync<GameObject>("e4f9d194b928e2943962a39ad25cc9b7").WaitForCompletion().InstantiateClone("NkotasGoldChest");
            NkotaifyChestPrefab(goldChestPrefab);

            void NkotaifyChestPrefab(GameObject prefab)
            {
                if (prefab)
                {
                    AlignToNormal atn = prefab.AddComponent<AlignToNormal>();
                    atn.maxDistance = 2;
                    atn.offsetDistance = 1;

                    if (prefab.TryGetComponent(out Highlight hl))
                    {
                        hl.targetRenderer.SetMaterials(materials, 1);
                    }

                    GameObject debris = GameObject.Instantiate(debrisPrefab);
                    debris.transform.SetParent(prefab.transform);
                }
            }
        }

        private void Stage_onServerStageBegin(Stage obj)
        {
            if (!NetworkServer.active) return;
            holdit = false;
            for (int i = 0; i < characterMasters.Length; i++)
            {
                if (characterMasters[i])
                {
                    NkotasManager.ActivateSingle(characterMasters[i].GetBody());
                }
            }
            int sex = characterMasters.Length;
            HG.ArrayUtils.Clear(characterMasters, ref sex); //they have sex and thus die
        }

        private void SceneExitController_onBeginExit(SceneExitController obj)
        {
            if (!NetworkServer.active) return;
            holdit = true;
        }

        private void GlobalEventManager_onCharacterLevelUp(CharacterBody obj)
        {
            if (!NetworkServer.active || !obj.inventory) return;
            if (obj.inventory.GetItemCount(SS2Content.Items.NkotasHeritage.itemIndex) < 1) return;
            if (holdit && obj.isPlayerControlled)
            {
                HG.ArrayUtils.ArrayAppend(ref characterMasters, obj.master); //Should add the same master in case they level up multiple times... not sure if its ok?
                return;
            }

            NkotasManager.ActivateSingle(obj);

        }

        public static class NkotasManager
        {
            public static void ActivateSingle(CharacterBody body)
            {
                if (body)
                {
                    if (body.isPlayerControlled)
                    {
                        bool dropped = false;
                        // spawn a chest if we have the prefab
                        if (chestDropProjectilePrefab && !dropped)
                        {
                            int stacks = body.inventory.GetItemCountEffective(SS2Content.Items.NkotasHeritage);
                            float level = TeamManager.instance.GetTeamLevel(body.teamComponent.teamIndex);
                            int chestCount = itemsPerStack * stacks;

                            // these #s/logic are just based directly on ss1
                            float score = stacks * (Mathf.Sqrt(level * 13) - 4);

                            GameObject chestPrefab = NkotasHeritage.chestPrefab;

                            if (Util.CheckRoll(0.2f * score - 1f, body.master))
                            {
                                chestPrefab = NkotasHeritage.goldChestPrefab;
                            }

                            else if (Util.CheckRoll(4f * score, body.master))
                            {
                                chestPrefab = NkotasHeritage.largeChestPrefab;
                            }

                            // this maybe should maybe be instanced each time but i think its fine to just overwrite the base given the limited use & it needing to roll every time?
                            if (NkotasHeritage.chestDropProjectilePrefab && NkotasHeritage.chestDropProjectilePrefab.TryGetComponent(out ProjectileInstantiateDeployable pid))
                            {
                                pid.prefab = chestPrefab;
                            }

                            NodeGraph groundNodes = SceneInfo.instance.groundNodes;
                            // find a nearby node to spawn on
                            List<NodeGraph.NodeIndex> candidates = groundNodes.FindNodesInRange(body.corePosition, 0f, 20f, HullMask.Human);
                            if (candidates != null)
                            {
                                NodeGraph.NodeIndex node = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                                Vector3 pos;
                                groundNodes.GetNodePosition(node, out pos);

                                ProjectileManager.instance.FireProjectileWithoutDamageType(NkotasHeritage.chestDropProjectilePrefab, pos, body.transform.rotation, body.gameObject, body.damage, 0f, RoR2.Util.CheckRoll(body.crit, body.master));
                            }
                            // if no position just do it on the body
                            else
                            {
                                ProjectileManager.instance.FireProjectileWithoutDamageType(NkotasHeritage.chestDropProjectilePrefab, body.corePosition, body.transform.rotation, body.gameObject, body.damage, 0f, RoR2.Util.CheckRoll(body.crit, body.master));
                            }

                            EffectData vfx = new EffectData
                            {
                                origin = body.corePosition,
                                scale = body.radius
                            };
                            EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("NkotasSpawnEffect", SS2Bundle.Items), vfx, true);
                            dropped = true;
                        }

                        // old behavior if we fail to set up the chest projectile for some reason
                        if (!dropped)
                        {
                            int itemCount = itemsPerStack * body.inventory.GetItemCount(SS2Content.Items.NkotasHeritage);

                            SS2Util.DropShipCall(body.transform, 1, TeamManager.instance.GetTeamLevel(body.teamComponent.teamIndex), itemCount, ItemTier.Tier1, "NkotasIdleEffect");

                            Transform effectSpawnLocation = body.transform; //Will only get overwritten if it succesfully find the child
                            ModelLocator modelLocator = body.modelLocator;
                            ChildLocator childLocator;
                            int childLocatorIndex = -1;
                            if (modelLocator == null) childLocator = null;
                            else
                            {
                                Transform transform = modelLocator.modelTransform;
                                childLocator = ((transform != null) ? transform.GetComponent<ChildLocator>() : null);
                            }
                            if (childLocator)
                            {
                                Transform transform = childLocator.FindChild("Head");
                                childLocatorIndex = childLocator.FindChildIndex("Head");
                                if (transform) effectSpawnLocation = transform;
                            }
                            EffectData sexBomb = new EffectData
                            {
                                origin = effectSpawnLocation.position,
                                scale = body.radius
                            };
                            if (childLocatorIndex != -1)
                                sexBomb.SetChildLocatorTransformReference(body.gameObject, childLocatorIndex);

                            EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("NkotasSpawnEffect", SS2Bundle.Items), sexBomb, true);
                        }
                    }
                    else
                    {
                        List<RoR2.Orbs.ItemTransferOrb> inFlightOrbs = new List<RoR2.Orbs.ItemTransferOrb>();
                        ItemDef itemToGrant;
                        do
                        {
                            itemToGrant = SS2Util.NkotasRiggedItemDrop(1, TeamManager.instance.GetTeamLevel(body.teamComponent.teamIndex));
                        } while (itemToGrant.ContainsTag(ItemTag.AIBlacklist) || itemToGrant.ContainsTag(ItemTag.CannotCopy) || itemToGrant == SS2Content.Items.NkotasHeritage || (BodyCatalog.FindBodyIndex("BrotherBody") == body.bodyIndex && itemToGrant.ContainsTag(ItemTag.BrotherBlacklist)));

                        ItemTransferOrb item = ItemTransferOrb.DispatchItemTransferOrb(body.transform.position, body.inventory, itemToGrant.itemIndex, 1, delegate (ItemTransferOrb orb)
                        {
                            body.inventory.GiveItem(orb.itemIndex, orb.stack);
                            inFlightOrbs.Remove(orb);
                        }, default(Either<NetworkIdentity, HurtBox>));
                        inFlightOrbs.Add(item);
                    }
                }
            }
        }
    }
}