using EntityStates.DroneTable;
using Moonstorm.Starstorm2;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static Moonstorm.Starstorm2.Interactables.DroneTable;

namespace EntityStates.DroneTable
{
    public class Idle : DroneTableBaseState
    {

        protected override bool enableInteraction
        {
            get
            {
                return true;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            var scrapRoot = this.gameObject.transform.Find("ScrapRoot").gameObject;
            scrapRoot.SetActive(false);
        }
    }
}
