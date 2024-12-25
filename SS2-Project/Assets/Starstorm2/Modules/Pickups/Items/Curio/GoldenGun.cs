using RoR2;
using UnityEngine;
using RoR2.ContentManagement;
using R2API;
using RoR2.Items;
using RoR2.Orbs;
using System.Collections.Generic;
namespace SS2.Items
{
    public sealed class GoldenGun : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acGoldenGun", SS2Bundle.Items);
        public override bool IsAvailable(ContentPack contentPack) => true;
        static GameObject orbEffect;

        public override void Initialize()
        {
            orbEffect = SS2Assets.LoadAsset<GameObject>("EliteDamageOrbEffect", SS2Bundle.Items);
        }
        
        public class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = false)]
            private static ItemDef GetItemDef() => SS2Content.Items.GoldenGun;

            private static readonly Queue<GoldOrb> orbQueue = new Queue<GoldOrb>();
            private static float minInterval = 0.05f;
            private float stopwatch;
            private void FixedUpdate()
            {
                if (orbQueue.Count > 0)
                {
                    stopwatch += Time.fixedDeltaTime;
                    while (stopwatch > minInterval)
                    {
                        stopwatch -= minInterval;
                        GoldOrb orb = orbQueue.Dequeue();
                        OrbManager.instance.AddOrb(orb);
                    }
                }
            }
            public void OnDamageDealtServer(DamageReport damageReport)
            {
                DamageInfo damageInfo = damageReport.damageInfo;
                if (!damageInfo.damageType.IsDamageSourceSkillBased) return;
                uint gold = (uint)(Run.instance.GetDifficultyScaledCost(1) * damageInfo.procCoefficient);
                GoldOrb orb = new GoldOrb();
                orb.origin = damageInfo.position;
                orb.overrideDuration = 0.3f;
                orb.goldAmount = gold;
                orb.target = body.mainHurtBox;               
                orbQueue.Enqueue(orb);
            }
        }
    }
}
