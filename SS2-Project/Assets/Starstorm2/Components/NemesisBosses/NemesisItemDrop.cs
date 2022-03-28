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
            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(itemDef.itemIndex), body.corePosition, Vector3.up);
        }
    }
}
