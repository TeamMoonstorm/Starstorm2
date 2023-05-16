using RoR2;
using RoR2.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Moonstorm.Starstorm2.Components
{
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
            if (icon != null)
                Debug.Log("found icon");
            else
                return;

            image = icon.GetComponent<Image>();
            if (image != null)
                Debug.Log("found image");
            else
                return;

            sprite = image.sprite;
            if (image != null)
                Debug.Log("found sprite: " + sprite.name);
            else
                return;

            zanzan = GameObject.Find("TraderBody(Clone)");
            zanzanController = zanzan.GetComponent<TraderController>();
            if (zanzanController != null)
                Debug.Log("found zanzan controller");

            tooltip = GetComponent<TooltipProvider>();
            if (tooltip != null)
                Debug.Log("found tooltip");

            item = zanzanController.GetItemThroughSprite(sprite);
            if (item != null)
            {
                Debug.Log("item = " + item);
                tooltip.titleColor = Color.white;
                tooltip.titleToken = item.nameToken;
                tooltip.bodyToken = zanzanController.itemValues[item].ToString();
            }
        }
    }
}
