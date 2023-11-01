using EntityStates;
using EntityStates.DroneTable;
using Moonstorm.Starstorm2.Components;
using R2API;
using RoR2;
using RoR2.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Moonstorm.Starstorm2.Interactables
{
    //[DisabledContent]

    public sealed class DroneTable : InteractableBase
    {
        public override GameObject Interactable { get; } = SS2Assets.LoadAsset<GameObject>("DroneTablePrefab", SS2Bundle.Interactables);

        private static GameObject interactable;

        private static GameObject bodyOrb = SS2Assets.LoadAsset<GameObject>("CharacterBodyOrbEffect", SS2Bundle.Interactables);


        /// <summary>
        /// List of drone and interactable cost pairs. String is the body.name, int is the price a player pays, not the director.
        /// </summary>
        public static List<KeyValuePair<string, int>> dronePairs = new List<KeyValuePair<string, int>>();

        /// <summary>
        /// Dictonary of drone string (body.name) and the required pseudo-transform required to make it sit nicely on the table. The drone hologram is a child of a specific node on the table, so you should just be able to edit the drone itself's local transforms, and then input them here.
        /// </summary>
        public static Dictionary<string, RefabricatorTriple> droneTripletPairs = new Dictionary<string, RefabricatorTriple>(); //i guess you cant make transforms without an object and i didnt want to make dummy objects
        
        /// <summary>
        /// Dictonary of drone names and runtime-made drone sprites.
        /// </summary>
        public static Dictionary<string, Sprite> droneSpritePairs = new Dictionary<string, Sprite>();

        public static CostTypeDef droneCostDef;
        public static int droneCostIndex;
        public static DroneTableDropTable droneDropTable;

        //public static Material GreenHoloMaterial = SS2Assets.LoadAsset<Material>("matHoloGreen");

        public override MSInteractableDirectorCard InteractableDirectorCard { get; } = SS2Assets.LoadAsset<MSInteractableDirectorCard>("midcDroneTable", SS2Bundle.Interactables);

        public override void Initialize()
        {
            CostTypeCatalog.modHelper.getAdditionalEntries += addDroneCostType;

            On.EntityStates.Drone.DeathState.OnEnter += overrideDroneCorpse;

            interactable = InteractableDirectorCard.prefab;

            var interactionToken = interactable.AddComponent<RefabricatorInteractionToken>();
            interactionToken.PurchaseInteraction = interactable.GetComponent<PurchaseInteraction>();

            droneDropTable = new DroneTableDropTable();

            SetupDroneValueList();  
        }

        private void overrideDroneCorpse(On.EntityStates.Drone.DeathState.orig_OnEnter orig, EntityStates.Drone.DeathState self)
        {
            orig(self);
            var hardToken = self.characterBody.GetComponent<RefabricatorHardDeathToken>();
            if (hardToken)
            {
                //SS2Log.Info(self.gameObject.name + " | " + self.characterBody + " | " + self.characterBody.name);
                EntityState.Destroy(self.gameObject);
            }
        }

        private void overrideItemIcon(On.RoR2.Orbs.ItemTakenOrbEffect.orig_Start orig, RoR2.Orbs.ItemTakenOrbEffect self)
        {
            var efc = self.GetComponent<EffectComponent>();
            bool boolean = efc.effectData.genericBool;
            uint value = efc.effectData.genericUInt;
            if (boolean) //regular ItemTakenOrbs have their genericBool set to false, so if it's true, it's supposed to be a drone
            {
                var pair = dronePairs[(int)value];
                var gameobject = BodyCatalog.FindBodyPrefab(pair.Key);
                var body = gameobject.GetComponent<CharacterBody>();

                Sprite output = null;
                bool success = droneSpritePairs.TryGetValue(pair.Key, out output);
                if (!success)
                {
                    Texture icon = body.portraitIcon;

                    Rect rect = new Rect(0, 0, icon.width, icon.height);
                    icon.filterMode = FilterMode.Point;
                    RenderTexture rt = RenderTexture.GetTemporary(icon.width, icon.height);
                    rt.filterMode = FilterMode.Point;
                    RenderTexture.active = rt;
                    Graphics.Blit(icon, rt);
                    Texture2D tex2d = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
                    tex2d.ReadPixels(new Rect(rect.x, icon.height - rect.height - rect.y, rect.width, rect.height), 0, 0);
                    tex2d.Apply();

                    RenderTexture.active = null;
                    tex2d.wrapMode = TextureWrapMode.Clamp;
                    tex2d.filterMode = FilterMode.Bilinear;
                    output = Sprite.Create(tex2d, new Rect(0, 0, 128, 128), new Vector2(.5f, .5f), 25);
                    //output.pixelsPerUnit = 25;
                    output.name = body.portraitIcon.name + "Refabricator";
                    
                    droneSpritePairs.Add(pair.Key, output);
                }

                self.iconSpriteRenderer.sprite = output;

                Color color = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Tier1Item);
                self.trailToColor.startColor *= color;
                self.trailToColor.endColor *= color;
                for (int i = 0; i < self.particlesToColor.Length; i++)
                {   
                    ParticleSystem particleSystem = self.particlesToColor[i];
                    var main = particleSystem.main;
                    main.startColor = color;
                    particleSystem.Play();
                }

                for (int j = 0; j < self.spritesToColor.Length; j++)
                {
                    self.spritesToColor[j].color = color;
                }
            }
            else
            {
                orig(self);
            }
        }

        private void SetupDroneValueList()
        {
            dronePairs.Add(new KeyValuePair<string, int>("Turret1Body", 35));
            dronePairs.Add(new KeyValuePair<string, int>("Drone1Body", 40)); //gunner
            dronePairs.Add(new KeyValuePair<string, int>("Drone2Body", 40)); //healing
            dronePairs.Add(new KeyValuePair<string, int>("MissileDroneBody", 60));
            dronePairs.Add(new KeyValuePair<string, int>("EquipmentDroneBody", 60)); //takes equipment so uhhhh i dunno
            dronePairs.Add(new KeyValuePair<string, int>("EmergencyDroneBody", 100));
            dronePairs.Add(new KeyValuePair<string, int>("FlameDroneBody", 100));
            dronePairs.Add(new KeyValuePair<string, int>("MegaDroneBody", 350));
            
            dronePairs.Add(new KeyValuePair<string, int>("ShockDroneBody", 40));
            dronePairs.Add(new KeyValuePair<string, int>("CloneDroneBody", 140));


            droneTripletPairs.Add("Turret1Body", new RefabricatorTriple(new Vector3(0, -0.575f, -.2f), new Vector3(0, 180, 0), new Vector3(.09f, .09f, .09f)));
            droneTripletPairs.Add("Drone1Body", new RefabricatorTriple(new Vector3(0, -0.28f, 0), new Vector3(0, 156, 0), new Vector3(1.475f, 1.475f, 1.475f)));
            droneTripletPairs.Add("Drone2Body", new RefabricatorTriple(new Vector3(0, -0.0125f, 0), new Vector3(0, 0, 0), new Vector3(.355f, .355f, .355f)));
            droneTripletPairs.Add("MissileDroneBody", new RefabricatorTriple(new Vector3(0.36f, 0, 0), new Vector3(0, 0, 90), new Vector3(4, 4, 4)));
            droneTripletPairs.Add("EquipmentDroneBody", new RefabricatorTriple(new Vector3(-.185f, 0, 0), new Vector3(0, 0, 90), new Vector3(.505f, .505f, .505f)));
            droneTripletPairs.Add("EmergencyDroneBody", new RefabricatorTriple(new Vector3(0, -.225f, 0), new Vector3(0, 0, 0), new Vector3(.18f, .18f, .18f)));
            droneTripletPairs.Add("FlameDroneBody", new RefabricatorTriple(new Vector3(0.165f, -0.05f, 0), new Vector3(0, 0, 90), new Vector3(0.45f, 0.45f, 0.45f)));
            droneTripletPairs.Add("MegaDroneBody", new RefabricatorTriple(new Vector3(0, -0.025f, 0), new Vector3(0, 0, 0), new Vector3(0.1225f, 0.1225f, 0.1225f)));

            droneTripletPairs.Add("ShockDroneBody", new RefabricatorTriple(new Vector3(.675f, .025f, 0), new Vector3(0, 0, 90), new Vector3(.2f, .2f, .2f)));
            droneTripletPairs.Add("CloneDroneBody", new RefabricatorTriple(new Vector3(0, 0, 0), new Vector3(0, 0, 90), new Vector3(.35f, .35f, .35f)));

        }

        private void addDroneCostType(List<CostTypeDef> obj)
        {
            droneCostDef = new CostTypeDef();
            droneCostDef.costStringFormatToken = "SS2_COST_DRONE_FORMAT";
            droneCostDef.isAffordable = new CostTypeDef.IsAffordableDelegate(DroneCostTypeHelper.IsAffordable);
            droneCostDef.payCost = new CostTypeDef.PayCostDelegate(DroneCostTypeHelper.PayCost);
            droneCostDef.colorIndex = ColorCatalog.ColorIndex.Interactable;
            droneCostDef.saturateWorldStyledCostString = true;
            droneCostDef.darkenWorldStyledCostString = false;
            droneCostIndex = CostTypeCatalog.costTypeDefs.Length + obj.Count;
            obj.Add(droneCostDef);
        }

        private static class DroneCostTypeHelper
        {
            public static bool IsAffordable(CostTypeDef costTypeDef, CostTypeDef.IsAffordableContext context)
            {
                CharacterBody body = context.activator.GetComponent<CharacterBody>();
                if (!body)
                {
                    return false;
                }
                Inventory inventory = body.inventory;
                if (!inventory)
                {
                    return false;
                }
                int cost = context.cost;

                if ((body != null) ? body.master : null)
                {
                    MinionOwnership.MinionGroup minionGroup = MinionOwnership.MinionGroup.FindGroup(body.master.netId);
                    if (minionGroup != null)
                    {
                        var members = minionGroup.members;
                        List<CharacterMaster> validMinions = new List<CharacterMaster>();
                        foreach (var drone in members)
                        {
                            if (drone)
                            {
                                CharacterMaster master = drone.GetComponent<CharacterMaster>();
                                if (master)
                                {
                                    var droneBody = master.GetBody();
                                    if (droneBody)
                                    {
                                        if ((droneBody.bodyFlags & CharacterBody.BodyFlags.Mechanical) > CharacterBody.BodyFlags.None)
                                        {
                                            foreach (var pair in dronePairs)
                                            {
                                                if (droneBody.bodyIndex == BodyCatalog.FindBodyIndex(pair.Key))
                                                {
                                                    validMinions.Add(master);
                                                    break;
                                                }
                                            }
                                            if (validMinions.Count >= cost)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //SS2Log.Info("Drone " + master.name + " didn't have a body? affordab;e");
                                    }
                                }
                            }
                        }
                    }
                }
                return false;
            }

            public static void PayCost(CostTypeDef costTypeDef, CostTypeDef.PayCostContext context)
            {
                CharacterBody body = context.activator.GetComponent<CharacterBody>();
                int cost = context.cost;

                MinionOwnership.MinionGroup minionGroup = MinionOwnership.MinionGroup.FindGroup(body.master.netId);
                if (minionGroup != null)
                {
                    var members = minionGroup.members;
                    List<CharacterMaster> validMinions = new List<CharacterMaster>();
                    foreach (var drone in members)
                    {
                        if (drone)
                        {
                            CharacterMaster master = drone.GetComponent<CharacterMaster>();
                            if (master)
                            {
                                var droneBody = master.GetBody();
                                if (droneBody)
                                {
                                    if ((droneBody.bodyFlags & CharacterBody.BodyFlags.Mechanical) > CharacterBody.BodyFlags.None)
                                    {
                                        foreach (var pair in dronePairs)
                                        {
                                            //SS2Log.Info("testing " + pair.Key);
                                            if (droneBody.bodyIndex == BodyCatalog.FindBodyIndex(pair.Key))
                                            {
                                                validMinions.Add(master);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //SS2Log.Info("Drone " + master.name + " didn't have a body?");
                                }
                            }
                        }
                    }

                    Util.ShuffleList(validMinions, context.rng);

                    for (int k = 0; k < cost; k++)
                    {
                        TakeOne();
                    }
                    MultiShopCardUtils.OnNonMoneyPurchase(context);
                    void TakeOne()
                    {
                        for (int i = 0; i < validMinions.Count(); ++i)
                        {
                            if (validMinions[i])
                            {
                                var drone = validMinions[i].GetBody();

                                for (int j = 0; j < dronePairs.Count; ++j)
                                {
                                    var pair = dronePairs[j];
                                    if (drone.bodyIndex == BodyCatalog.FindBodyIndex(pair.Key))
                                    {
                                        EffectData effectData = new EffectData
                                        {
                                            origin = drone.corePosition,
                                            genericFloat = 1.5f,
                                            genericUInt = (uint)(drone.bodyIndex + 1),
                                            genericBool = true
                                            
                                        };

                                        var model = context.purchasedObject;

                                        effectData.SetNetworkedObjectReference(context.purchasedObject);  //behaves strangely if target is networked ref

                                        EffectManager.SpawnEffect(bodyOrb, effectData, true);

                                        if (drone.baseNameToken == "TURRET1_BODY_NAME" || drone.name == "Turret1Body(Clone)")
                                        {
                                            Util.PlaySound("Play_drone_deathpt1", drone.gameObject);
                                            Util.PlaySound("Play_drone_deathpt2", drone.gameObject);
                                            //Util.PlaySound("RefabricatorSelect2", model);
                                        }

                                        if (!droneDropTable)
                                        {
                                            droneDropTable = new DroneTableDropTable();
                                        }
                                        int dropvalue = Mathf.RoundToInt((Mathf.Pow(pair.Value, 1.01f) * .15f) - 4);
                                        //int dropvalue = Mathf.RoundToInt((Mathf.Sqrt(pair.Value) - 5.65f) * 2.2f);
                                        //int val = Mathf.RoundToInt(Mathf.Pow(Mathf.Sqrt(pair.Value) / 3, 1.35f) - 1.5f);
                                        PickupIndex ind = droneDropTable.GenerateDropPreReplacement(context.rng, dropvalue);

                                        //some weird catches to try help make drones give roughly their 'tier' but like i dunno man
                                        if (ind.pickupDef.itemTier != ItemTier.Tier1 && pair.Value < 60)
                                        {
                                            PickupIndex ind2 = droneDropTable.GenerateDropPreReplacement(context.rng, dropvalue);
                                            if (ind2.pickupDef.itemTier != ItemTier.Tier3)
                                            {
                                                ind = ind2;
                                            }  
                                        }

                                        if (ind.pickupDef.itemTier == ItemTier.Tier3 && pair.Value >= 60 && pair.Value < 350)
                                        {
                                            {
                                                PickupIndex ind2 = droneDropTable.GenerateDropPreReplacement(context.rng, dropvalue);
                                                if (ind2.pickupDef.itemTier != ItemTier.Tier1)
                                                {
                                                    ind = ind2;
                                                }
                                            }
                                        }

                                        if (ind.pickupDef.itemTier != ItemTier.Tier3 && pair.Value >= 350)
                                        {
                                            {
                                                PickupIndex ind2 = droneDropTable.GenerateDropPreReplacement(context.rng, dropvalue);
                                                if (ind2.pickupDef.itemTier != ItemTier.Tier1)
                                                {
                                                    ind = ind2;
                                                    PickupIndex ind3 = droneDropTable.GenerateDropPreReplacement(context.rng, dropvalue);
                                                    if (ind3.pickupDef.itemTier != ItemTier.Tier1)
                                                    {
                                                        ind = ind3;
                                                    }
                                                }
                                            }
                                        }

                                        Util.PlaySound("RefabricatorSelect2", model);

                                        var esm = model.GetComponent<EntityStateMachine>();
                                        if (esm)
                                        {
                                            DestroyLeadin nextState = new DestroyLeadin();
                                            nextState.droneIndex = (int)drone.bodyIndex + 1;
                                            nextState.itemIndex = (int)ind.pickupDef.itemIndex;
                                            esm.SetNextState(nextState);
                                        }

                                        drone.gameObject.AddComponent<RefabricatorHardDeathToken>();
                                        drone.healthComponent.Suicide();

                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }


        public class RefabricatorInteractionToken : MonoBehaviour
        {
            public CharacterBody LastActivator;
            public PurchaseInteraction PurchaseInteraction;
            
            //public Transform symbolTransform;
            //public EntityStateMachine esm;

            public void Start()
            {
                if (NetworkServer.active && Run.instance)
                {
                    PurchaseInteraction.SetAvailableTrue();
                }
                PurchaseInteraction.costType = (CostTypeIndex)droneCostIndex;
                PurchaseInteraction.onPurchase.AddListener(DronePurchaseAttempt);

                
                //esm = GetComponent<EntityStateMachine>();
                //SS2Log.Info("esm: " + esm);
                //InteractableBodyModelPrefab.transform.Find("Symbol");
                //BuffBrazierStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Body");

                //ConstructFlameChoice();
                //BaseCostDetermination = (int)(PurchaseInteraction.cost * ChosenBuffBrazierBuff.CostModifier);
                //SetCost();
            }

            public void DronePurchaseAttempt(Interactor interactor)
            {
                if (!interactor) { return; }

                var body = interactor.GetComponent<CharacterBody>();
                if (body && body.master)
                {
                    if (NetworkServer.active)
                    {
                        //SS2Log.Info("Purchase Successful");
                        //AttemptSpawnVoidPortal();
                        //GameObject effectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ShrineUseEffect.prefab").WaitForCompletion();
                        //EffectManager.SimpleImpactEffect(effectPrefab, this.transform.position, new Vector3(0, 0, 0), true);

                        //GameObject portal = UnityEngine.Object.Instantiate<GameObject>(ShatteredMonolith.voidFieldPortalObject, this.transform.position, new Quaternion(0, 70, 0, 0));
                        //NetworkServer.Spawn(portal);
                        LastActivator = body;

                        //symbolTransform.gameObject.SetActive(false);
                        //PurchaseInteraction.SetAvailable(false);
                        //play animation
                        //esm.SetNextState(new DestroyLeadin());
                        //StartCoroutine(reenableAvailablity());
                    }
                }
            }
        }

        public class RefabricatorHardDeathToken : MonoBehaviour
        {

        }

        public class DroneTableDropTable : BasicPickupDropTable
        {
            private void AddNew(List<PickupIndex> sourceDropList, float listWeight)
            {
                if (listWeight <= 0f || sourceDropList.Count == 0)
                {
                    return;
                }
                float weight = listWeight / (float)sourceDropList.Count;
                foreach (PickupIndex value in sourceDropList)
                {
                    selector.AddChoice(value, weight);
                }
            }

            public PickupIndex GenerateDropPreReplacement(Xoroshiro128Plus rng, int count)
            {
                selector.Clear();
                AddNew(Run.instance.availableTier1DropList, tier1Weight);
                AddNew(Run.instance.availableTier2DropList, tier2Weight * (float)count);
                AddNew(Run.instance.availableTier3DropList, tier3Weight * Mathf.Pow((float)count, 2f)); //this is basically the shipping request code but with a slightly lower red weight scaling

                return PickupDropTable.GenerateDropFromWeightedSelection(rng, selector);
            }

            public override int GetPickupCount()
            {
                return selector.Count;
            }

            public override PickupIndex[] GenerateUniqueDropsPreReplacement(int maxDrops, Xoroshiro128Plus rng)
            {
                return PickupDropTable.GenerateUniqueDropsFromWeightedSelection(maxDrops, rng, selector);
            }

            new private float tier1Weight = .793f; //.316f;

            new private float tier2Weight = .20f; //.08f;

            new private float tier3Weight = .007f; //.004f;

            new private readonly WeightedSelection<PickupIndex> selector = new WeightedSelection<PickupIndex>(8);
        }

        public class RefabricatorTriple : MonoBehaviour
        {
            public Vector3 position;
            public Vector3 rotation;
            public Vector3 scale;

            public RefabricatorTriple(Vector3 pos, Vector3 rot, Vector3 size)
            {
                position = pos;
                rotation = rot;
                scale = size;
            }
        }

    }
}
