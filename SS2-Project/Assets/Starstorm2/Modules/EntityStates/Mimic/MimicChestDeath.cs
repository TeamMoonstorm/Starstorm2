using RoR2;
using SS2;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Mimic
{
    public class MimicChestDeath : GenericCharacterDeath
    {
        private List<ItemIndex> itemInd;

        public override void OnEnter()
        {
            base.OnEnter();
            itemInd = characterBody.inventory.itemAcquisitionOrder;
            SS2Log.Warning("Death On Enter : " + itemInd.Count);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        { 
            var pivot = FindModelChild("PickupPivot");
            SS2Log.Warning("Death Exit : " + pivot + " | " + itemInd.Count);
            if (itemInd.Count > 0 && pivot)
            {
                var dir = characterDirection.forward;
                var angle = 90 / itemInd.Count;
    
                Vector3 vec = Vector3.up * 5 + dir * 5;
                Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
                
                foreach (var ind in itemInd)
                {
                    var pind = RoR2.PickupCatalog.FindPickupIndex(ind);                    
                    PickupDropletController.CreatePickupDroplet(pind, pivot.position, vec);
                    vec = rot * vec;
                }
            }
            base.OnExit();
        }
    }
}
