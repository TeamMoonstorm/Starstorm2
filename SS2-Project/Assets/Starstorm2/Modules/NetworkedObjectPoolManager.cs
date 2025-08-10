using System;
using System.Collections.Generic;
using Grumpy;
using UnityEngine;
using UnityEngine.Networking;
using SS2.Components;
namespace SS2
{
	// copypasted ComponentPoolManager :(  i got confused
	public class NetworkedObjectPoolManager
	{
		public NetworkedObjectPoolManager()
		{
			this.StartingAmount = 1;
			this.MaximumAmount = 1;
			this.AddComponentIfMissing = true;
			this.AlwaysGrowable = false;
		}

		public NetworkedObjectPoolManager(int startingPoolNumber, int maximuimPoolNumber, bool addComponentIfMissing, bool alwaysGrowable)
		{
			this.StartingAmount = Mathf.Max(startingPoolNumber, 1);
			this.MaximumAmount = Mathf.Max(maximuimPoolNumber, 1);
			this.AddComponentIfMissing = addComponentIfMissing;
			this.AlwaysGrowable = alwaysGrowable;
		}

		public NetworkComponentPoolObject GetPooledObject(GameObject prefab)
		{
			this.CreatePoolIfNotPresent(prefab);
			NetworkedObjectPool<NetworkComponentPoolObject> pool = NetworkedObjectPoolManager._PoolDictionary[prefab];
			NetworkComponentPoolObject @object = pool.GetObject();
			if (@object != null)
			{
				@object.SetOwningPool(pool);
				@object.InitializeForPool();
				@object.gameObject.SetActive(true);
				return @object;
			}
			return null;
		}

		private void CreatePoolIfNotPresent(GameObject prefab)
		{
			if (!NetworkedObjectPoolManager._PoolDictionary.ContainsKey(prefab))
			{
				NetworkedObjectPool<NetworkComponentPoolObject> pool = new NetworkedObjectPool<NetworkComponentPoolObject>();
				pool.AlwaysGrowable = true;
				pool.AddComponentIfMissing = this.AddComponentIfMissing;
				pool.PrefabObject = prefab;
				pool.Initialize(this.MaximumAmount, this.StartingAmount);
				NetworkedObjectPoolManager._PoolDictionary.Add(prefab, pool);
			}
		}

		public void ResetPools()
		{
			foreach (GameObject key in NetworkedObjectPoolManager._PoolDictionary.Keys)
			{
				if (NetworkedObjectPoolManager._PoolDictionary[key] != null)
				{
					NetworkedObjectPoolManager._PoolDictionary[key].Reset(true);
				}
			}
			NetworkedObjectPoolManager._PoolDictionary.Clear();
		}

		public int StartingAmount = 1;

		public int MaximumAmount = 10;

		public bool AddComponentIfMissing;

		public bool AlwaysGrowable;

		public static Dictionary<GameObject, NetworkedObjectPool<NetworkComponentPoolObject>> _PoolDictionary = new Dictionary<GameObject, NetworkedObjectPool<NetworkComponentPoolObject>>();
	}
	public class NetworkedObjectPool<T> : PrefabComponentPool<T> where T : Component
	{
		protected override T CreateNewObject(bool inPoolEntry = true)
		{
			T t = base.CreateNewObject(inPoolEntry);
			if(!t)
            {
				SS2Log.Error($"NetworkedObjectPool<{t.name}>.CreateNewObject failed.");
				return null;
			}
			if (NetworkServer.active)
			{
				NetworkServer.Spawn(t.gameObject);
			}
			else
			{
				SS2Log.Warning($"NetworkedObjectPool<{t.name}>.CreateNewObject called on client.");
				return null;
			}
			return t;
		}
	}
}

