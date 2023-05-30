using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    public class NemesisItemDrop : MonoBehaviour, IOnKilledServerReceiver
    {
        public ItemDef itemDef;

        private CharacterBody body;
        private void Awake()
        {
            body = GetComponent<CharacterBody>();
        }

        public void OnKilledServer(DamageReport damageReport)
        {
            //SS2Log.Info("itemdef: " + itemDef.name + " | " + itemDef.tier + " | " + itemDef.deprecatedTier + " | " + (int)itemDef.deprecatedTier + " | " + itemDef._itemTierDef + " | "); 

            //itemdef: StirringSoul | 11 | AssignedAtRuntime | 10 | Sibylline (RoR2.ItemTierDef) | (enabled)
            //itemdef: StirringSoul | 11 | NoTier | 5 | Sibylline (RoR2.ItemTierDef) | (disabled) 

            //SS2Log.Info(itemDef.name + " stuff: " + (int)itemDef.deprecatedTier + " | " + (int)ItemTier.NoTier);
            if (itemDef.deprecatedTier != ItemTier.NoTier) //sorry using itemDef.tier doesnt change when the item gets notiered :(
            {
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(itemDef.itemIndex), body.corePosition, Vector3.up);
            }
            else
            {
                //SS2Log.Info("Item is disabled: " + itemDef.nameToken);
            }
            //PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(itemDef.itemIndex), body.corePosition, Vector3.up);
        }
    }
}
