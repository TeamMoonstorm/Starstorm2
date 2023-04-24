using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using Moonstorm.Starstorm2;
using UnityEngine.Networking;
using HG;

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

        public override void OnEnter()
        {
            base.OnEnter();
            //Animator = GetModelAnimator(); does this thing even need an animator?
            childLocator = GetModelChildLocator();

            if (isAuthority)
            {
                sphereSearch = new SphereSearch();
                sphereSearch.origin = transform.position;
                sphereSearch.mask = LayerIndex.pickups.mask;
                sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.Collide;
                sphereSearch.radius = radius;
            }

            Search();
        }
        public void Search()
        {
            List<Collider> list = CollectionPool<Collider, List<Collider>>.RentCollection();
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
                //Debug.Log("found obj: " + obj.name);
                objectList.Add(obj);
            }

            GameObject closestObject = null;

            foreach (GameObject obj in objectList)
            {
                //bypass non-items
                if (obj.GetComponent<GenericPickupController>() != null)
                {
                    //to-do: filter out non-items. filter lunars?
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
                //Debug.Log("closest item pickup: " + gpc.GetDisplayName());
                //Clone();
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (gpc != null)
            {
                Clone();
            }
            //Debug.Log("exiting search");
        }

        public void Clone()
        {
            //Debug.Log("setting state with gpc: " + gpc.GetDisplayName());
            CloneDroneCook nextState = new CloneDroneCook();
            nextState.gpc = gpc;
            outer.SetNextState(nextState); //why doesn't this work????
            //Debug.Log("set next state");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && duration >= fixedAge)
                outer.SetNextStateToMain();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
