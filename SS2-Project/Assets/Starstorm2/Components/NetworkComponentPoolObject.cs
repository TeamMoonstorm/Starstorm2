using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using Grumpy;
namespace SS2.Components
{
	public class NetworkComponentPoolObject : NetworkBehaviour
	{
		public virtual void SetOwningPool(NetworkedObjectPool<NetworkComponentPoolObject> inOwner)
		{
			this._OwningPool = inOwner;
		}

		public virtual void InitializeForPool()
		{
		}

		public virtual void PreReturnToPool()
		{
		}

		public virtual void ReturnToPool()
		{
			if (this._OwningPool != null)
			{
				this._OwningPool.ReturnObject(this);
				if (this._OwningPool.InUse.Contains(this))
				{
					SS2Log.Error("NetworkComponentPoolObject.ReturnToPool failed");
					return;
				}
			}
			else
			{
				Destroy(base.gameObject);
			}
		}

		private NetworkedObjectPool<NetworkComponentPoolObject> _OwningPool;
	}
}
