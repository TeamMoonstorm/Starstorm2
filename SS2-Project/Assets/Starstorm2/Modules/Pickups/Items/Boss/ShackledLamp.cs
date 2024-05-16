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
        public override SS2AssetRequest<ItemAssetCollection> AssetRequest<ItemAssetCollection>()
        {
            return SS2Assets.LoadAssetAsync<ItemAssetCollection>("acShackledLamp", SS2Bundle.Items);
        }
        public override void OnAssetCollectionLoaded(AssetCollection assetCollection)
        {
            projectilePrefab = assetCollection.FindAsset<GameObject>("LampBulletPlayer");
        }

        public ItemDef.Pair lampPair;

        private static GameObject projectilePrefab;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
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
