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
		public GameObject inspectPanel;
		public Image inspectVisual;
		public TextMeshProUGUI valueText;
		public TextMeshProUGUI specialText;
		public TextMeshProUGUI percentSymbol;
		public Image wantedItemImage;		
		public Color desiredItemOutline; // wanted desired favorite. in the same class. fucking reatrd
		public Color desiredItemHover;
		public Gradient valueGradient;
		public float minFontSize = 28f;
		public float maxFontSize = 48f;
		private PickupPickerPanel panel;
		private TraderController traderController;
		private PickupIndex favoriteItem;
		private void Start()
        {
			inspectPanel.SetActive(false);
			panel = base.GetComponent<PickupPickerPanel>();
			traderController = panel.pickerController.GetComponent<TraderController>();
			favoriteItem = traderController.favoriteItem;
			panel.SetPickupOptions(panel.pickupOptions); // LOL!!. cant get favoriteitem before options are set the first time. so we set them again.
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
			inspectPanel.SetActive(false);
			bool isSpecial = traderController.IsSpecial(pickupDef.pickupIndex);
			inspectVisual.sprite = pickupDef.iconSprite;
			float value = traderController.GetValue(pickupDef.pickupIndex);
			if (isSpecial) value = 999f; // "Special" or %999
			float t = Mathf.Clamp01(value / 200f);
			Color color = valueGradient.Evaluate(t);
			valueText.text = Mathf.FloorToInt(value).ToString();
			valueText.fontSize = Mathf.Lerp(minFontSize, maxFontSize, t);
			valueText.color = color;
			percentSymbol.color = color;			
			inspectPanel.SetActive(true);
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
			else if (traderController && traderController.IsSpecial(pickupDef.pickupIndex))
            {
				Image image = childLocator.FindChild("BaseOutline").GetComponent<Image>();
				image.color = valueGradient.Evaluate(999);
				image.pixelsPerUnitMultiplier = 0.75f;
				childLocator.FindChild("HoverOutline").GetComponent<Image>().color = valueGradient.Evaluate(999);
			}
		}

	}
}
