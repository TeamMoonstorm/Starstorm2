using HG;
using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class NkotasHeritage : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("NkotasHeritage", SS2Bundle.Items);

        public const string token = "SS2_ITEM_NKOTASHERITAGE_DESC";

        [ConfigurableField(SS2Config.IDItem, ConfigDesc = "Number of items given upon level up per stack.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static int itemsPerStack = 1;

        //[ConfigurableField(SS2Config.IDItem, ConfigDesc = "Level where white items are removed from the reward pool.")]
        //[TokenModifier(token, StatTypes.Default, 1)]
        //public static int whiteRemovalLevel = 11;

        //[ConfigurableField(SS2Config.IDItem, ConfigDesc = "Level where green items are removed from the reward pool.")]
        //[TokenModifier(token, StatTypes.Default, 2)]
        //public static int greenRemovalLevel = 22;

        //Either store this in a static var or do a dictionary checkup if you want to save all the assetbundle lookups in this code
        //Im lazy and this works so.......................
        private static bool holdit = false; //get some funny uhhhh ace attorney youtube link here :)

        private static CharacterMaster[] characterMasters = new CharacterMaster[0];

        public override void Initialize()
        {
            GlobalEventManager.onCharacterLevelUp += GlobalEventManager_onCharacterLevelUp;
            SceneExitController.onBeginExit += SceneExitController_onBeginExit;
            Stage.onStageStartGlobal += Stage_onServerStageBegin; //Cannot be onServerStageBegin because the players havent spawned by then!
            //PickupDropletController.onDropletHitGroundServer += NkotaApplyParticleEffect;
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
            if (!obj.gameObject.GetComponent<BabyToys.BabyToyToken>())
            {
                NkotasManager.ActivateSingle(obj);
            }
            else
            {
                //SS2Log.Debug("babys removal - ignoring level adjustment");
            }

        }

        public static class NkotasManager
        {
            public static void ActivateSingle(CharacterBody body)
            {
                if (body)
                {
                    if (body.isPlayerControlled)
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