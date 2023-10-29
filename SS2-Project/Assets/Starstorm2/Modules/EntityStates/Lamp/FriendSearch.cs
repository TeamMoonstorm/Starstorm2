using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using Moonstorm.Starstorm2;
using UnityEngine.Networking;
using HG;
using Moonstorm.Starstorm2.Components;

namespace EntityStates.Lamp
{
    public class FriendSearch : BaseSkillState
    {
        public static float duration = 0.6f;
        public static float radius = 12f;
        public static string muzzleString = "Muzzle";
        private SphereSearch sphereSearch;

        public override void OnEnter()
        {
            base.OnEnter();

            if (isAuthority)
            {
                sphereSearch = new SphereSearch();
                sphereSearch.origin = transform.position;
                sphereSearch.mask = LayerIndex.enemyBody.mask;
                sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.Collide;
                sphereSearch.radius = radius;
            }
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
                objectList.Add(obj);
            }

            GameObject closestObject = null;


        }
    }
}
