using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates;
namespace SS2.Items
{
    public sealed class AffixStorm : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acAffixStorm", SS2Bundle.Equipments);

        public override void Initialize()
        {
            BuffOverlays.AddBuffOverlay(AssetCollection.FindAsset<BuffDef>("BuffAffixStorm"), SS2Assets.LoadAsset<Material>("matAffixLightning", SS2Bundle.Equipments));
        }
        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class BodyBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.AffixStorm;

            private static SerializableEntityStateType deathStateType = new SerializableEntityStateType(typeof(EntityStates.AffixStorm.DeathState));
            private CharacterDeathBehavior deathBehavior;
            private SerializableEntityStateType originalDeathState;
            public void Start()
            {
                if (NetworkServer.active)
                {
                    body.AddBuff(SS2Content.Buffs.BuffAffixStorm);


                    // FOR TESTING, JUST TO MAKE THEM STRONGER. MAKE A REAL EFFECT LATER(?)
                    body.inventory.GiveItem(SS2Content.Items.BoostMovespeed, 25);
                    body.inventory.GiveItem(SS2Content.Items.BoostCharacterSize, 15);
                    //body.inventory.GiveItem(RoR2Content.Items.BoostHp, 25);
                }

                // If something else is going to override death states, then we need to make a skilloverride equivalent system.
                deathBehavior = base.GetComponent<CharacterDeathBehavior>();
                if(deathBehavior)
                {
                    deathBehavior.deathState = deathStateType;
                }
            }

            private void OnDestroy()
            {
                if (NetworkServer.active)
                {
                    body.RemoveBuff(SS2Content.Buffs.BuffAffixStorm);
                }
                if(deathBehavior)
                {
                    deathBehavior.deathState = originalDeathState;
                }
            }

        }
    }
}
