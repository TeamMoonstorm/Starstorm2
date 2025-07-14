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
        public Transform pickupPivot;
        public CharacterDirection cdir;
        public BasicPickupDropTable dropTable;

        public void Start()
        {
            if (!pickupPivot || !cdir)
            {
                FindPivot();
            }

            var item = dropTable.GenerateDropPreReplacement(Run.instance.treasureRng);
            var itemIndex = PickupCatalog.GetPickupDef(item).itemIndex;
            var def = ItemCatalog.GetItemDef(itemIndex);

            if(def.DoesNotContainTag(ItemTag.AIBlacklist))
            {
                this.GetComponent<CharacterBody>().inventory.GiveItem(itemIndex);
            }

             AddItem(itemIndex);
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
                    pickupPivot = cloc.FindChild("PickupPivot");
                }
            }
        }

        public void AddItem(ItemIndex ind)
        {
            itemInd.Add(ind);
        }

        public void BeginDropCountdown()
        {
            StartCoroutine(DropItems());
        }

        IEnumerator DropItems()
        {
            if (!pickupPivot || !cdir)
            {
                FindPivot();
            }

            yield return new WaitForSeconds(1.5f);

            var temp = pickupPivot.position;
            EffectData effectData = new EffectData
            {
                origin = temp
            };
            //effectData.SetNetworkedObjectReference(pickupPivot.gameObject);
            EffectManager.SpawnEffect(SS2.Monsters.Mimic.itemStarburst, effectData, true);

            if (itemInd.Count > 0 && pickupPivot && cdir)
            {
                var dir = cdir.forward;
                var angle = 90 / itemInd.Count;

                Vector3 vec = Vector3.up * 5 + dir * 5;
                Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);

                foreach (var ind in itemInd)
                {
                    var pind = RoR2.PickupCatalog.FindPickupIndex(ind);
                    PickupDropletController.CreatePickupDroplet(pind, pickupPivot.position, vec);
                    vec = rot * vec;
                }
            }
        }
    }
}
