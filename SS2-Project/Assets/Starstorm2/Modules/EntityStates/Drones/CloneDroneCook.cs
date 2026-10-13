using System;
using MSU;
using UnityEngine;
using RoR2;
using SS2;
using UnityEngine.Networking;
using SS2.Interactables;
using Starstorm2.Components;
using Object = UnityEngine.Object;

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

        public Material t1Mat;
        public Material t2Mat;
        public Material t3Mat;
        public Material bossMat;
        public Material voidMat;
        public Material lunarMat;
        public Material equipMat;
        public Material hexMat;
        public Material curseMat;
        public Material foodMat;

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
            pickupTier = gpc._pickupState.pickupIndex.pickupDef.itemTier;

            originalMoveSpeed = characterBody.moveSpeed;

            if (targetIndicatorVfxPrefab)
            {
                targetIndicatorVfxInstance = Object.Instantiate<GameObject>(targetIndicatorVfxPrefab, gpc.gameObject.transform.position, Quaternion.identity);
                ChildLocator cl = targetIndicatorVfxInstance.GetComponent<ChildLocator>();
                if (cl)
                {
                    Color pickupColor = gpc._pickupState.pickupIndex.pickupDef.baseColor;
                    
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
                            var main = sparksps.main;
                            main.startColor = pickupColor;
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
                            light.color = pickupColor;
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

            float cooldown = 60f;
            duration = baseDuration;

            Material lightMat = (pickupTier) switch
            {
                ItemTier.Tier1 => t1Mat,
                ItemTier.Tier2 => t2Mat,
                ItemTier.Tier3 => t3Mat,
                ItemTier.Lunar => lunarMat,
                ItemTier.Boss => bossMat,
                ItemTier.VoidTier1 => voidMat,
                ItemTier.VoidTier2 => voidMat,
                ItemTier.VoidTier3 => voidMat,
                ItemTier.VoidBoss => voidMat,
                ItemTier.FoodTier => foodMat,
                ItemTier.Hex => hexMat,
                ItemTier.Cursed => curseMat,
            };
            if (gpc._pickupState.pickupIndex.pickupDef.equipmentIndex != EquipmentIndex.None)
                lightMat = equipMat;

            float multiplier = pickupTier switch
            {
                ItemTier.Tier2 => 1.5f,
                ItemTier.Tier3 => 2f,
                ItemTier.Boss => 2f,
                ItemTier.VoidTier2 => 1.5f,
                ItemTier.VoidTier3 => 2f,
                ItemTier.VoidBoss => 2f,
                ItemTier.FoodTier => 2f,
                ItemTier.Cursed => 2f,
                ItemTier.Hex => 2f,
                _ => 1f
            };
            
            duration *= multiplier;
            cooldown *= multiplier;
            if (charModel)
                charModel.baseRendererInfos[2].defaultMaterial = lightMat;

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
            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();
                //create the item

                if(NetworkServer.active)
                    CreateClone();
            }

        }

        public void CreateClone()
        {
            GenericPickupController.CreatePickupInfo info = new GenericPickupController.CreatePickupInfo
            {
                prefabOverride = CloneDroneDamaged.pickupPrefab,
                _pickupState = gpc._pickupState,
                position = transform.position,
                duplicated = true
            };
            gameObject.GetComponent<CloneDroneClonedPickups>().AsValidOrNull()?.OnCloned(gpc.gameObject);
            PickupDropletController.CreatePickupDroplet(info, transform.position, Vector3.up * dropUpVelocityStrength + transform.forward * dropForwardVelocityStrength);
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
