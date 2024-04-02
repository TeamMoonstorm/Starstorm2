using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace SS2.Items
{
    public sealed class ShackledLamp : SS2Item, IContentPackModifier
    {
        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;
        public ItemDef.Pair lampPair;

        private static GameObject projectilePrefab;

        public override void Initialize()
        {    //something comes up null when doing this
             //it should probably be its own base or some shit for future additions anyway
             //will investigate more soon:tm:

            //so i just did it basically exactly how i did it in vv and it worked 
            //SS2Log.Info("my tier is " + ItemDef.tier);

            //What did lilith meant by this...? Ah, remind me to add a utility for this.
            On.RoR2.Items.ContagiousItemManager.Init += AddZoeaPair;
        }

        private void AddZoeaPair(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            List<ItemDef.Pair> newVoidPairs = new List<ItemDef.Pair>();

            ItemDef.Pair lampPair = new ItemDef.Pair()
            {
                itemDef1 = _itemDef, //nonvoid
                itemDef2 = RoR2.DLC1Content.Items.VoidMegaCrabItem //void
            };
            newVoidPairs.Add(lampPair);

            var voidPairs = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem];
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = voidPairs.Union(newVoidPairs).ToArray();
            orig();
        }

        public override bool IsAvailable()
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var helper = new ParallelAssetLoadCoroutineHelper();

            helper.AddAssetToLoad<ItemDef>("ShackledLamp", SS2Bundle.Items);
            helper.AddAssetToLoad<GameObject>("LampBulletPlayer", SS2Bundle.Items);

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            _itemDef = helper.GetLoadedAsset<ItemDef>("ShackledLamp");
            projectilePrefab = helper.GetLoadedAsset<GameObject>("LampBulletPlayer");
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.projectilePrefabs.AddSingle(projectilePrefab);
        }

        public sealed class LampBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.ShackledLamp;

            
            private float attackCounter;

            private List<GameObject> lampDisplay;
            private Transform displayPos;
            private ItemFollower follower;
            private bool tried = false;

            private void Start()
            {
                displayPos = null;
                body.onSkillActivatedAuthority += ChainEffect;
                lampDisplay = body.modelLocator.modelTransform.GetComponent<CharacterModel>().GetItemDisplayObjects(SS2Content.Items.ShackledLamp.itemIndex);
            }

            private void ChainEffect(GenericSkill skill)
            {
                if (!body.skillLocator.primary.Equals(skill))
                    return;

                //if (skill.skillDef.skillIndex == SkillCatalog.FindSkillIndexByName("ExecutionerFireIonGun"))
                //    return;
                //sorry nebby
                //:sob: -N
                if (skill.skillNameToken == "SS2_EXECUTIONER2_IONBURST_NAME")
                    return;

                IncrementFire();
            }

            public void IncrementFire()
            {
                attackCounter++;
                if (attackCounter >= 5)
                {
                    attackCounter %= 5f;
                    Util.PlayAttackSpeedSound("LampBullet", gameObject, body.attackSpeed);
                    float damage = body.damage * (2f + stack);
                    Vector3 muzzlePos = body.inputBank.aimOrigin;
                    SS2Log.Info("lampDisplay.Count: " + lampDisplay.Count);
                    if (displayPos != null)
                    {
                        muzzlePos = displayPos.position;
                    }
                    else if (lampDisplay.Count != 0)
                    {
                        follower = lampDisplay[0].GetComponent<ItemFollower>();
                        displayPos = follower.followerInstance.transform.Find("mdlLamp").transform;
                        muzzlePos = displayPos.position;
                    }

                    ProjectileManager.instance.FireProjectile(
                        projectilePrefab,
                        muzzlePos,
                        Util.QuaternionSafeLookRotation(body.inputBank.aimDirection),
                        body.gameObject,
                        damage,
                        80f,
                        Util.CheckRoll(body.crit, body.master));
                }
            }


            private void OnDestroy()
            {
                body.onSkillActivatedAuthority -= ChainEffect;
            }
        }
    }
}
