using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SS2.Equipments
{
    public sealed class DivineRight : SS2Equipment
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<EquipmentAssetCollection>("acDivineRight", SS2Bundle.Indev);

        public override bool Execute(EquipmentSlot slot)
        {
            //TO-DO:
            //prefab: should probably be a component that parents to body like microbots rather than an item display.
            //reasoning for above being to ensure compatability with every character without setup

            //code: make prefab run an entitystate for the swing animation and just play a blast attack from body..?
            //is it possible to run an actual melee attack off of the prefab i wonder?
            //dreading this.

            //^ ok shut the fuck up loser new idea:
            //the sword itself is a companion you get. and the equipment makes it fly towards a target and slice it.
            //maybe redo it like that later. i think this is good for now.

            CharacterModel model = slot.characterBody.modelLocator.modelTransform.GetComponent<CharacterModel>();
            List<GameObject> displayList;
            GameObject displayObject;
            GameObject followerObject;
            Animator animator;

            if (model != null)
            {
                displayList = model.GetEquipmentDisplayObjects(SS2Content.Equipments.equipDivineRight.equipmentIndex);
                if (displayList != null)
                {
                    displayObject = displayList[0];
                    if (displayObject != null)
                    {
                        followerObject = displayObject.GetComponent<ItemFollower>().followerInstance;
                        if (followerObject != null)
                        {
                            animator = followerObject.GetComponent<ChildLocator>().FindChild("Model").gameObject.GetComponent<Animator>();
                            animator.Play("Swing");
                        }
                    }
                }
            }

            return true;
        }

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }
    }
}
