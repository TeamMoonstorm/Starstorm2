using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RoR2;
using RoR2.UI;
using SS2.Components;
namespace SS2.UI
{
    public class PedestalInfoPanelHelper : MonoBehaviour
    {
        public Image currentItemImage;

        private PickupPickerPanel panel;
        private PedestalBehavior pedestalBehavior;
        private PickupIndex currentPickup;
        private void Start()
        {
            panel = GetComponent<PickupPickerPanel>();
            pedestalBehavior = panel.pickerController.GetComponent<PedestalBehavior>();
            currentPickup = pedestalBehavior.GetPickup().pickupIndex;
            UpdateCurrentItem();
        }
        private void Update()
        {
            var pickupIndex = pedestalBehavior.GetPickup().pickupIndex;
            if (pickupIndex != PickupIndex.none && pickupIndex != currentPickup)
            {
                UpdateCurrentItem();
            }
        }
        public void UpdateCurrentItem()
        {
            currentItemImage.sprite = PickupCatalog.GetPickupDef(currentPickup).iconSprite;
        }
    }
}
