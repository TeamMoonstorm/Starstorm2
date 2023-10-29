﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using JetBrains.Annotations;
using UnityEngine.Networking;
namespace Moonstorm.Starstorm2.Components
{
    public class RemunerationChoiceBehavior : NetworkBehaviour
    {
        //[SyncVar]
        //private PickupIndex pickupIndex = PickupIndex.none;

        //[SyncVar]
        //private bool hidden;

        //[Tooltip("The PickupDisplay component that should show which item this shop terminal is offering.")]
        //public PickupDisplay pickupDisplay;

        //[Tooltip("The position from which the drop will be emitted")]
        //public Transform dropTransform;

        //[Tooltip("The drop table to select a pickup index from - only works if the pickup generates itself")]
        //public PickupDropTable dropTable;

        //[Tooltip("The velocity with which the drop will be emitted. Rotates with this object.")]
        //public Vector3 dropVelocity;

        //private Xoroshiro128Plus rng;
        //public RemunerationShopBehavior.ShopOption option;
        //public GenericInteraction interaction;

        // ^^^^^^^^^^^^^^^^^ THIS SHIT WILL BE USED ONCE VOID PORTAL AND VOID TOKENS ARE IMPLEMENTED. UNTIL THEN WE JUST USE VANILLA SHOP TERMINALS

        public RemunerationShopBehavior shop;
        public PurchaseInteraction temp_peniswenis;
        [SyncVar]
        public bool alive;

        void Start()
        {
            this.temp_peniswenis = base.GetComponent<PurchaseInteraction>();
            this.temp_peniswenis.onPurchase.AddListener(OnPurchased);
            if (this.shop)
            {
                this.shop.DiscoverChoice(this);
            }     

            if (NetworkServer.active)
            {

            }
            if (NetworkClient.active)
            {

            }
        }


        public void OnPurchased(Interactor interactor)
        {
            this.shop.OnChoicePicked(this);
        }
		
	}
}
