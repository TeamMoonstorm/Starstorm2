using UnityEngine;
using RoR2;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;

namespace SS2.Components
{
    [RequireComponent(typeof(CharacterBody))]
    [RequireComponent(typeof(PurchaseInteraction))]
    public class MimicEquipInventoryManager : MonoBehaviour
    {
        private List<EquipmentIndex> equipmentIndex = new List<EquipmentIndex>();
        public Transform pickupPivot;
        public CharacterDirection cdir;
        public BasicPickupDropTable dropTable;
        public float rechestPreventionTime = 2.5f;

        public void Start()
        {
            if (!pickupPivot || !cdir)
            {
                FindPivot();
            }

            if (NetworkServer.active)
            {
                var pickup = dropTable.GeneratePickupPreReplacement(Run.instance.treasureRng);
                var equipIndex = PickupCatalog.GetPickupDef(pickup.pickupIndex).equipmentIndex;
                var def = EquipmentCatalog.GetEquipmentDef(equipIndex);
                var cb = GetComponent<CharacterBody>();

                if (cb && cb.inventory)
                {
                    if (cb.inventory.GetItemCountEffective(RoR2Content.Items.UseAmbientLevel) <= 0)
                    {
                        cb.inventory.GiveItemPermanent(RoR2Content.Items.UseAmbientLevel);
                    }
#if DEBUG
                    SS2Log.Warning("giving items ");
#endif
                    if (cb.inventory.GetItemCountEffective(RoR2Content.Items.ExtraLifeConsumed) <= 0 && cb.inventory.GetItemCountEffective(DLC1Content.Items.ExtraLifeVoidConsumed) <= 0)
                    {
                        cb.inventory.SetEquipmentIndexForSlot(equipIndex, 0);
                        
                        AddItem(equipIndex);
                        SS2Log.Warning($"gave {def.name} item");
                    }
                }

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
                    pickupPivot = cloc.FindChild("PickupPivot");
                }
            }
        }

        private void FixedUpdate()
        {
            if(rechestPreventionTime > 0)
            {
                rechestPreventionTime -= Time.fixedDeltaTime;
            }
        }

        public void AddItem(EquipmentIndex ind)
        {
            equipmentIndex.Add(ind);
        }

        public void DropItems()
        {
            SS2Log.Warning("Dropping items");
            SS2Log.Warning($"lefnthj {equipmentIndex.Count}");

            var temp = pickupPivot.position;
            EffectData effectData = new EffectData
            {
                origin = temp
            };

            EffectManager.SpawnEffect(SS2.Monsters.MimicEquip.itemStarburst, effectData, true);

            if (equipmentIndex.Count > 0 && pickupPivot && cdir)
            {
                SS2Log.Warning("Dropping items 1");
                
                var dir = cdir.forward;
                var angle = 90 / equipmentIndex.Count;

                Vector3 vec = Vector3.up * 5 + dir * 5;
                Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
                
                foreach (var ind in equipmentIndex)
                {
                    SS2Log.Warning($"dropping item {EquipmentCatalog.GetEquipmentDef(ind).nameToken}");
                    var pind = RoR2.PickupCatalog.FindPickupIndex(ind);
                    PickupDropletController.CreatePickupDroplet(pind, pickupPivot.position, vec);
                    vec = rot * vec;
                }
            }

            var cb = GetComponent<CharacterBody>();
            if(cb && cb.inventory)
            {
                cb.inventory.CleanInventory();
            }
        }
    }
}
