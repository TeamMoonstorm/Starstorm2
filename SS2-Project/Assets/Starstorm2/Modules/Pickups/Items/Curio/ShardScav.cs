using MSU;
using R2API;
using RoR2;
using EntityStates.VoidCamp;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Items
{
	public sealed class ShardScav : SS2Item // move to zanzan
	{
		public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemDef>("ShardScav", SS2Bundle.Items);

		public override void Initialize()
		{

		}
		public override bool IsAvailable(ContentPack contentPack) => SS2Config.enableBeta;
	}
}
