using UnityEngine;
using RoR2;
using System.Collections.Generic;
using System.Collections;

namespace SS2.Components
{
    [RequireComponent(typeof(CharacterBody))]
    [RequireComponent(typeof(PurchaseInteraction))]
    public class MimicInventoryManager : MonoBehaviour
    {
        private List<ItemIndex> itemInd = new List<ItemIndex>();
        public Transform pivot;
        public CharacterDirection cdir;

        public void Start()
        {
            if (!pivot || !cdir)
            {
                FindPivot();
            }
        }

        public void FindPivot()
        {
            var mloc = this.GetComponent<ModelLocator>();
            if (mloc && mloc.modelTransform)
            {
                var cloc = mloc.modelTransform.GetComponent<ChildLocator>();
                cdir = GetComponent<CharacterDirection>();
                if (cloc && cdir)
                {
                    pivot = cloc.FindChild("PickupPivot");
                }
            }
        }

        public void AddItem(ItemIndex ind)
        {
            SS2Log.Warning("Adding item " + ind + " | " + PickupCatalog.FindPickupIndex(ind) + " | " + PickupCatalog.GetPickupDef(PickupCatalog.FindPickupIndex(ind)));
            itemInd.Add(ind);
        }

        public void BeginCountdown()
        {
            StartCoroutine(DropItems());
        }

        IEnumerator DropItems()
        {
            if (!pivot || !cdir)
            {
                FindPivot();
            }
            SS2Log.Warning("begin waity");
            yield return new WaitForSeconds(1.5f);
            SS2Log.Warning("end wait");
            if (itemInd.Count > 0 && pivot && cdir)
            {
                SS2Log.Warning("real,n");
                var dir = cdir.forward;
                var angle = 90 / itemInd.Count;

                Vector3 vec = Vector3.up * 5 + dir * 5;
                Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);

                foreach (var ind in itemInd)
                {
                    SS2Log.Warning("droppuy");
                    var pind = RoR2.PickupCatalog.FindPickupIndex(ind);
                    PickupDropletController.CreatePickupDroplet(pind, pivot.position, vec);
                    vec = rot * vec;
                }
            }
        }
    }
}
