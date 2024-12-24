using MSU;
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using RoR2.ContentManagement;
using RoR2.Orbs;
namespace SS2.Items
{
    public sealed class SnakeEyes : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acSnakeEyes", SS2Bundle.Items);
        public override bool IsAvailable(ContentPack contentPack) => true;
        public static GameObject orbEffect;
        public override void Initialize()
        {
            orbEffect = SS2Assets.LoadAsset<GameObject>("CritOnShrineOrbEffect", SS2Bundle.Items);
            RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += OnInteractionBegin;
        }
        private void OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            orig(self, activator);
            if (self.isShrine && activator && activator.TryGetComponent(out CharacterBody body) && body.inventory)
            {
                int crit = body.inventory.GetItemCount(SS2Content.Items.SnakeEyes);
                if (crit > 0)
                {
                    GrantItemOrb orb = new GrantItemOrb();
                    orb.origin = self.transform.position;
                    orb.target = Util.FindBodyMainHurtBox(body);
                    orb.item = SS2Content.Items.StackSnakeEyes.itemIndex;
                    orb.count = 3 + 2 * crit;
                    orb.effectPrefab = orbEffect;
                    OrbManager.instance.AddOrb(orb);
                }
            }
        }
        private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (!sender.inventory) return;
            args.critAdd += sender.inventory.GetItemCount(SS2Content.Items.StackSnakeEyes);
            if (sender.crit > 100)
            {
                // it will take a second recalcstats to calculate this correctly
                // you could do sender.crit + itemCount to fix the first recalcstats, but it would mean the crit dmg bonus is doubled afterwards
                // i stupid
                args.critDamageMultAdd += sender.crit - 100;
            }

        }

    }
}
