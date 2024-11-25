using System;
using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Grumpy;
namespace SS2.Components
{
    // CoffeeBeanPickup HAS to be on a child object, but the pooling stuff needs the component to be on the root object
    public class CoffeeBeanPooler : NetworkComponentPoolObject
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
            if (CoffeeBeanPooler.PoolManager != null && forceNewInstantiation)
            {
                CoffeeBeanPooler.PoolManager.ResetPools();
                CoffeeBeanPooler.PoolManager = null;
            }
            if (CoffeeBeanPooler.PoolManager == null)
            {
                CoffeeBeanPooler.PoolManager = new NetworkedObjectPoolManager(1, 15, false, true);
            }
        }
        public static CoffeeBeanPooler GetPooledBean()
        {
            CoffeeBeanPooler.EnsurePoolManagerExists(false);
            return (CoffeeBeanPooler)CoffeeBeanPooler.PoolManager.GetPooledObject(beanPrefab); // gets object from pool, instantiates if not
        }
        public static void SpawnBean(Vector3 position, int itemStacks, TeamIndex team)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            CoffeeBeanPooler bean = GetPooledBean();
            if (!bean)
            {
                SS2Log.Error("CoffeeBeanPooler.SpawnBean(): rut roh");
                return;
            }
            bean.beanis.Initialize(itemStacks, team);
            bean.transform.position = position;
            bean.transform.rotation = UnityEngine.Random.rotation;
            bean.velocity.Start(); // LOOOOOOOOOOOOOOOOOL
            bean.blink.fixedAge = 0;
            bean.gravity.gravitateTarget = null;
            bean.timer = 0;
        }
        public override void PreReturnToPool()
        {
            beanis.initialized = false;
        }

        private void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if(timer > duration)
            {
                Cleanup();
            }
        }

        public void Cleanup()
        {
            this.PreReturnToPool();
            this.ReturnToPool();
        }

        public GravitatePickup gravity;
        public VelocityRandomOnStart velocity;
        public BeginRapidlyActivatingAndDeactivating blink;
        public CoffeeBeanPickup beanis;
        public float duration = 7f;
        private float timer;
    }
}
