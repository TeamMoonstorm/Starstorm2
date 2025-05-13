using R2API;
using RoR2;
using RoR2.Items;

using MSU;
using System.Collections.Generic;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using EntityStates;
using UnityEngine.AddressableAssets;
using System;

namespace SS2.Items
{
    public sealed class PrimalBirthright : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acPrimalBirthright", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of legendary chests the first stack grants per stage.")]
        [FormatToken("SS2_ITEM_PRIMAL_BIRTHRIGHT_DESC", 0)]
        public static float legendaryCountBase = 1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of legendary chests each additional stack grants per stage.")]
        [FormatToken("SS2_ITEM_PRIMAL_BIRTHRIGHT_DESC", 1)]
        public static float legendaryCountStacking = 1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount the price of the Legendary chest is multiplied by. (1 = 100%, normal value for current level)")]
        [FormatToken("SS2_ITEM_RELICOFMASS_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float priceModifier = 1f;

        public GameObject indevChest; // this probably not be here in a live build i just wanted to do it fast
        public InteractableSpawnCard indevCard;
        public PrimalPrevention? primalToken;
        public Xoroshiro128Plus birthrightRng;
 
        public override void Initialize()
        {
            On.RoR2.TeleporterInteraction.IdleState.OnInteractionBegin += TeleporterInteractionPrimalOverride;
            On.RoR2.SceneDirector.PopulateScene += PopulateSceneAddPrimalChest;
            //On.RoR2.SceneDirector.OnServerTeleporterPlaced += OnTeleporterPlacedAddPrimalToken;
            On.RoR2.SceneDirector.PlaceTeleporter += PlaceTeleporterAddPrimalToken;

            var tempChest = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/GoldChest/GoldChest.prefab").WaitForCompletion();
            indevChest = PrefabAPI.InstantiateClone(tempChest, "PrimalChest");

            var pinter = indevChest.GetComponent<PurchaseInteraction>();
            pinter.displayNameToken = "SS2_BIRTHRIGHT_CHEST_NAME";
            pinter.contextToken = "SS2_BIRTHRIGHT_CHEST_CONTEXT";

            indevCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            indevCard.name = "iscPrimalChest";
            indevCard.prefab = indevChest;
            indevCard.sendOverNetwork = true;
            indevCard.hullSize = HullClassification.Human;
            indevCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            indevCard.requiredFlags = RoR2.Navigation.NodeFlags.None;
            indevCard.forbiddenFlags = RoR2.Navigation.NodeFlags.None;

            indevCard.directorCreditCost = 0;

            indevCard.occupyPosition = true;
            indevCard.orientToFloor = true;
            indevCard.skipSpawnWhenSacrificeArtifactEnabled = false;
            indevCard.maxSpawnsPerStage = -1;

        }

        private void PlaceTeleporterAddPrimalToken(On.RoR2.SceneDirector.orig_PlaceTeleporter orig, SceneDirector self)
        {
            orig(self);
            primalToken = self.teleporterInstance.AddComponent<PrimalPrevention>();
        }

        private void TeleporterInteractionPrimalOverride(On.RoR2.TeleporterInteraction.IdleState.orig_OnInteractionBegin orig, BaseState self, Interactor activator)
        {
            var tc = self.outer.gameObject.GetComponent<PrimalPrevention>();
            if (tc)
            {
                foreach(var pinter in tc.purchaseInteractions)
                {
                    if (pinter.available)
                    {
                        if (birthrightRng == null)
                        {
                            birthrightRng = new Xoroshiro128Plus(Run.instance.seed);
                        }
                        if (birthrightRng.RangeFloat(0, 1) < .975f)
                        {
                            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                            {
                                baseToken = "SS2_BIRTHRIGHT_UNCLAIMED"
                            });
                        }
                        else
                        {
                            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                            {
                                baseToken = "SS2_BIRTHRIGHT_UNCLAIMED_RARE"
                            });
                        }
                        return;
                    }
                }

                orig(self, activator);
            }
            else
            {
                orig(self, activator);
            }
        }

        private void PopulateSceneAddPrimalChest(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            primalToken = null;

            orig(self); //things after orig occur after PlaceTeleporter

            var sceneDef = SceneCatalog.GetSceneDefForCurrentScene();
            //SS2Log.Warning("Scenedef : " + sceneDef + " | " + sceneDef.sceneType + " | " + sceneDef.allowItemsToSpawnObjects);
            if (sceneDef.sceneType == SceneType.Stage || sceneDef.allowItemsToSpawnObjects)
            {
                if (birthrightRng == null)
                {
                    birthrightRng = new Xoroshiro128Plus(Run.instance.seed);
                }

                foreach (var player in PlayerCharacterMasterController.instances)
                {
                    //SS2Log.Info("Found a player with item");
                    int itemCount = player.master.inventory.GetItemCount(SS2Content.Items.PrimalBirthright);
                    for(int i = 0; i < itemCount; ++i)
                    {
                        var chest = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(indevCard, new DirectorPlacementRule { placementMode = DirectorPlacementRule.PlacementMode.Random }, birthrightRng));
                        //SS2Log.Warning("Chest : " + chest + " | " + chest.name);

                        var pinter = chest.GetComponent<PurchaseInteraction>();
                        if (pinter && primalToken)
                        {
                            //SS2Log.Warning("FOUND IT " + pinter);
                            primalToken.purchaseInteractions.Add(pinter);
                            pinter.Networkcost = (int)(Run.instance.GetDifficultyScaledCost(pinter.cost) * priceModifier);
                            //SS2Log.Warning("Added " + pinter);
                            //
                            //pinter.displayNameToken = Language.GetStringFormatted("SS2_BIRTHRIGHT_CHEST_NAME", Util.GetBestBodyName(player.body.gameObject));


                            //SS2Log.Warning("intermediate: " + intermediate);
                            //pinter.displayNameToken = intermediate.Replace("{0}", player.body.GetDisplayName());
                            //SS2Log.Warning("ahh: " + intermediate.Replace("{0}", player.body.GetDisplayName()) + " | " + player.body.GetDisplayName());
                            //string result = player.body.GetDisplayName();
                            //pinter.displayNameToken = Language.GetStringFormatted("SS2_BIRTHRIGHT_CHEST_NAME", result);
                        }
                    }
                }
            }

          


        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

    }

    public class PrimalPrevention : MonoBehaviour
    {
        public List<PurchaseInteraction> purchaseInteractions = new List<PurchaseInteraction>();
        //i swear this makes sense
    }
}
