using System;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using Moonstorm.Starstorm2.Items;
namespace Moonstorm.Starstorm2.Components
{
	public class CoffeeBeanPickup : MonoBehaviour
	{
		private void OnTriggerStay(Collider other)
		{
			if (NetworkServer.active && this.alive && TeamComponent.GetObjectTeam(other.gameObject) == this.teamFilter.teamIndex)
			{
				CharacterBody body = other.GetComponent<CharacterBody>();
				if (body)
				{
					int stack = body.inventory ? body.inventory.GetItemCount(SS2Content.Items.CoffeeBag) : 1;					
					body.AddTimedBuff(SS2Content.Buffs.BuffCoffeeBag, CoffeeBag.buffDuration * stack);
					EffectManager.SimpleEffect(this.pickupEffect, base.transform.position, Quaternion.identity, true);

					this.alive = false;
					UnityEngine.Object.Destroy(this.baseObject);
				}
			}
		}

		[Tooltip("The base object to destroy when this pickup is consumed.")]
		public GameObject baseObject;

		[Tooltip("The team filter object which determines who can pick up this pack.")]
		public TeamFilter teamFilter;

		public GameObject pickupEffect;

		private bool alive = true;
	}
}
