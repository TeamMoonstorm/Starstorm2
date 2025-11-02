using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;

using MSU;
using System.Collections.Generic;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using SS2.Components;

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
            return SS2Config.enableBeta;
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
            bool needsSpawnAdjustment = false;
            float removalTimer = 0;
            float creationTimer = 0;
            float currentDesiredRatio;

            List<RainbowRootBranchInstance> branchInstances = new List<RainbowRootBranchInstance>();
            List<RainbowRootBranchInstance> branchStorage = new List<RainbowRootBranchInstance>();
            
            private void Awake()
            {
                base.Awake();
                Setup();
            }

            private void Setup()
            {
                totalHealth = body.maxHealth * branchHealth;
                hits = maxHits;
                charges = 3;
                needsVisualAdjustment = false;
                cd = 0;

                var ratio = 360 / charges;
                for (int i = 0; i < charges; ++i)
                {
                    float angle = ratio * i;
                    Vector3 vec = new Vector3(0, angle, 0);

                    var tempBranch = Instantiate(branchObject, body.gameObject.transform.position, Quaternion.Euler(vec), body.gameObject.transform);
                    var tempStruct = new RainbowRootBranchInstance
                    {
                        rotation = angle,
                        branch = tempBranch
                    };
                    branchStorage.Add(tempStruct);
                    branchInstances.Add(tempStruct);
                }

                needsSpawnAdjustment = true;

            }

            private void Refresh()
            {
                totalHealth = body.maxHealth * branchHealth;
                hits = maxHits;
                charges = 3;
                needsVisualAdjustment = false;
                cd = 0;

                for(int i = 0; i < branchInstances.Count; ++i)
                {
                    CleanupInstance(branchInstances[i]);
                }
                branchInstances.Clear();

                var ratio = 360 / charges;
                for (int i = 0; i < charges; ++i)
                {
                    float angle = ratio * i;
                    Vector3 vec = new Vector3(0, angle, 0);

                    var instance = branchStorage[i];
                    instance.rotation = angle;

                    var branch = instance.branch;
                    var child = branch.transform.GetChild(0);

                    child.transform.rotation = Quaternion.identity;

                    branch.transform.rotation = Quaternion.Euler(vec);
                    branch.transform.localScale = Vector3.zero;

                    branch.SetActive(true);
                    if(child.TryGetComponent<RotateObject>(out var rotate))
                    {
                        rotate.enabled = true;
                    }
                    branchInstances.Add(instance);
                }

                needsSpawnAdjustment = true;

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
                    //SS2Log.Info("cd: " + cd);
                    if (cd > (20 / MSUtil.InverseHyperbolicScaling(1, .5f, 10, stack)))
                    {
                        Refresh();
                    }
                }

                if(needsVisualAdjustment)
                {
                    removalTimer += Time.deltaTime;
                    for (int i = 0; i < branchInstances.Count; ++i)
                    {
                        float current = branchInstances[i].rotation;
                        var result = Mathf.Lerp(current, currentDesiredRatio * i, removalTimer);
                        branchInstances[i].branch.transform.rotation = Quaternion.Euler(new Vector3(0, result, 0));
                    }

                    if (removalTimer > 1)
                    {
                        needsVisualAdjustment = false;
                        removalTimer = 0;
                        for (int i = 0; i < branchInstances.Count; ++i)
                        {
                            var calculated = currentDesiredRatio * i;
                            var instance = branchInstances[i];
                            instance.branch.transform.rotation = Quaternion.Euler(new Vector3(0, calculated, 0));
                            instance.rotation = calculated;
                        }
                    }
                }

                if (needsSpawnAdjustment)
                {
                    creationTimer += Time.deltaTime;
                    for (int i = 0; i < branchInstances.Count; ++i)
                    {
                        var result = Mathf.Lerp(0, 1, creationTimer);
                        var instance = branchInstances[i];
                        instance.branch.transform.localScale = new Vector3(result, result, result);
                    }

                    if (creationTimer > 1)
                    {
                        needsSpawnAdjustment = false;
                        creationTimer = 0;
                        for (int i = 0; i < branchInstances.Count; ++i)
                        {
                            var instance = branchInstances[i];
                            instance.branch.transform.localScale = new Vector3(1, 1, 1);
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

                        CleanupInstance(branchInstances[0]);

                        branchInstances.RemoveAt(0);

                        if(charges != 0)
                        {
                            needsVisualAdjustment = true;
                            currentDesiredRatio = 360 / charges;
                        }
                    }
                }
            }

            public void CleanupInstance(RainbowRootBranchInstance instance)
            {
                instance.rotation = 0;
                var branch = instance.branch;

                var child = instance.branch.transform.GetChild(0);
                if (child)
                {
                    var rotate = child.GetComponent<RotateObject>();
                    rotate.enabled = false;
                    child.transform.rotation = Quaternion.identity;

                }

                branch.SetActive(false);
                branch.transform.rotation = Quaternion.identity;
                branch.transform.localScale = Vector3.zero;
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
