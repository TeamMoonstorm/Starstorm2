using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using HG;
using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.Components;

namespace EntityStates.CloneDrone
{
    public class CloneDroneCook : BaseSkillState
    {
        public static float baseDuration = 4f;
        public static string spawnMuzzle = "Muzzle";
        public static string spawnMuzzle2 = "Muzzle2";
        public static float dropUpVelocityStrength = -5f;
        public static float dropForwardVelocityStrength = 3f;
        public static GameObject targetIndicatorVfxPrefab;
        public GenericPickupController gpc;

        private ChildLocator childLocator;
        private ItemTier pickupTier;
        private float duration = 4;

        private GameObject targetIndicatorVfxInstance;
        private GameObject sparksObj;
        private GameObject lightObj;
        private GameObject startObj;

        private GameObject glowMesh;
        private MeshRenderer glowMeshRenderer;
        private Material glowMeshMat;

        private SphereSearch sphereSearch;

        private bool isInterrupted = false;

        public override void OnEnter()
        {
            base.OnEnter();
            childLocator = GetModelChildLocator();
            targetIndicatorVfxPrefab = SS2Assets.LoadAsset<GameObject>("DuplicatingCircleVFX", SS2Bundle.Indev);
            pickupTier = gpc.pickupIndex.pickupDef.itemTier;

            if (targetIndicatorVfxPrefab)
            {
                targetIndicatorVfxInstance = Object.Instantiate<GameObject>(targetIndicatorVfxPrefab, gpc.gameObject.transform.position, Quaternion.identity);
                ChildLocator cl = targetIndicatorVfxInstance.GetComponent<ChildLocator>();
                if (cl)
                {
                    //move + set end effect
                    Transform lineEndTransform = cl.FindChild("LineEnd");
                    if (lineEndTransform)
                    {
                        lineEndTransform.position = FindModelChild(spawnMuzzle).position;
                        lineEndTransform.SetParent(FindModelChild(spawnMuzzle).transform); //hmm...
                    }

                    //move + parent + set + recolor spark effect
                    Transform sparksTransform = cl.FindChild("Sparks");
                    if (sparksTransform)
                    {
                        if (sparksTransform.GetComponent<ParticleSystem>() != null)
                        {
                            ParticleSystem sparksps = sparksTransform.GetComponent<ParticleSystem>();
                            sparksps.startColor = gpc.pickupIndex.GetPickupColor();
                        }
                        sparksTransform.position = FindModelChild(spawnMuzzle2).position;
                        sparksTransform.SetParent(FindModelChild(spawnMuzzle2).transform);
                        sparksObj = sparksTransform.gameObject;
                    }

                    //move + parent + set + recolor light effect
                    Transform lightTransform = cl.FindChild("Light");
                    if (lightTransform)
                    {
                        if (lightTransform.GetComponent<Light>() != null)
                        {
                            Light light = lightTransform.GetComponent<Light>();
                            light.color = gpc.pickupIndex.GetPickupColor();
                        }
                        lightTransform.position = FindModelChild(spawnMuzzle2).position;
                        lightTransform.SetParent(FindModelChild(spawnMuzzle2).transform);
                        lightObj = lightTransform.gameObject;
                    }

                    //move + parent + set start effect
                    Transform startTransform = cl.FindChild("StartEffect");
                    if (startTransform)
                    {
                        startTransform.position = FindModelChild(spawnMuzzle).position;
                        startTransform.SetParent(FindModelChild(spawnMuzzle).transform);
                        startObj = startTransform.gameObject;
                    }
                }
            }

            //if (isAuthority)
            //characterBody.SetBuffCount(SS2Content.Buffs.bdHiddenSlow20.buffIndex, 8); //lol
            float cooldown = 60f;
            duration = baseDuration;

            //tier 2 items have a 50% longer cooldown / duration
            if (pickupTier == ItemTier.Tier2 || pickupTier == ItemTier.VoidTier2)
            {
                duration *= 1.5f;
                cooldown *= 1.5f;
            }

            //tier 3 and boss items have a 100% longer cooldown / duration
            if (pickupTier == ItemTier.Tier3 || pickupTier == ItemTier.VoidTier3 || pickupTier == ItemTier.Boss || pickupTier == ItemTier.VoidBoss)
            {
                duration *= 2f;
                cooldown *= 2f;
            }

            //add the cooldown amount to the primary cooldown
            skillLocator.primary.stock = 0;
            skillLocator.primary.rechargeStopwatch -= cooldown;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority)
            {
                rigidbodyMotor.moveVector = Vector3.zero;
                rigidbody.velocity = new Vector3(rigidbody.velocity.x * 0.95f, rigidbody.velocity.y * 0.95f, rigidbody.velocity.z * 0.95f);

                //checks if item still exists - if not, end the skill
                if (gpc == null && !isInterrupted)
                {
                    isInterrupted = true;
                    //Debug.Log("was interrupted grabbing item");
                    skillLocator.primary.rechargeStopwatch *= 0.25f; //as a treat
                    outer.SetNextStateToMain();
                }

                if (duration <= fixedAge)
                {
                    outer.SetNextStateToMain();
                    //create the item
                    CreateClone();
                }
            }
        }

