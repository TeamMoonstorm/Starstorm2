using RoR2;
using SS2;
using UnityEngine;
namespace SS2.Components
{
    public class NemesisItemDrop : MonoBehaviour, IOnKilledServerReceiver
    {
        public ItemDef itemDef;

        private CharacterBody body;
        private void Awake()
        {
            if (!TryGetComponent(out body))
            {
                SS2Log.Error("NemesisItemDrop.Awake: CharacterBody missing on " + gameObject.name);
            }
        }

        public void OnKilledServer(DamageReport damageReport)
        {
            if (!itemDef || !body)
                return;

            if (itemDef.deprecatedTier != ItemTier.NoTier)
            {
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(itemDef.itemIndex), body.corePosition, Vector3.up);
            }
        }
    }
}
