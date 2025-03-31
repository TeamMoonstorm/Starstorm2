using MSU;
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections.Generic;
using RoR2.Items;
namespace SS2.Items
{
    //jellyfish and larva mainly
    public sealed class NoSelfDamage : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemDef>("NoSelfDamage", SS2Bundle.Items);
        public override void Initialize()
        {
            On.EntityStates.JellyfishMonster.JellyNova.Detonate += JankAssHook;
        }
        // keeps it from killing itself since it null checks the components
        private void JankAssHook(On.EntityStates.JellyfishMonster.JellyNova.orig_Detonate orig, EntityStates.JellyfishMonster.JellyNova self)
        {
            if(self.characterBody && self.characterBody.inventory && self.characterBody.inventory.GetItemCount(SS2Content.Items.NoSelfDamage) > 0)
            {
                var hc = self.healthComponent;
                var ml = self.modelLocator;
                self.outer.commonComponents.healthComponent = null;
                self.outer.commonComponents.modelLocator = null;
                orig(self);
                self.outer.commonComponents.healthComponent = hc;
                self.outer.commonComponents.modelLocator = ml;
                return;
            }
            orig(self);
            
        }
        public class Behavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.NoSelfDamage;
            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (body.inventory && body.inventory.GetItemCount(SS2Content.Items.NoSelfDamage) > 0 && damageInfo.attacker == base.gameObject)
                {
                    damageInfo.rejected = true;
                }
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