        public void CreateClone()
        {
            PickupDropletController.CreatePickupDroplet(gpc.pickupIndex, transform.position, Vector3.up * dropUpVelocityStrength + transform.forward * dropForwardVelocityStrength);

            sphereSearch = new SphereSearch();
            sphereSearch.origin = transform.position;
            sphereSearch.mask = LayerIndex.pickups.mask;
            sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.Collide;
            sphereSearch.radius = 8f;

            gpc.gameObject.AddComponent<CloneDroneItemMarker>();

            Search();
        }

        public void Search() //this is so fucking stupid man
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

            GameObject probablyTheObjectIJustSpawnedPleaseFuckGod = null;

            foreach (GameObject obj in objectList)
            {
                Debug.Log("beginning to check obj");
                //are you even an item?
                if (obj.GetComponent<GenericPickupController>() != null)
                {
                    Debug.Log("obj is item");
                    //do you have dominant or recessive genes?
                    if (obj.GetComponent<CloneDroneItemMarker>() == null)
                    {
                        Debug.Log("obj isnt clone");
                        GenericPickupController tempGPC = obj.GetComponent<GenericPickupController>();
                        //check that the item matches the intended item
                        if (tempGPC.pickupIndex == gpc.pickupIndex)
                        {
                            Debug.Log("obj is correct pickup");
                            float distance = Vector3.Distance(transform.position, obj.transform.position);
                            //if there's no better candidate, set this one - if a closer one is found, choose that instead
                            if (probablyTheObjectIJustSpawnedPleaseFuckGod == null || distance < Vector3.Distance(transform.position, probablyTheObjectIJustSpawnedPleaseFuckGod.transform.position))
                            {
                                Debug.Log("set predicted obj to: " + obj);
                                probablyTheObjectIJustSpawnedPleaseFuckGod = obj;
                            }
                        }
                    }
                }
            }

            if (probablyTheObjectIJustSpawnedPleaseFuckGod != null)
            {
                Debug.Log("probablyobj: " + probablyTheObjectIJustSpawnedPleaseFuckGod.name);
                probablyTheObjectIJustSpawnedPleaseFuckGod.AddComponent<CloneDroneItemMarker>();
            }
            else
            {
                Debug.Log("found nothing :(");
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            //destroy the target indicator vfx, if it exists
            if (targetIndicatorVfxInstance)
            {
                Destroy(targetIndicatorVfxInstance);
                targetIndicatorVfxInstance = null;
            }

            //set stock to 0 again :slight_smile:
            skillLocator.primary.DeductStock(1);

            //remove slow effect
            //if (isAuthority)
            //characterBody.SetBuffCount(SS2Content.Buffs.bdHiddenSlow20.buffIndex, 0);

            //destroy sparks effect
            if (sparksObj != null)
                Destroy(sparksObj);

            //destroy light effect
            if (lightObj != null)
                Destroy(lightObj);

            //destroy muzzle effect
            if (startObj != null)
                Destroy(startObj);

            glowMesh = childLocator.FindChild("GlowMesh").gameObject;
            glowMeshRenderer = glowMesh.GetComponent<MeshRenderer>();
            glowMeshRenderer.material = SS2Assets.LoadAsset<Material>("matCloneDroneLightInvalid", SS2Bundle.Indev);

        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
