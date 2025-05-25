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
            SS2Log.Warning("Death On Enter");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            SS2Log.Warning("Death Fixed Update");
        }

        public override void OnExit()
        {
            SS2Log.Warning("Death Exit");
            if (itemInd.Count > 0)
            {
                var dir = characterDirection.forward;
                var angle = 90 / itemInd.Count;
    
                Vector3 vec = Vector3.up * 5 + dir * 5;
                Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
    
                foreach (var ind in itemInd)
                {
                    var pind = RoR2.PickupCatalog.FindPickupIndex(ind);
                    PickupDropletController.CreatePickupDroplet(pind, characterBody.modelLocator.modelTransform.GetComponent<ChildLocator>().FindChildGameObject("PickupPivot").transform.position, vec);
                    vec = rot * vec;
                }
            }
            base.OnExit();
        }
    }
}
