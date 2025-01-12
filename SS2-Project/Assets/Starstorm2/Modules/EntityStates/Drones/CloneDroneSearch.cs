using System.Collections.Generic;
using UnityEngine;
using RoR2;
using HG;
using UnityEngine.Networking;
namespace EntityStates.CloneDrone
{
    public class CloneDroneSearch : BaseSkillState
    {
        public static float duration = 0.6f;
        //public static float damageCoefficient = 1.0f;
        public static float radius = 18f;
        public static string muzzleString = "Muzzle";
        public static GameObject blastEffect;
        private ChildLocator childLocator;
        private SphereSearch sphereSearch;
        public GenericPickupController gpc;
        public ItemTier pickupTier;

        public override void OnEnter()
        {
            base.OnEnter();
            //Animator = GetModelAnimator(); does this thing even need an animator?
            childLocator = GetModelChildLocator();

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
        public void Search()
        {
            List<Collider> list = CollectionPool<Collider, List<Collider>>.RentCollection();
            sphereSearch.mask = LayerIndex.CommonMasks.interactable;
            sphereSearch.ClearCandidates();
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByColliderEntities();
            sphereSearch.OrderCandidatesByDistance();
            sphereSearch.FilterCandidatesByDistinctColliderEntities();
            sphereSearch.GetColliders(list);
            List<GameObject> objectList;
            objectList = new List<GameObject>();

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
                if (tempGPC && (!obj.TryGetComponent<SS2.Interactables.CloneDroneDamaged.ClonedPickup>(out var c) || c.CanBeCloned(base.gameObject)) && IsWhitelistTier(tempGPC.pickupIndex.pickupDef))
                {
                    float distance = Vector3.Distance(transform.position, obj.transform.position);
                    //if no closest object or obj is closer than old closest object
                    if (closestObject == null || distance < Vector3.Distance(transform.position, closestObject.transform.position))
                    {
                        closestObject = obj;
                    }                                        
                }
            }

            if (closestObject != null)
            {
                gpc = closestObject.GetComponent<GenericPickupController>();
                Clone();
            }
            else
                outer.SetNextStateToMain();
        }

        public bool IsWhitelistTier(PickupDef pickupDef)
        {
            if (pickupTier == ItemTier.Boss) 
            {
                Debug.Log(pickupDef);

                if (PickupCatalog.FindPickupIndex(pickupDef.artifactIndex) == PickupIndex.none)
                {
                    return true;
                }

                return false;
            }
            else if (pickupTier == ItemTier.Tier1 || pickupTier == ItemTier.Tier2 || pickupTier == ItemTier.Tier3 || pickupTier == ItemTier.VoidTier1 || pickupTier == ItemTier.VoidTier2 || pickupTier == ItemTier.VoidTier3 || pickupTier == ItemTier.VoidBoss)
            {
                return true;
            }
            else
            {
                return false;
            } 
        }

        public override void OnExit()
        {
            base.OnExit();
            if (gpc != null)
            {
                //Clone();
            }
        }

        public void Clone()
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
