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

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "The portion of your health each branch has. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float branchHealth = .25f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "How many hits it takes to break a branch, regardless of branch health. (1 = 1 hit)")]
        [FormatToken(token, 1)]
        public static int maxHits = 20;

        public static GameObject branchObject;

        public override void Initialize()
        {
            branchObject = AssetCollection.FindAsset<GameObject>("RainbowRootBranch");
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class RootBehavior : BaseItemBodyBehavior, IOnTakeDamageServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RainbowRoot;

            float totalHealth;
            int hits;
            int charges;
            float cd;
            bool needsVisualAdjustment = false;
            float currentDesiredRatio;
            float visualTimer = 0;
            //List<GameObject> branches = new List<GameObject>();
            List<RainbowRootBranchInstance> branchInstances = new List<RainbowRootBranchInstance>();
            List<RainbowRootBranchInstance> branchAll = new List<RainbowRootBranchInstance>();
            
            private void Awake()
            {
                base.Awake();
                Refresh();
            }

            private void Refresh()
            {

                totalHealth = body.maxHealth * branchHealth;
                hits = maxHits;
                charges = 3;
                needsVisualAdjustment = false;
                SS2Log.Info("Refresh  " + totalHealth + " | " + hits + " | " + charges + " | " + cd);
                cd = 0;


                for(int i = 0; i < branchInstances.Count; ++i)
                {
                    Destroy(branchInstances[i].branch);
                }
                branchInstances.Clear();

                var ratio = 360 / charges;
                for (int i = 0; i < charges; ++i)
                {
                    Vector3 vec = new Vector3(0, ratio * i, 0);
                    var tempBranch = Instantiate(branchObject, body.gameObject.transform.position, Quaternion.Euler(vec), body.gameObject.transform);
                    var tempStruct = new RainbowRootBranchInstance
                    {
                        rotation = ratio * i,
                        branch = tempBranch
                    };
                    //branches.Add(tempBranch);
                    branchInstances.Add(tempStruct);
                }

            }

            private void FixedUpdate()
            {
                if (charges < 3)
                {
                    var mult = 1f;
                    if(charges <= 0)
                    {
                        mult = 2f;
                    }
                    cd += Time.deltaTime * mult;
                    SS2Log.Info("cd: " + cd);
                    if (cd > (20 / MSUtil.InverseHyperbolicScaling(1, .5f, 10, stack)))
                    {
                        SS2Log.Info("Refresh at: " + cd);
                        Refresh();
                    }
                }

                if(needsVisualAdjustment)
                {
                    visualTimer += Time.deltaTime;
                    for (int i = 0; i < branchInstances.Count; ++i)
                    {
                        float current = branchInstances[i].rotation;
                        var result = Mathf.Lerp(current, currentDesiredRatio * i, visualTimer);
                        branchInstances[i].branch.transform.rotation = Quaternion.Euler(new Vector3(0, result, 0));
                    }

                    if (visualTimer > 1)
                    {
                        needsVisualAdjustment = false;
                        visualTimer = 0;
                        for (int i = 0; i < branchInstances.Count; ++i)
                        {
                            var calculated = currentDesiredRatio * i;
                            var instance = branchInstances[i];
                            instance.branch.transform.rotation = Quaternion.Euler(new Vector3(0, calculated, 0));
                            instance.rotation = calculated;
                        }
                    }
                }
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (charges > 0)
                {
                    totalHealth -= damageReport.damageInfo.damage;
                    --hits;
                    if (hits <= 0 || totalHealth <= 0)
                    {
                        --charges;
                        hits = 20;
                        totalHealth = body.maxHealth * .25f;
                        body.healthComponent.AddBarrierAuthority(body.maxHealth * .25f);

                        Destroy(branchInstances[0].branch);
                        //branchInstances[0].branch.SetActive(false);
                        branchInstances.RemoveAt(0);

                        if(charges > 1)
                        {
                            needsVisualAdjustment = true;
                            currentDesiredRatio = 360 / charges;
                        }
                    }
                }
            }
        }

        public struct RainbowRootBranchInstance
        {
            public float rotation;
            public GameObject branch;
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
