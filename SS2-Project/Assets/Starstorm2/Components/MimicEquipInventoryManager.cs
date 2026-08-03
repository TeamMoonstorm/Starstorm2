using UnityEngine;
using RoR2;
using System.Collections.Generic;
using MSU;
using R2API;
using SS2.Monsters;
using UnityEngine.Networking;

namespace SS2.Components
{
    [RequireComponent(typeof(CharacterBody))]
    [RequireComponent(typeof(PurchaseInteraction))]
    public class MimicEquipInventoryManager : MonoBehaviour
    {
        private List<EquipmentIndex> equipmentIndex = new List<EquipmentIndex>();
        private CharacterBody characterBody;
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

            if (!NetworkServer.active) return;
            
            GlobalEventManager.onServerDamageDealt += PreventRechest;

            UniquePickup pickup = dropTable.GeneratePickupPreReplacement(Run.instance.treasureRng);
            EquipmentIndex equipIndex = PickupCatalog.GetPickupDef(pickup.pickupIndex)!.equipmentIndex;
            characterBody = GetComponent<CharacterBody>();
            
            if (characterBody && characterBody.inventory)
            {
                if (characterBody.inventory.GetItemCountEffective(RoR2Content.Items.UseAmbientLevel) <= 0)
                {
                    characterBody.inventory.GiveItemPermanent(RoR2Content.Items.UseAmbientLevel);
                }

                if (characterBody.inventory.currentEquipmentIndex == EquipmentIndex.None)
                {
                    characterBody.inventory.SetEquipmentIndexForSlot(equipIndex, 0);
                        
                    AddEquip(equipIndex);
                }
            }
        }

        private void PreventRechest(DamageReport obj)
        {
            if (obj.attackerBody != characterBody) return;
            rechestPreventionTime = 2.5f;
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

        public void AddEquip(EquipmentIndex ind)
        {
            equipmentIndex.Add(ind);
        }

        public void DropItems()
        {
            var temp = pickupPivot.position;
            EffectData effectData = new EffectData
            {
                origin = temp
            };

            EffectManager.SpawnEffect(SS2.Monsters.MimicEquip.itemStarburst, effectData, true);

            if (equipmentIndex.Count > 0 && pickupPivot && cdir)
            {
                var dir = cdir.forward;
                var angle = 90 / equipmentIndex.Count;

                Vector3 vec = Vector3.up * 5 + dir * 5;
                Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
                
                foreach (var ind in equipmentIndex)
                {
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
