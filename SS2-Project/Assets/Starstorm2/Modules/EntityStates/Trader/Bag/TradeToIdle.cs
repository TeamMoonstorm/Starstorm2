using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;

namespace EntityStates.Trader.Bag
{
    public class TradeToIdle : BagBaseState
    {
        public static string enterSoundString;
        public static string exitSoundString;

        public static float duration;

        public static float dropUpVelocityStrength;
        public static float dropForwardVelocityStrength;

        public static GameObject muzzleFlashEffectPrefab;
        public static string muzzleString;

        public static float lowTrade = -0.1f;
        public static float highTrade = 0.4f;
        public static float rareThreshold = 0.72f;
        public static float uncommonThreshold = 0.32f;
        public static float commonThreshold = 0.01f;
        private bool foundValidItem;

        protected override bool enableInteraction
        {
            get
            {
                return false;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(enterSoundString, gameObject);
            //sfx, vfx, etc.

            if (!NetworkServer.active)
                return;

            foundValidItem = false;
            PickupIndex zanPickup = PickupIndex.none;
            ItemDef playerItem = ItemCatalog.GetItemDef(traderController.lastTradedItemIndex);
            if (playerItem != null)
            {
                float playerValue = traderController.GetValue(playerItem);
                Debug.Log("initial player item value: " + playerValue);

                if (playerItem.tier == ItemTier.Tier1 || playerItem.tier == ItemTier.VoidTier1)
                    playerValue -= 0.05f; //btfo

                playerValue += Util.Remap(UnityEngine.Random.value, 0f, 1f, lowTrade, highTrade);
                Debug.Log("slightly randomized: " + playerValue);


                if (playerValue >= rareThreshold)
                {
                    //spawn red
                    ItemIndex[] tier3Items = ItemCatalog.tier3ItemList.ToArray();
                    ItemIndex randomItem = tier3Items[UnityEngine.Random.Range(0, tier3Items.Length)];
                    zanPickup = PickupCatalog.FindPickupIndex(randomItem);

                }
                else if (playerValue >= uncommonThreshold)
                {
                    //spawn green
                    ItemIndex[] tier2Items = ItemCatalog.tier2ItemList.ToArray();
                    ItemIndex randomItem = tier2Items[UnityEngine.Random.Range(0, tier2Items.Length)];
                    zanPickup = PickupCatalog.FindPickupIndex(randomItem);
                }
                else if (playerValue >= commonThreshold)
                {
                    //spawn white
                    ItemIndex[] tier1Items = ItemCatalog.tier1ItemList.ToArray();
                    ItemIndex randomItem = tier1Items[UnityEngine.Random.Range(0, tier1Items.Length)];
                    zanPickup = PickupCatalog.FindPickupIndex(randomItem);
                }

                if (playerValue < commonThreshold)
                {
                    //spawn scrap
                    zanPickup = PickupCatalog.FindPickupIndex(RoR2Content.Items.ScrapWhite.itemIndex);
                }
                else
                {
                    if (zanPickup == PickupCatalog.FindPickupIndex(RoR2Content.Items.ScrapWhite.itemIndex))
                        zanPickup = PickupCatalog.FindPickupIndex(RoR2Content.Items.Syringe.itemIndex);
                    if (zanPickup == PickupCatalog.FindPickupIndex(RoR2Content.Items.ScrapGreen.itemIndex))
                        zanPickup = PickupCatalog.FindPickupIndex(RoR2Content.Items.Feather.itemIndex);
                    if (zanPickup == PickupCatalog.FindPickupIndex(RoR2Content.Items.ScrapRed.itemIndex))
                        zanPickup = PickupCatalog.FindPickupIndex(RoR2Content.Items.ExtraLife.itemIndex);
                }

                

                //filter out unobtainables
                if (!Run.instance.IsItemAvailable(zanPickup.itemIndex))
                    zanPickup = PickupCatalog.FindPickupIndex(zanPickup.itemIndex + 1);

                traderController.ReduceValue();

                Debug.Log("zan giving: " + zanPickup.GetPickupNameToken());

                if (zanPickup != PickupIndex.none)
                {
                    foundValidItem = true;
                    Transform transform = FindModelChild(muzzleString);
                    PickupDropletController.CreatePickupDroplet(zanPickup, transform.position, Vector3.up * dropUpVelocityStrength + transform.forward * dropForwardVelocityStrength);
                }
            }
        }

        public override void OnExit()
        {
            //sfx, vfx, etc.
            Util.PlaySound(exitSoundString, gameObject);
            if (isAuthority)
                characterBody.SetBuffCount(Moonstorm.Starstorm2.SS2Content.Buffs.bdHiddenSlow20.buffIndex, 0);
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (foundValidItem && fixedAge > duration)
                outer.SetNextState(new Idle());
        }
    }
}
