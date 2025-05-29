using RoR2;
using SS2;
using SS2.Components;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Mimic
{
    public class MimicChestDeath : GenericCharacterDeath
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if (gameObject)
            {
                if (gameObject)
                {
                    var mim = gameObject.GetComponent<MimicInventoryManager>();
                    SS2Log.Error("Death enter : " + mim + " | ");
                    if (mim)
                    {
                        SS2Log.Error("calling : " + mim + " | ");
                        mim.BeginCountdown();
                    }
                    //Util.PlaySound("Play_UI_chest_unlock", gameObject);
                }
                //Util.PlaySound("Play_UI_chest_unlock", gameObject);
            }
            //itemInd = characterBody.inventory.itemAcquisitionOrder;

            //character
            //SS2Log.Warning("Death On Enter : " + itemInd.Count);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            SS2Log.Error("Death Exit : " + gameObject);

            base.OnExit();
        }
    }
}
