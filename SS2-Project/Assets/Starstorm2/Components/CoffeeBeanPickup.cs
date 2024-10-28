using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using SS2.Items;
using System;
using System.Collections.Generic;
namespace SS2.Components
{
    public class CoffeeBeanPickup : NetworkComponentPoolObject
	{
        [SystemInitializer]
        private static void Init()
        {
            Stage.onStageStartGlobal += (_) => EnsurePoolManagerExists(true);
            beanPrefab = SS2Assets.LoadAsset<GameObject>("CoffeeBeanPickup", SS2Bundle.Items);
        }
        private static GameObject beanPrefab;
        public static NetworkedObjectPoolManager PoolManager;
        public static void EnsurePoolManagerExists(bool forceNewInstantiation = false)
        {
            if (CoffeeBeanPickup.PoolManager != null && forceNewInstantiation)
            {
                CoffeeBeanPickup.PoolManager.ResetPools();
                CoffeeBeanPickup.PoolManager = null;
            }
            if (CoffeeBeanPickup.PoolManager == null)
            {
                CoffeeBeanPickup.PoolManager = new NetworkedObjectPoolManager(1, 15, false, true);
            }
        }
        public static CoffeeBeanPickup GetPooledBean()
        {
            CoffeeBeanPickup.EnsurePoolManagerExists(false);
            return (CoffeeBeanPickup)CoffeeBeanPickup.PoolManager.GetPooledObject(beanPrefab); // gets object from pool, instantiates if not
        }
        public static void SpawnBean(Vector3 position, int itemStacks, TeamIndex team)
        {
            if(!NetworkServer.active)
            {
                return;
            }
            CoffeeBeanPickup bean = GetPooledBean();
            if (!bean)
            {
                SS2Log.Error("CoffeeBeanPickup.SpawnBean(): rut roh");
                return;
            }
            bean.Initialize(position, itemStacks, team);

        }
        private void Awake()
        {
            velocity = GetComponent<VelocityRandomOnStart>();
            blink = GetComponent<BeginRapidlyActivatingAndDeactivating>();
        }

        public void Initialize(Vector3 position, int itemStacks, TeamIndex teamIndex)
        {
            transform.position = position;
            transform.rotation = UnityEngine.Random.rotation;
            velocity.Start(); // LOOOOOOOOOOOOOOOOOL
            blink.fixedAge = 0;
            teamFilter.teamIndex = teamIndex;
            this.itemStacks = itemStacks;
            this.initialized = true;
            this.alive = true;
        }

        public override void PreReturnToPool()
        {
            this.initialized = false;
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
                    Cleanup();
				}
			}
		}
        public void Cleanup()
        {
            this.PreReturnToPool();
            this.ReturnToPool();
        }

        private bool initialized;
        private int itemStacks;
        private VelocityRandomOnStart velocity;
        private BeginRapidlyActivatingAndDeactivating blink;

		[Tooltip("The base object to destroy when this pickup is consumed.")]
		public GameObject baseObject;

		[Tooltip("The team filter object which determines who can pick up this pack.")]
		public TeamFilter teamFilter;

		public GameObject pickupEffect;

		private bool alive = true;
	}
}
