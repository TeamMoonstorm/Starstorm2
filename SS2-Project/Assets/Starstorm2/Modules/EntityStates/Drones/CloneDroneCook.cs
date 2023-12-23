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
        private CharacterModel charModel;

        private Material lightOn = SS2Assets.LoadAsset<Material>("matCloneDroneLight", SS2Bundle.Interactables);
        private Material lightOff = SS2Assets.LoadAsset<Material>("matCloneDroneNoLight", SS2Bundle.Interactables);

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
            charModel = childLocator.gameObject.GetComponent<CharacterModel>(); //lol

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
            //if (pickupTier == ItemTier.Tier2 || pickupTier == ItemTier.VoidTier2)
            //{
            //    duration *= 1.5f;
            //    cooldown *= 1.5f;
            //}
            //
            ////tier 3 and boss items have a 100% longer cooldown / duration
            //if (pickupTier == ItemTier.Tier3 || pickupTier == ItemTier.VoidTier3 || pickupTier == ItemTier.Boss || pickupTier == ItemTier.VoidBoss)
            //{
            //    duration *= 2f;
            //    cooldown *= 2f;
            //}

            switch (pickupTier)
            {
                case ItemTier.Tier1:
                    if (charModel)
                        charModel.baseRendererInfos[2].defaultMaterial = SS2Assets.LoadAsset<Material>("matCloneDroneLightT1", SS2Bundle.Interactables);
                    break;

                case ItemTier.Tier2:
                    duration *= 1.5f;
                    cooldown *= 1.5f;
                    if (charModel)
                        charModel.baseRendererInfos[2].defaultMaterial = SS2Assets.LoadAsset<Material>("matCloneDroneLightT2", SS2Bundle.Interactables);
                    break;

                case ItemTier.Tier3:
                    duration *= 2f;
                    cooldown *= 2f;
                    if (charModel)
                        charModel.baseRendererInfos[2].defaultMaterial = SS2Assets.LoadAsset<Material>("matCloneDroneLightT3", SS2Bundle.Interactables); 
                    break;

                case ItemTier.Boss:
                    duration *= 2f;
                    cooldown *= 2f;
                    if (charModel)
                        charModel.baseRendererInfos[2].defaultMaterial = SS2Assets.LoadAsset<Material>("matCloneDroneLightT4", SS2Bundle.Interactables);
                    break;

                case ItemTier.VoidTier1:
                    if (charModel)
                        charModel.baseRendererInfos[2].defaultMaterial = SS2Assets.LoadAsset<Material>("matCloneDroneLightVoid", SS2Bundle.Interactables);
                    break;

                case ItemTier.VoidTier2:
                    duration *= 1.5f;
                    cooldown *= 1.5f;
                    if (charModel)
                        charModel.baseRendererInfos[2].defaultMaterial = SS2Assets.LoadAsset<Material>("matCloneDroneLightVoid", SS2Bundle.Interactables);
                    break;

                case ItemTier.VoidTier3:
                    duration *= 2f;
                    cooldown *= 2f;
                    if (charModel)
                        charModel.baseRendererInfos[2].defaultMaterial = SS2Assets.LoadAsset<Material>("matCloneDroneLightVoid", SS2Bundle.Interactables);
                    break;

                case ItemTier.VoidBoss:
                    duration *= 2f;
                    cooldown *= 2f;
                    if (charModel)
                        charModel.baseRendererInfos[2].defaultMaterial = SS2Assets.LoadAsset<Material>("matCloneDroneLightVoid", SS2Bundle.Interactables);
                    break;

                case ItemTier.Lunar:
                    if (charModel)
                        charModel.baseRendererInfos[2].defaultMaterial = SS2Assets.LoadAsset<Material>("matCloneDroneLightLunar", SS2Bundle.Interactables);
                    break;

                default: //lol its an equipment 
                    if (charModel){
                        if (gpc.pickupIndex.pickupDef.isLunar){ //lunar
                            charModel.baseRendererInfos[2].defaultMaterial = SS2Assets.LoadAsset<Material>("matCloneDroneLightLunar", SS2Bundle.Interactables);
                        }else{ //normal
                            charModel.baseRendererInfos[2].defaultMaterial = SS2Assets.LoadAsset<Material>("matCloneDroneLightEquip", SS2Bundle.Interactables);
                        }
                    }
                    break;
            }

            //add the cooldown amount to the primary cooldown
            skillLocator.primary.stock = 0;
            skillLocator.primary.rechargeStopwatch -= cooldown;
            //charModel.baseRendererInfos[2].defaultMaterial = SS2Assets.LoadAsset<Material>("matCloneDroneLight", SS2Bundle.Interactables);
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
            //if (charModel)
            //{
            //    if((int)Mathf.Round(fixedAge) != (int)Mathf.Floor(fixedAge)){
            //        charModel.baseRendererInfos[2].defaultMaterial = lightOn;
            //    }
            //    else
            //    {
            //        charModel.baseRendererInfos[2].defaultMaterial = lightOff;
            //    }
            //}
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

            //glowMesh = childLocator.FindChild("GlowMesh").gameObject;
            //glowMeshRenderer = glowMesh.GetComponent<MeshRenderer>();
            //glowMeshRenderer.material = SS2Assets.LoadAsset<Material>("matCloneDroneLightInvalid", SS2Bundle.Interactables);
            if (charModel)
            {
                charModel.baseRendererInfos[2].defaultMaterial = SS2Assets.LoadAsset<Material>("matCloneDroneNoLight", SS2Bundle.Interactables); ;
            }
                    

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
