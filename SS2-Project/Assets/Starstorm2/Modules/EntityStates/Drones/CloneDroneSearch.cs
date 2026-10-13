using System.Collections.Generic;
using UnityEngine;
using RoR2;
using HG;
using MSU;
using SS2.Interactables;
using Starstorm2.Components;
using UnityEngine.Networking;
namespace EntityStates.CloneDrone
{
    public class CloneDroneSearch : BaseSkillState
    {
        private static float duration = 0.6f;
        //public static float damageCoefficient = 1.0f;
        private static float radius = 18f;
        public static string muzzleString = "Muzzle";
        public static GameObject blastEffect;
        private ChildLocator childLocator;
        private SphereSearch sphereSearch;
        private GenericPickupController gpc;
        private CloneDroneClonedPickups clonedPickupChecker;

        public override void OnEnter()
        {
            base.OnEnter();
            //Animator = GetModelAnimator(); does this thing even need an animator?
            childLocator = GetModelChildLocator();
            clonedPickupChecker = gameObject.GetComponent<CloneDroneClonedPickups>();

            if (NetworkServer.active)
            {
                sphereSearch = new SphereSearch();
                sphereSearch.origin = transform.position;
                sphereSearch.mask = LayerIndex.pickups.mask;
                sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.Collide;
                sphereSearch.radius = radius;
                Search();
            }
            
            //this doesn't work - intent is to prevent player from cloning the new clone.
            //unsure how to implement.
            //suffering.

            //looking back on this months later ... wtf was bro on about.
            //shit.
        }

        private void Search()
        {
            List<Collider> list = CollectionPool<Collider, List<Collider>>.RentCollection();
            sphereSearch.mask = LayerIndex.CommonMasks.interactable;
            sphereSearch.ClearCandidates();
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByColliderEntities();
            sphereSearch.OrderCandidatesByDistance();
            sphereSearch.FilterCandidatesByDistinctColliderEntities();
            sphereSearch.GetColliders(list);
            var objectList = new List<GameObject>();

            foreach (Collider c in list)
            {
                GameObject obj = c.transform.parent.gameObject;
                objectList.Add(obj);
            }

            GameObject closestObject = null;

            foreach (GameObject obj in objectList)
            {
                //bypass non-items
                var tempGPC = obj.GetComponent<GenericPickupController>();
                if (tempGPC && !tempGPC.Duplicated && !tempGPC._pickupState.isRental && !tempGPC._pickupState.isTempItem && (IsWhitelistTier(tempGPC._pickupState.pickupIndex.pickupDef) || tempGPC._pickupState.pickupIndex.pickupDef.equipmentIndex != EquipmentIndex.None) && clonedPickupChecker.CanBeCloned(tempGPC.gameObject))
                {
                    float distance = Vector3.Distance(transform.position, obj.transform.position);
                    //if no closest object or obj is closer than old closest object
                    if (!closestObject || distance < Vector3.Distance(transform.position, closestObject.transform.position))
                    {
                        closestObject = obj;
                    }                                        
                }
            }

            if (closestObject)
            {
                gpc = closestObject.GetComponent<GenericPickupController>();
                Clone();
            }
            else
                outer.SetNextStateToMain();
        }

        private bool IsWhitelistTier(PickupDef pickupDef)
        {
            if (pickupDef == null)
                return false;

            return pickupDef.itemTier == ItemTier.Tier1 || 
                   pickupDef.itemTier == ItemTier.Tier2 || 
                   pickupDef.itemTier == ItemTier.Tier3 || 
                   pickupDef.itemTier == ItemTier.Boss || 
                   pickupDef.itemTier == ItemTier.VoidTier1 || 
                   pickupDef.itemTier == ItemTier.VoidTier2 || 
                   pickupDef.itemTier == ItemTier.VoidTier3 || 
                   pickupDef.itemTier == ItemTier.VoidBoss || 
                   pickupDef.itemTier == ItemTier.Cursed || 
                   pickupDef.itemTier == ItemTier.Hex || 
                   pickupDef.itemTier == ItemTier.FoodTier;
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void Clone()
        {
            CloneDroneCook nextState = new CloneDroneCook();
            nextState.target = gpc.gameObject;
            outer.SetNextState(nextState); 
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= duration)
                outer.SetNextStateToMain();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
