using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RoR2;
using RoR2.UI;
using SS2.Components;
namespace SS2.UI
{
	public class TraderInfoPanelHelper : MonoBehaviour
	{
		[SerializeField]
		public InspectPanelController inspectPanelController;
		public TextMeshProUGUI valueText;
		public TextMeshProUGUI specialText;
		public TextMeshProUGUI percentSymbol;
		public Image wantedItemImage;		
		public Color desiredItemOutline;
		public Color desiredItemHover;
		public Gradient valueGradient;

		private PickupPickerPanel panel;
		private TraderController traderController;
		private PickupIndex favoriteItem;
		private void Start()
        {
			panel = base.GetComponent<PickupPickerPanel>();
			traderController = panel.pickerController.GetComponent<TraderController>();
			favoriteItem = traderController.favoriteItem;
			UpdateFavoriteItem();
        }
        private void Update()
        {
            if(traderController.favoriteItem != PickupIndex.none && traderController.favoriteItem != favoriteItem)
            {
				favoriteItem = traderController.favoriteItem;
				UpdateFavoriteItem();
            }
        }
		public void UpdateFavoriteItem()
        {
			wantedItemImage.sprite = PickupCatalog.GetPickupDef(favoriteItem).iconSprite;
        }
        public void ShowInfo(MPButton button, PickupDef pickupDef)
		{
			InspectInfo info = pickupDef;
			this.inspectPanelController.Show(info, false);
			bool isSpecial = traderController.IsSpecial(pickupDef.pickupIndex);
			valueText.enabled = !isSpecial;
			percentSymbol.enabled = !isSpecial;
			specialText.enabled = isSpecial;
			if(!isSpecial)
            {
				float value = traderController.GetValue(pickupDef.pickupIndex) / 200f;
				Color color = valueGradient.Evaluate(value);
				valueText.text = Mathf.FloorToInt(value).ToString();
				valueText.color = color;
				percentSymbol.color = color;
			}		
		}
		public void HighlightDesired(MPButton button, PickupDef pickupDef)
		{
			ChildLocator childLocator = button.GetComponent<ChildLocator>();
			if (pickupDef.pickupIndex == favoriteItem)
            {				
				Image image = childLocator.FindChild("BaseOutline").GetComponent<Image>();
				image.color = desiredItemOutline;
				image.pixelsPerUnitMultiplier = 0.75f; // thicker outline
				childLocator.FindChild("HoverOutline").GetComponent<Image>().color = desiredItemHover;
			}
		}

	}
}
