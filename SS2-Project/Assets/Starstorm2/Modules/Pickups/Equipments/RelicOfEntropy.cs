using MSU.Config;
using MSU;
using R2API;
using SS2.Equipments;
using SS2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MSU.BaseBuffBehaviour;
using UnityEngine.Networking;
using UnityEngine;
using RoR2;
using RoR2.ContentManagement;

namespace Assets.Starstorm2.Modules.Pickups.Equipments
{
    public class RelicOfEntropy : SS2Equipment
    {
        private const string token = "SS2_EQUIP_RELICOFENTROPY_DESC";

        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<EquipmentAssetCollection>("acRelicOfEntropy", SS2Bundle.Equipments);

        public override bool Execute(EquipmentSlot slot)
        {
           

            return true;
        }

        public override void Initialize()
        {

        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }
    }
}
