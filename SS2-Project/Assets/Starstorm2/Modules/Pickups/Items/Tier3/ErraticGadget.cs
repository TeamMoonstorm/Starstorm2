using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;
using RoR2.Orbs;
using System.Collections.Generic;
using System.Linq;
using MSU;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace SS2.Items
{

    /// <summary>
    /// 
    /// 
    ///  LOTS OF STUFF NEEDS TO BE REDONE WHEN PROCTYPEAPI RELEASES
    /// 
    /// 
    /// 
    /// </summary>
    public sealed class ErraticGadget : SS2Item
    {
        private const string token = "SS2_ITEM_ERRATICGADGET_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acErraticGadget", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Chance on hit to fire lightning. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float procChance = 0.1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Damage coefficient of Erratic Gadget's lightning proc. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float damageCoefficient = 3f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Number of targets for Erratic Gadget's lightning proc, per stack.")]
        [FormatToken(token, 2)]
        public static int bounceTargets = 1;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Damage multiplier for Erratic Gadget's doubled lightning.")]
        public static int repeatDamageMultiplier = 1;

        private static GameObject _orbEffectPrefab;
        public static GameObject _procEffectPrefab;
        private static GameObject _displayEffectPrefab;

        //N: True...
        public static DamageAPI.ModdedDamageType PROCTYPEAPIWHEN;

        // man o war handled in its own class
        public override void Initialize()
        {
            _orbEffectPrefab = AssetCollection.FindAsset<GameObject>("GadgetOrbEffect");
            _procEffectPrefab = AssetCollection.FindAsset<GameObject>("GadgetLightningStartEffect");
            _displayEffectPrefab = AssetCollection.FindAsset<GameObject>("GadgetLightningProc");

            PROCTYPEAPIWHEN = DamageAPI.ReserveDamageType();
            On.RoR2.Orbs.LightningOrb.OnArrival += LightningOrb_OnArrival; // uke tesla BFG arti loader 
            //On.RoR2.Orbs.VoidLightningOrb.Begin += VoidLightningOrb_Begin; // polylute /// nah no thanks not real lightning
            On.RoR2.Orbs.SimpleLightningStrikeOrb.OnArrival += SimpleLightningStrikeOrb_OnArrival; // charged perforator i think
            On.RoR2.Orbs.LightningStrikeOrb.OnArrival += LightningStrikeOrb_OnArrival; // royal capacitor
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        // figure out if a lightningorb "ends", then figure out how many objects it bounced to, then spawn a gadgetlightningorb with the same stats that bounces to that many targets
        private void LightningOrb_OnArrival(On.RoR2.Orbs.LightningOrb.orig_OnArrival orig, LightningOrb self)
        {
            if(!self.target) // i dont like this... but im also lazy
            {
                orig(self);
                return;
            }
            // jank ass way to check if a lightning orb "ended" by not finding a new target
            bool orbFoundNewTarget = false;
            if (self.bouncedObjects != null && self.bouncesRemaining > 0)
            {
                self.bouncedObjects.Add(self.target.healthComponent); // add self.target.healthComponent so PickNextTarget doesnt pick the same target it already had
                orbFoundNewTarget = self.PickNextTarget(self.target.transform.position); // check if theres an available target, same way LightningOrb does
                if (orbFoundNewTarget)
                    self.bouncedObjects.RemoveAt(self.bouncedObjects.Count - 1); // remove what PickNextTarget added to the end of the list, because we dont want it there
                self.bouncedObjects.RemoveAt(self.bouncedObjects.Count - 1); // remove self.target.healthComponent from the list 
            }
            orig(self);

            bool isLastBounce = self.bouncedObjects != null && (self.bouncesRemaining == 0 || !orbFoundNewTarget);

            if (isLastBounce && self.attacker && self.attacker.GetComponent<CharacterBody>().inventory.GetItemCount(SS2Content.Items.ErraticGadget) > 0)
            {
                bool canProcGadget = false;
                bool wasFirstBounceFake = false;
                switch (self.lightningType)
                {
                    case LightningOrb.LightningType.Ukulele:
                        canProcGadget = true;
                        wasFirstBounceFake = true;
                        break;
                    case LightningOrb.LightningType.Tesla:
                        canProcGadget = true;
                        wasFirstBounceFake = true;
                        break;
                    case LightningOrb.LightningType.BFG:
                        canProcGadget = true;
                        break;
                    case LightningOrb.LightningType.TreePoisonDart:
                        break;
                    case LightningOrb.LightningType.HuntressGlaive:
                        break;
                    case LightningOrb.LightningType.Loader:
                        canProcGadget = true;
                        break;
                    case LightningOrb.LightningType.RazorWire:
                        break;
                    case LightningOrb.LightningType.CrocoDisease:
                        break;
                    case LightningOrb.LightningType.MageLightning:
                        canProcGadget = true;
                        break;
                }

                if (canProcGadget)
                {
                    // ukulele and tesla add the initial target to bouncedObjects without damaging them. 
                    //we want to subtract 1 if wasFirstBounceEnemy==true, in order to get the amount of targets the lightning actually hit
                    //we also have to count unique objects in bouncedObjects because vanilla code adds each target more than once
                    // tldr lightningorb is shit and now its my problem
                    HashSet<HealthComponent> uniqueObjects = new HashSet<HealthComponent>();
                    for (int i = 0; i < self.bouncedObjects.Count; i++)
                    {
                        uniqueObjects.Add(self.bouncedObjects[i]);
                    }
                    int bounces = uniqueObjects.Count;
                    if (wasFirstBounceFake) bounces--;


                    GadgetLightningOrb gadgetOrb = new GadgetLightningOrb();
                    gadgetOrb.search = self.search;
                    gadgetOrb.origin = self.target.transform.position;
                    gadgetOrb.attacker = self.attacker;
                    gadgetOrb.inflictor = self.inflictor;
                    gadgetOrb.teamIndex = self.teamIndex;
                    gadgetOrb.damageValue = self.damageValue * self.damageCoefficientPerBounce * repeatDamageMultiplier;
                    gadgetOrb.bouncesRemaining = bounces;
                    gadgetOrb.isCrit = self.isCrit;
                    gadgetOrb.bouncedObjects = new List<HealthComponent> { self.target.healthComponent };
                    gadgetOrb.procChainMask = self.procChainMask;
                    gadgetOrb.procCoefficient = self.procCoefficient;
                    gadgetOrb.damageColorIndex = DamageColorIndex.Item;
                    gadgetOrb.damageCoefficientPerBounce = self.damageCoefficientPerBounce;
                    gadgetOrb.range = self.range;
                    gadgetOrb.damageType = self.damageType;
                    gadgetOrb.canBounceOnSameTarget = self.canBounceOnSameTarget;
                    gadgetOrb.duration = self.duration;

                    HurtBox nextTarget = gadgetOrb.PickNextTarget(self.target.transform.position);
                    if (nextTarget)
                    {
                        gadgetOrb.target = nextTarget;
                        OrbManager.instance.AddOrb(gadgetOrb);
                        EffectManager.SimpleEffect(_procEffectPrefab, self.target.transform.position, Quaternion.identity, true);
                    }
                }
            }
        }

        #region Other Orbs
        // ROYAL CAPACITOR SPAWNS SECOND STRIKE
        private void LightningStrikeOrb_OnArrival(On.RoR2.Orbs.LightningStrikeOrb.orig_OnArrival orig, LightningStrikeOrb self)
        {
            // jank ass way to check if lightning was spawned by erratic gadget proc
            // can be un-janked whith proctypeapi
            //hopefully no lightning orbs use FruitOnHit lol
            bool isSecondStrike = self.damageType.damageType.HasFlag(DamageType.FruitOnHit);
            if (isSecondStrike) self.damageType &= ~DamageType.FruitOnHit;
            orig(self);

            if (!isSecondStrike && self.attacker?.GetComponent<CharacterBody>()?.inventory?.GetItemCount(SS2Content.Items.ErraticGadget) > 0)
            {
                OrbManager.instance.AddOrb(new LightningStrikeOrb
                {
                    attacker = self.attacker,
                    damageColorIndex = self.damageColorIndex,
                    damageValue = self.damageValue * repeatDamageMultiplier,
                    isCrit = self.isCrit,
                    procChainMask = self.procChainMask,
                    procCoefficient = self.procCoefficient,
                    damageType = self.damageType | DamageType.FruitOnHit, // PROCTYPEAPI WHENNNNNNNN
                    target = self.target
                });

                EffectManager.SimpleEffect(_procEffectPrefab, self.target.transform.position, Quaternion.identity, true);
            }
        }

        // charged perforator strikes twice
        private void SimpleLightningStrikeOrb_OnArrival(On.RoR2.Orbs.SimpleLightningStrikeOrb.orig_OnArrival orig, SimpleLightningStrikeOrb self)
        {
            bool isSecondStrike = self.damageType.damageType.HasFlag(DamageType.FruitOnHit);
            if (isSecondStrike) self.damageType &= ~DamageType.FruitOnHit;
            orig(self);

            if (!isSecondStrike && self.attacker?.GetComponent<CharacterBody>()?.inventory?.GetItemCount(SS2Content.Items.ErraticGadget) > 0)
            {
                OrbManager.instance.AddOrb(new SimpleLightningStrikeOrb
                {
                    attacker = self.attacker,
                    damageColorIndex = self.damageColorIndex,
                    damageValue = self.damageValue * repeatDamageMultiplier,
                    isCrit = self.isCrit,
                    procChainMask = self.procChainMask,
                    procCoefficient = self.procCoefficient,
                    damageType = self.damageType | DamageType.FruitOnHit, // PROCTYPEAPI WHENNNNNNNN
                    target = self.target
                });

                EffectManager.SimpleEffect(_procEffectPrefab, self.target.transform.position, Quaternion.identity, true);
            }
        }

        private void VoidLightningOrb_Begin(On.RoR2.Orbs.VoidLightningOrb.orig_Begin orig, VoidLightningOrb self)
        {
            orig(self);

            if (self.attacker?.GetComponent<CharacterBody>()?.inventory?.GetItemCount(SS2Content.Items.ErraticGadget) > 0)
            {
                self.totalStrikes *= 2;

                EffectManager.SimpleEffect(_procEffectPrefab, self.target.transform.position, Quaternion.identity, true);
            }
        }
        #endregion

        public sealed class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.ErraticGadget;

            private Transform muzzleTransform;
            private ChildLocator childLocator;
            public Transform GetMuzzleTransform()
            {
                if (muzzleTransform != null) return muzzleTransform;

                if (body.modelLocator && body.modelLocator.modelTransform)
                {
                    List<GameObject> displays = body.modelLocator.modelTransform.GetComponent<CharacterModel>().GetItemDisplayObjects(SS2Content.Items.ErraticGadget.itemIndex);
                    foreach (GameObject display in displays)
                    {
                        this.childLocator = display.GetComponent<ChildLocator>();
                        Transform muzzle = childLocator.FindChild("Muzzle");
                        if (muzzle)
                        {
                            return muzzle;
                        }
                    }
                }
                return base.body.coreTransform;
            }

            public void OnDamageDealtServer(DamageReport report)
            {
                if (!report.damageInfo.HasModdedDamageType(PROCTYPEAPIWHEN) && Util.CheckRoll(procChance * report.damageInfo.procCoefficient * 100f, body.master))
                {
                    // 1, 3, 5...
                    int bouncesRemaining = bounceTargets + 2 * (this.stack - 1);
                    GadgetLightningOrb orb = new GadgetLightningOrb();
                    orb.duration = 0.2f;
                    orb.bouncesRemaining = bouncesRemaining;
                    orb.range = 24f;
                    orb.damageCoefficientPerBounce = 1f;
                    orb.damageValue = body.damage * damageCoefficient;
                    orb.damageType = DamageType.Generic;
                    orb.isCrit = body.RollCrit();
                    orb.damageColorIndex = DamageColorIndex.Item;
                    orb.procCoefficient = 1f;
                    orb.origin = GetMuzzleTransform().position;
                    orb.teamIndex = body.teamComponent.teamIndex;
                    orb.attacker = body.gameObject;
                    orb.procChainMask = default(ProcChainMask); //////////////////////////////////////////////////////////////////////////////
                    orb.bouncedObjects = new List<HealthComponent>();
                    orb.target = report.victimBody.mainHurtBox;

                    OrbManager.instance.AddOrb(orb);

                    EffectManager.SimpleMuzzleFlash(_displayEffectPrefab, this.childLocator.gameObject, "Muzzle", true);
                }
            }
        }

        public class GadgetLightningOrb : Orb
        {
            public override void Begin()
            {
                if (duration <= 0) duration = 0.01f; //??????????????????????????
                base.duration = duration;
                EffectData effectData = new EffectData
                {
                    origin = this.origin,
                    genericFloat = base.duration
                };
                effectData.SetHurtBoxReference(this.target);
                EffectManager.SpawnEffect(_orbEffectPrefab, effectData, true);
            }

            public override void OnArrival()
            {
                if (this.target)
                {
                    HealthComponent healthComponent = this.target.healthComponent;
                    if (healthComponent)
                    {
                        DamageInfo damageInfo = new DamageInfo();
                        damageInfo.damage = this.damageValue;
                        damageInfo.attacker = this.attacker;
                        damageInfo.inflictor = this.inflictor;
                        damageInfo.force = Vector3.zero;
                        damageInfo.crit = this.isCrit;
                        damageInfo.procChainMask = this.procChainMask;
                        damageInfo.procCoefficient = this.procCoefficient;
                        damageInfo.position = this.target.transform.position;
                        damageInfo.damageColorIndex = this.damageColorIndex;
                        damageInfo.damageType = this.damageType;
                        damageInfo.AddModdedDamageType(PROCTYPEAPIWHEN);
                        healthComponent.TakeDamage(damageInfo);
                        GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                        GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
                    }
                    if (this.bouncesRemaining > 0)
                    {
                        if (this.bouncedObjects != null)
                        {
                            if (this.canBounceOnSameTarget)
                            {
                                this.bouncedObjects.Clear();
                            }
                            this.bouncedObjects.Add(this.target.healthComponent);
                        }
                        HurtBox hurtBox = this.PickNextTarget(this.target.transform.position);
                        if (hurtBox)
                        {
                            GadgetLightningOrb lightningOrb = new GadgetLightningOrb();
                            lightningOrb.search = this.search;
                            lightningOrb.duration = this.duration;
                            lightningOrb.origin = this.target.transform.position;
                            lightningOrb.target = hurtBox;
                            lightningOrb.attacker = this.attacker;
                            lightningOrb.inflictor = this.inflictor;
                            lightningOrb.teamIndex = this.teamIndex;
                            lightningOrb.damageValue = this.damageValue * this.damageCoefficientPerBounce;
                            lightningOrb.bouncesRemaining = this.bouncesRemaining - 1;
                            lightningOrb.isCrit = this.isCrit;
                            lightningOrb.bouncedObjects = this.bouncedObjects;
                            lightningOrb.procChainMask = this.procChainMask;
                            lightningOrb.procCoefficient = this.procCoefficient;
                            lightningOrb.damageColorIndex = this.damageColorIndex;
                            lightningOrb.damageCoefficientPerBounce = this.damageCoefficientPerBounce;
                            lightningOrb.range = this.range;
                            lightningOrb.damageType = this.damageType;
                            OrbManager.instance.AddOrb(lightningOrb);
                        }
                    }

                }
            }
            public HurtBox PickNextTarget(Vector3 position)
            {
                if (this.search == null)
                {
                    this.search = new BullseyeSearch();
                }
                float range = this.range;
                if (this.target && this.target.healthComponent)
                {
                    range += this.target.healthComponent.body.radius;
                }
                this.search.searchOrigin = position;
                this.search.searchDirection = Vector3.zero;
                this.search.teamMaskFilter = TeamMask.allButNeutral;
                this.search.teamMaskFilter.RemoveTeam(this.teamIndex);
                this.search.filterByLoS = false;
                this.search.sortMode = BullseyeSearch.SortMode.Distance;
                this.search.maxDistanceFilter = range;
                this.search.RefreshCandidates();
                HurtBox hurtBox = (from v in this.search.GetResults()
                                   where !this.bouncedObjects.Contains(v.healthComponent)
                                   select v).FirstOrDefault<HurtBox>();
                if (hurtBox)
                {
                    this.bouncedObjects.Add(hurtBox.healthComponent);
                }
                return hurtBox;
            }
            public bool canBounceOnSameTarget;

            public float damageValue;

            public GameObject attacker;

            public GameObject inflictor;

            public int bouncesRemaining;

            public List<HealthComponent> bouncedObjects;

            public TeamIndex teamIndex;

            public bool isCrit;

            public ProcChainMask procChainMask;

            public float procCoefficient = 1f;

            public DamageColorIndex damageColorIndex;

            public float range = 20f;

            public float damageCoefficientPerBounce = 1f;

            public DamageType damageType;

            public BullseyeSearch search;
        }
    }
}