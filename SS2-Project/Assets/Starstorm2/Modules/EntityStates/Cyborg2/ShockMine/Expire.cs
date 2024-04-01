using RoR2;
using UnityEngine.Networking;
using UnityEngine;

namespace EntityStates.Cyborg2.ShockMine
{
    public class Expire : BaseShockMineState
	{
		protected override bool shouldStick => false;

		public static GameObject expireEffectPrefab; /////////////


		public override void OnEnter()
		{
			base.OnEnter();

			if (NetworkServer.active)
            {
				if (expireEffectPrefab)
				{
					EffectManager.SpawnEffect(expireEffectPrefab, new EffectData
					{
						origin = base.transform.position,
					}, true);
				}
			}
			

			Destroy(base.gameObject);
		}


		
	}
}
