using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;

using MSU;
using System.Collections.Generic;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace SS2.Items
{
    public sealed class RainbowRoot : SS2Item
    {
        private const string token = "SS2_ITEM_RAINBOWROOT_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acRainbowRoot", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Amount of armor gained upon pickup. Does not scale with stack count. (1 = 1 armor)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float baseArmor = 20;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Base portion of damage prevented to be gained as barrier. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float baseAmount = .25f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Portion of damage prevented to be gained as barrier per stack. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float scalingAmount = .15f;


        public override void Initialize()
        {

        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class Behavior : BaseItemBodyBehavior, IOnTakeDamageServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RainbowRoot;

            float totalHealth;
            int hits;
            int charges;
            float cd;

            private void Awake()
            {
                base.Awake();
                Refresh();
            }

            private void Refresh()
            {

                totalHealth = body.maxHealth * .25f;
                hits = 20;
                charges = 3;
                SS2Log.Info("Refresh  " + totalHealth + " | " + hits + " | " + charges + " | " + cd);
                cd = 0;

            }

            private void FixedUpdate()
            {
                if (charges <= 0)
                {
                    cd += Time.deltaTime;
                    SS2Log.Info("cd: " + cd);
                    if (cd > (15 / MSUtil.InverseHyperbolicScaling(1, .5f, 15, stack)))
                    {
                        SS2Log.Info("Refresh at: " + cd);
                        Refresh();
                    }
                }
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (charges > 0)
                {
                    SS2Log.Info("AAAAAAAAAAAAAAA " + damageReport.damageInfo.damage + " | " + totalHealth + " | " + hits);
                    totalHealth -= damageReport.damageInfo.damage;
                    --hits;
                    SS2Log.Info("2 " + damageReport.damageInfo.damage + " | " + totalHealth + " | " + hits);
                    if (hits <= 0 || totalHealth <= 0)
                    {
                        --charges;
                        hits = 20;
                        totalHealth = body.maxHealth * .25f;
                        SS2Log.Info("RESETING  " + damageReport.damageInfo.damage + " | " + totalHealth + " | " + hits + " | " + charges);
                        body.healthComponent.AddBarrierAuthority(body.maxHealth * .25f);
                    }
                }
            }
        }

    }
}


/* 
what if instead its jobs were split up into an “object” that stores that taken damage, then “cashes” it in as barrier, then recharging like void teddies or opal

on pickup (or when recharged) -
spawn 3 branches around you (styled like rainbow root, orbiting)

absorbs a percentage of damage you take and puts it into a “bucket”
branches each can absorb either 25% of your max hp or 20 hits, and then breaks. i dont think it needs a ui maybe the branch could have model swaps to frailer models

breaking “cashes in” the bucket, granting all of it as barrier. 
recharges over time once all 3 branches break

i think this enables you to have an early game “total damage” mitigation item, as opposed to e.g. RAP which is a chip damage mitigation mostly, which then cashes itself in to then supply “all” of that 
barrier at once, in bursts of 3. 
the three branches dont need any logic or anything to determjne which one takes the damage first or anything, you just have branch 1, 2, and 3, and when all 3 break it stops to recharge
it’s a little complex as a white but i think if it’s described simply enough it can make sense. stacking could make the recharge quicker instead of potentially making a single break give 200% hp as 
barrier or something, which still enables sloppy play to be punished 
*/
