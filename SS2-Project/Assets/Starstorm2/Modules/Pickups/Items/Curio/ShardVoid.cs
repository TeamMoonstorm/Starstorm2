using MSU;
using R2API;
using RoR2;
using EntityStates.VoidCamp;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Items
{
    public sealed class ShardVoid : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemDef>("ShardVoid", SS2Bundle.Items);

        public override void Initialize()
        {
            On.EntityStates.VoidCamp.Deactivate.OnEnter += SpawnVoidShard;
        }

        private void SpawnVoidShard(On.EntityStates.VoidCamp.Deactivate.orig_OnEnter orig, Deactivate self)
        {
            orig(self);
			if (NetworkServer.active && Run.instance)
			{
				Transform transform = self.GetModelChildLocator().FindChild("RewardSpawnTarget");
				int participatingPlayerCount = Run.instance.participatingPlayerCount;
				if (participatingPlayerCount > 0 && transform && self.rewardDropTable)
				{
					int num = participatingPlayerCount;
					float angle = 360f / (float)num;
					Vector3 vector = Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
					Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
					Vector3 position = transform.position;					
					int i = 0;
					while (i < num)
					{
						PickupDropletController.CreatePickupDroplet(new GenericPickupController.CreatePickupInfo
						{
							pickupIndex = PickupCatalog.FindPickupIndex(base.ItemDef.itemIndex),
							rotation = Quaternion.identity,
							position = position
						}, position, vector);
						i++;
						vector = rotation * vector;
					}
				}
			}
		}

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
