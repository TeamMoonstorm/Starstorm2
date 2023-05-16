using RoR2;
using RoR2.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Moonstorm.Starstorm2.Components
{
    //Not good. None of it.
    public class PriceTooltipManager : NetworkBehaviour
    {
        private Transform icon;
        private Image image;
        private Sprite sprite;

        private GameObject zanzan;
        private TraderController zanzanController;

        private TooltipProvider tooltip;

        private ItemDef item;
        void Start()
        {
            icon = transform.Find("Icon");
            if (icon == null)
                return;

            image = icon.GetComponent<Image>();
            if (image == null)
                return;

            sprite = image.sprite;
            if (image == null)
                return;

            zanzan = GameObject.Find("TraderBody(Clone)");
            zanzanController = zanzan.GetComponent<TraderController>();
            if (zanzanController == null)
                return;

            tooltip = GetComponent<TooltipProvider>();
            if (tooltip == null)
                return;

            //fuck it we ball
            item = zanzanController.GetItemThroughSprite(sprite);
            if (item != null)
            {
                Debug.Log("item = " + item);
                float itemValue = zanzanController.itemValues[item];
                string quality;
                //these thresholds should be calculated but they're hard coded for now.
                if (itemValue >= 0.82f)
                {
                    quality = "SS2_TRADER_QUALITY_BEST";
                }
                else if (itemValue >= 0.62f)
                {
                    quality = "SS2_TRADER_QUALITY_GREAT";
                }
                else if (itemValue >= 0.33f)
                {
                    quality = "SS2_TRADER_QUALITY_GOOD";
                }
                else if (itemValue >= 0.11f)
                {
                    quality = "SS2_TRADER_QUALITY_MODERATE";
                }
                else
                {
                    quality = "SS2_TRADER_QUALITY_POOR";
                }

                tooltip.titleColor = ColorCatalog.GetColor(item.darkColorIndex);
                tooltip.titleToken = item.nameToken;
                string value = Mathf.Floor(zanzanController.itemValues[item] * 100).ToString();
                string combinedString = string.Format("{0}{1}{2}", value, "% - ", Language.GetStringFormatted(quality));
                tooltip.bodyToken = combinedString;
            }
        }
    }
}
