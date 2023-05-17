using RoR2;
using RoR2.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Moonstorm.Starstorm2.Components
{
    public class FavoriteTooltipManager : NetworkBehaviour
    {
        private ItemDef itemDef;

        private GameObject zanzan;
        private TraderController zanzanController;

        private LanguageTextMeshController ltmc;
        private void Start()
        {
            zanzan = GameObject.Find("TraderBody(Clone)");
            zanzanController = zanzan.GetComponent<TraderController>();
            if (zanzanController == null)
                return;

            ltmc = GetComponent<LanguageTextMeshController>();
            string combinedString = string.Format("{0}{1}{2}", Language.GetStringFormatted("SS2_TRADER_WANT_TEXT"), Language.GetStringFormatted(zanzanController.favoriteItem.nameToken), Language.GetStringFormatted("SS2_TRADER_WANT2_TEXT"));
            ltmc.token = combinedString;
        }
    }
}
