using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using HG;
using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.Components;
using UnityEngine.Networking;

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
        public GameObject target;

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

        private float originalMoveSpeed;

        public override void OnEnter()
        {
            base.OnEnter();
            childLocator = GetModelChildLocator();
            targetIndicatorVfxPrefab = SS2Assets.LoadAsset<GameObject>("DuplicatingCircleVFX", SS2Bundle.Interactables);

            this.gpc = this.target.GetComponent<GenericPickupController>(); 
            pickupTier = gpc.pickupIndex.pickupDef.itemTier;

            originalMoveSpeed = characterBody.moveSpeed;

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
            characterBody.moveSpeed = originalMoveSpeed * 0.1f;
            if (!target)
            {
                //Debug.Log("was interrupted grabbing item");
                skillLocator.primary.rechargeStopwatch *= 0.25f; //as a treat
                outer.SetNextStateToMain();
                return;
            }
            if (duration <= fixedAge)
            {
                outer.SetNextStateToMain();
                //create the item

                if(NetworkServer.active)
                    CreateClone();
            }
        }

        public void CreateClone()
        {
            PickupDropletController.CreatePickupDroplet(gpc.pickupIndex, transform.position, Vector3.up * dropUpVelocityStrength + transform.forward * dropForwardVelocityStrength);
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

            //restore movespeed
            characterBody.moveSpeed = originalMoveSpeed;

            //set stock to 0 again :slight_smile:
            skillLocator.primary.DeductStock(1);

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
            glowMeshRenderer.material = SS2Assets.LoadAsset<Material>("matCloneDroneLightInvalid", SS2Bundle.Interactables);

        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(this.target);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.target = reader.ReadGameObject();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
