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
        //protected RefabricatorInteractionToken refabController;
        //protected PurchaseInteraction purchaseInter;

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
            SS2Log.Info("idle enter");
            //refabController = GetComponent<RefabricatorInteractionToken>();
            //purchaseInter = GetComponent<PurchaseInteraction>();
            //purchaseInter.SetAvailable(enableInteraction);
        }


        // Start is called before the first frame update
        //void Start()
        //{
        //    
        //}
        //
        //// Update is called once per frame
        //void Update()
        //{
        //    
        //}
    }
}
