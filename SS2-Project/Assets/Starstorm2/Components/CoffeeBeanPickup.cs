using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using SS2.Items;
using System;
using System.Collections.Generic;
namespace SS2.Components
{
    public class CoffeeBeanPickup : MonoBehaviour
	{       
        public void Initialize(int itemStacks, TeamIndex teamIndex)
        {         
            teamFilter.teamIndex = teamIndex;
            this.itemStacks = itemStacks;
            this.initialized = true;
            this.alive = true;
        }
        

        private void OnTriggerStay(Collider other)
		{
			if (NetworkServer.active && this.initialized && this.alive && TeamComponent.GetObjectTeam(other.gameObject) == this.teamFilter.teamIndex)
			{
				CharacterBody body = other.GetComponent<CharacterBody>();
				if (body)
				{
                    int stack = Mathf.Max(itemStacks, 1);					
					body.AddTimedBuff(SS2Content.Buffs.BuffCoffeeBag, CoffeeBag.buffDuration * stack);
					EffectManager.SimpleEffect(this.pickupEffect, base.transform.position, Quaternion.identity, true);

					this.alive = false;

                    pooler.Cleanup();
				}
			}
		}        

        public bool initialized;
        private int itemStacks;    
        public CoffeeBeanPooler pooler;
        [Tooltip("The base object to destroy when this pickup is consumed.")]
		public GameObject baseObject;

		[Tooltip("The team filter object which determines who can pick up this pack.")]
		public TeamFilter teamFilter;

		public GameObject pickupEffect;

		private bool alive = true;
	}
}
