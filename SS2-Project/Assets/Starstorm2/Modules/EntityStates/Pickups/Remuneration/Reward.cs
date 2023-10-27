using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.Components;
namespace EntityStates.Pickups.Remuneration
{
    public class Reward : BaseState
    {
        public static float dropletSpreadAngle = 30f;
        public static float dropletUpVelocity = 20f;
        public static float dropletForwardVelocity = 8f;
        //private Xoroshiro128Plus rng;

        private int itemsDropped;
        private int numItems = 3;

        private RemunerationShopBehavior shop;

        private Transform portalTransform;

        public static GameObject explosionPrefab = SS2Assets.LoadAsset<GameObject>("RemunerationExplosion", SS2Bundle.Items);
        public static float effectScale = 1f;
        public override void OnEnter()
        {
            base.OnEnter();

            this.shop = base.GetComponent<RemunerationShopBehavior>();

            if (NetworkServer.active)
            {
                //this.rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
                ChildLocator childLocator = base.GetComponent<ChildLocator>();
                if(childLocator)
                {
                    
                    this.portalTransform = childLocator.FindChild("Portal");
                    // should probably find out why portaltransform can ever be fucking null ??????????????????????? wat
                    EffectData data = new EffectData
                    {
                        origin = portalTransform ? portalTransform.position : base.transform.position, 
                        rotation = Quaternion.identity,
                        scale = effectScale,
                    };
                    EffectManager.SpawnEffect(explosionPrefab, data, true);
                }
                while(itemsDropped < numItems)
                {                 
                    this.DropItem();
                    itemsDropped++;
                }
                
            }

        }


        private void DropItem()
        {
            //0
            //-15 15
            //-30 0 30
            //-45 -15 15 45
            //-60 -30 0 30 60
            //float angle = (-numItems / 2 + itemsDropped) * dropletSpreadAngle + dropletSpreadAngle / 2;
            //angle = (itemsDropped * dropletSpreadAngle) - dropletSpreadAngle;
            float startingAngle = -(dropletSpreadAngle / 2) * (numItems-1);
            float angle = startingAngle + itemsDropped * dropletSpreadAngle;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * (this.portalTransform ? this.portalTransform.forward : base.transform.forward);
            Vector3 velocity = Vector3.up * dropletUpVelocity + direction * dropletForwardVelocity;
            this.shop.SpawnDroplet(this.portalTransform ? this.portalTransform.position : base.transform.position, velocity);


        }
    }
}
