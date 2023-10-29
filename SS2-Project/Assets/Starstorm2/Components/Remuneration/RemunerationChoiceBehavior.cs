using System;
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
        public static GameObject pedestalPrefab;
        [SystemInitializer]
        private static void Init()
        {
            pedestalPrefab = SS2Assets.LoadAsset<GameObject>("RemunerationPedestalBase", SS2Bundle.Items);
        }
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
        public PurchaseInteraction interaction;
        [SyncVar]
        public bool alive;

        void Start()
        {
            this.interaction = base.GetComponent<PurchaseInteraction>();
            this.interaction.onPurchase.AddListener(OnPurchased);
            if(pedestalPrefab) // CRINGE BUT IDC
            {
                GameObject.Instantiate(pedestalPrefab, base.transform.position, base.transform.rotation);
            }
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
