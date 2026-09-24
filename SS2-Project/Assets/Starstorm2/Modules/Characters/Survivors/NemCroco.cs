using MSU;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2.ContentManagement;
using R2API;
using EntityStates;
using MSU.Config;
using RoR2.Skills;
using UnityEngine.Networking;
using RoR2.UI;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using RoR2.Orbs;
using RoR2.Projectile;
using RiskOfOptions.Resources;

namespace SS2.Survivors
{
    public sealed class NemCroco : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acNemCroco", SS2Bundle.NemCroco);

        public static DamageAPI.ModdedDamageType NemesisPoisonOnHit { get; private set; }
        public static DamageAPI.ModdedDamageType DamageShareOnHit { get; private set; }
        public static DamageAPI.ModdedDamageType NemCrocoExecute { get; private set; }
        public static DamageAPI.ModdedDamageType RadiationOnHit { get; private set; }
        public static DamageAPI.ModdedDamageType Radiation { get; private set; }

        public static DotController.DotIndex PoisonDotIndex{ get; private set; }
        public static DotController.DotIndex RadiationDotIndex { get; private set; }

        private static ModdedProcType DamageShare;

        private static float poisonDamageCoefficient = 2.4f;
        private static float poisonTickDamageCoefficient = 0.8f;

        private static float radiationDamageCoefficient = 1f;
        private static float radiationDuration = 5f;
        private static float radiationTickSpeed = 0.2f;
        private static DamageColorIndex radiationDamageColor = DamageColorIndex.WeakPoint;

        private static bool radiationNoiseActive = true;
        private static float radiationNoiseCutoffMax = 1f;
        private static float radiationNoiseCutoffMin = 0f;
        private static float radiationNoiseStrength = 0.8f;
        private static float radiationNoiseFrequency = 3f;

        private static GameObject executeEffectPrefab;

        private static float damageShareDuration = 7f;
        private static float damageShareCoefficient = 0.5f;
        private static float damageShareCoefficientPerBounce = 0.5f;
        private static float damageShareProcCoefficient = 0.5f;
        private static float damageShareRadius = 12f;
        private static float damageShareChainRadius = 18f;
        private static float damageShareOrbSpeed = 90f;
        private static bool damageShareNonLethal = true;
        private static DamageColorIndex damageShareColor = DamageColorIndex.WeakPoint;
        private static GameObject damageShareOrbEffectPrefab;

        public static RoR2.UI.HealthBarStyle.BarStyle RadiationBarStyle;

        public override bool IsAvailable(ContentPack contentPack)
        {
            return SS2Config.enableBeta && base.IsAvailable(contentPack);
        }
        public override void Initialize()
        {
            CharacterBody cb = CharacterPrefab.GetComponent<CharacterBody>();
            if (cb)
            {
                UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/SurvivorPod/SurvivorPod.prefab").Completed += (x) => { cb.preferredPodPrefab = x.Result; };
            }

            damageShareOrbEffectPrefab = SS2Assets.LoadAsset<GameObject>("DamageShareOrbEffect", SS2Bundle.NemCroco);
            executeEffectPrefab = SS2Assets.LoadAsset<GameObject>("NemCrocoExecuteEffect", SS2Bundle.NemCroco);
            // lol
            NemesisPoisonOnHit = R2API.DamageAPI.ReserveDamageType(); // normal dot, but it has a proc coefficient
            DamageShareOnHit = R2API.DamageAPI.ReserveDamageType(); // applies damage sharing debuff
            NemCrocoExecute = R2API.DamageAPI.ReserveDamageType();  // kills if health is less than accumulated radiation damage
            RadiationOnHit = R2API.DamageAPI.ReserveDamageType(); // applies a DoT that deals radiation damage
            Radiation = R2API.DamageAPI.ReserveDamageType(); // the damage type of the radiation DoT. applies a buff instead of taking away health.

            PoisonDotIndex = DotAPI.RegisterDotDef(0.33f, poisonTickDamageCoefficient, DamageColorIndex.Poison, AssetCollection.FindAsset<BuffDef>("bdNemCrocoPoison"));
            RadiationDotIndex = DotAPI.RegisterDotDef(radiationTickSpeed, 1f, DamageColorIndex.WeakPoint, AssetCollection.FindAsset<BuffDef>("bdNemCrocoRadiation"), customDotBehaviour: new DotAPI.CustomDotBehaviour(AddDot));

            DamageShare = R2API.ProcTypeAPI.ReserveProcType();

            var projectileDamage = SS2Assets.LoadAsset<GameObject>("NemCrocoDiseaseProjectile", SS2Bundle.NemCroco)?.GetComponent<ProjectileDamage>();
            projectileDamage?.damageType.AddModdedDamageType(DamageShareOnHit);
            projectileDamage?.damageType.AddModdedDamageType(RadiationOnHit);

            On.RoR2.HealthComponent.TakeDamage += TakeDamage;
            IL.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
            On.RoR2.DotController.EvaluateDotStacksForType += DotController_EvaluateDotStacksForType;
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;

            var executeBarSprite = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texUIHighlightExecute.png").WaitForCompletion();
            RadiationBarStyle = new RoR2.UI.HealthBarStyle.BarStyle
            {
                enabled = true,
                baseColor = new Color(247f / 255f, 156f / 255f, 45f / 255f),
                imageType = UnityEngine.UI.Image.Type.Sliced,
                sizeDelta = 6f,
                sprite = executeBarSprite,
            };
            var barInfo = new RoR2.UI.HealthBar.BarInfo
            {
                enabled = true,
                color = new Color(247f / 255f, 156f / 255f, 45f / 255f),
                imageType = UnityEngine.UI.Image.Type.Sliced,
                sizeDelta = 6f,
                sprite = executeBarSprite,
            };

            var bar = new R2API.HealthBarAPI.BarOverlayInfo
            {
                BarInfo = barInfo, ////// ? 
                ModifyBarInfo = new HealthBarAPI.BarOverlayInfo.ModifyBarInfoCallback(ModifyBarInfo),
                ModifyHealthValues = new HealthBarAPI.BarOverlayInfo.ModifyHealthValuesCallback(ModifyHealthValues),
            };
            R2API.HealthBarAPI.RegisterBarOverlay(bar);
        }


        
        

        // pretty sure damage macros can do this instead. iykyk
        // Allow the Radiation damage type to calculate all of the damage modifiers, but instead of deducting from health, instead add buff stacks equal to the calculated damage. 
        // conveniently, the calculated damage is what gets used to spawn a damage number instead of the health deduction.
        // Also, if the enemy is fully irradiated and is being hit by NemCrocoExecute, set the calculated damage value to the victim's remaining health.
        private void HealthComponent_TakeDamageProcess(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int calculatedDamageVarIndex = -1;
            int healthDeductionVarIndex = -1;

            bool a = c.TryGotoNext(MoveType.After,
                x => x.MatchStfld<HealthComponent>(nameof(HealthComponent.isShieldRegenForced)));
            if (!a)
            {
                SS2Log.Fatal("NemCroco TakeDamageProcess ILHook failed 1");
                return;
            }

            bool b = c.TryGotoPrev(MoveType.After,
                x => x.MatchLdloc(out calculatedDamageVarIndex), // num4, calculated damage
                x => x.MatchStloc(out healthDeductionVarIndex)); // num5, damage to deduct from health
            if (b)
            {
                c.Emit(OpCodes.Ldloc, calculatedDamageVarIndex);
                c.Emit(OpCodes.Ldarg_0); // healthComponent
                c.Emit(OpCodes.Ldarg_1); // damageInfo
                c.EmitDelegate<Func<float, HealthComponent, DamageInfo, float>>((calculatedDamage, victim, damageInfo) =>
                {
                    if (damageInfo.HasModdedDamageType(NemCrocoExecute) && IsFullRadiation(victim))
                    {
                        victim.forceHideBody = true;
                        EffectManager.SimpleEffect(executeEffectPrefab, victim.body.corePosition, Quaternion.identity, true); // TODO: dont force bullseye/core position  for executes!! and move this to nemcroco authority somehow bruh!!
                        return victim.fullCombinedHealth + 1f;  // return calculated damage equal to enemy's remaining health.
                    }

                    return calculatedDamage;
                });
                c.Emit(OpCodes.Stloc, calculatedDamageVarIndex);

                c.Emit(OpCodes.Ldloc, calculatedDamageVarIndex);
                c.Emit(OpCodes.Ldarg_0); // healthComponent
                c.Emit(OpCodes.Ldarg_1); // damageInfo
                c.EmitDelegate<Func<float, HealthComponent, DamageInfo, float>>((calculatedDamage, victim, damageInfo) =>
                {
                    if (damageInfo.HasModdedDamageType(Radiation))
                    {
                        int currentBuffCount = victim.body.GetBuffCount(SS2Content.Buffs.bdNemCrocoRadValue);
                        int buffCount = Mathf.Max(Mathf.CeilToInt(calculatedDamage), 1);
                        victim.body.SetBuffCount(SS2Content.Buffs.bdNemCrocoRadValue.buffIndex, currentBuffCount + buffCount);

                        return 0f; // return zero damage to health
                    }


                    return calculatedDamage;
                });
                c.Emit(OpCodes.Stloc, healthDeductionVarIndex);
            }
            else
            {
                SS2Log.Fatal("NemCroco TakeDamageProcess ILHook failed 2");
            }
        }

        public static void AddDot(DotController dotController, DotController.DotStack dotStack)
        {
            if (dotStack.dotIndex == RadiationDotIndex) // 
            {
                dotStack.AddModdedDamageType(Radiation);
            }
        }

        public static float GetRadiationValue(HealthComponent healthComponent)
        {
            if (!healthComponent) return 0;
            return healthComponent.body.GetBuffCount(SS2Content.Buffs.bdNemCrocoRadValue); // ideally this is a syncvar'd value on healthcomponent instead of a buff
        }
        public static bool IsFullRadiation(HealthComponent healthComponent)
        {
            if (!healthComponent) return false;
            return GetRadiationValue(healthComponent) > healthComponent.health;
        }

        // add radiation damage to health bar
        private static void ModifyBarInfo(HealthBar healthBar, ref HealthBar.BarInfo barInfo)
        {
            float radiation = GetRadiationValue(healthBar.source);
            barInfo.enabled = radiation > 0;
            barInfo.color = RadiationBarStyle.baseColor;
            barInfo.sprite = RadiationBarStyle.sprite;
            barInfo.imageType = RadiationBarStyle.imageType;
            barInfo.sizeDelta = RadiationBarStyle.sizeDelta;
            barInfo.normalizedXMin = 0f; // TODO: ADD ONTO CULL FRACTION ?
            barInfo.normalizedXMax = healthBar.source ? radiation / healthBar.source.fullHealth : 0f;
        }

        // idk what this does
        private static void ModifyHealthValues(HealthBar healthBar, ref float currentHealth, ref float maxHealth)
        {

        }

        // Randomize tick speed of radiation DoT
        private void DotController_EvaluateDotStacksForType(On.RoR2.DotController.orig_EvaluateDotStacksForType orig, DotController self, DotController.DotIndex dotIndex, float dt, out int remainingActive)
        {
            orig(self, dotIndex, dt, out remainingActive);

            // Randomize the tick speed
            if (radiationNoiseActive && dotIndex == RadiationDotIndex)
            {
                var dotDef = DotController.GetDotDef(RadiationDotIndex);
                self.dotTimers[(int)RadiationDotIndex] -= dotDef.interval; // undo the normal interval reset

                // randomize using noise
                float noiseAge = Time.fixedTime * radiationNoiseFrequency;
                float noiseSeed = self.GetInstanceID(); // lolz
                float noise = Mathf.PerlinNoise(noiseAge, noiseSeed); 
                noise = Util.Remap(noise, radiationNoiseCutoffMin, radiationNoiseCutoffMax, 0f, 1f);
                float variance = noise * radiationNoiseStrength * radiationTickSpeed;
                self.dotTimers[(int)RadiationDotIndex] += radiationTickSpeed + variance;
            }
            
        }

        // make poison have a proc coefficient
        private void TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            bool triggerGougeProc = false;
            if (NetworkServer.active)
            {
                if (damageInfo.dotIndex == PoisonDotIndex && damageInfo.procCoefficient == 0f && self.alive)
                {
                    if (damageInfo.attacker)
                    {
                        CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                        if (attackerBody)
                        {
                            damageInfo.crit = Util.CheckRoll(attackerBody.crit, attackerBody.master);
                        }
                    }
                    damageInfo.procCoefficient = 1f;
                    triggerGougeProc = true;
                }
            }

            orig(self, damageInfo);

            if (NetworkServer.active && !damageInfo.rejected && self.alive)
            {
                if (triggerGougeProc)
                {
                    GlobalEventManager.instance.OnHitEnemy(damageInfo, self.gameObject);
                }
            }
        }

        // apply DoTs
        // spawn damage share orbs
        // apply damageshare
        private void OnServerDamageDealt(DamageReport report)
        {
            var victimBody = report.victimBody;
            var attackerBody = report.attackerBody;
            var damageInfo = report.damageInfo;

            if (DamageAPI.HasModdedDamageType(damageInfo, NemesisPoisonOnHit))
            {
                var dotInfo = new InflictDotInfo()
                {
                    attackerObject = attackerBody.gameObject,
                    victimObject = victimBody.gameObject,
                    dotIndex = PoisonDotIndex,
                    totalDamage = poisonDamageCoefficient * attackerBody.damage,
                    damageMultiplier = 1f,
                    maxStacksFromAttacker = 1,
                };
                DotController.InflictDot(ref dotInfo);
            }

            if (DamageAPI.HasModdedDamageType(damageInfo, RadiationOnHit))
            {
                float targetTotalDamage = report.damageInfo.damage * radiationDamageCoefficient;
                float damageMultiplier = SS2Util.GetDotDamageMultiplier(report.attackerBody, targetTotalDamage, radiationDuration, RadiationDotIndex);
                var dotInfo = new InflictDotInfo()
                {
                    attackerObject = report.attacker,
                    victimObject = report.victim.gameObject,
                    dotIndex = RadiationDotIndex,
                    duration = radiationDuration,
                    damageMultiplier = damageMultiplier,
                };
                DotController.InflictDot(ref dotInfo);
            }

            if (DamageAPI.HasModdedDamageType(damageInfo, DamageShareOnHit))
            {
                victimBody.AddTimedBuff(SS2Content.Buffs.bdNemCrocoDamageShare, damageShareDuration);
            }

            if (report.victimBody && report.victimBody.HasBuff(SS2Content.Buffs.bdNemCrocoDamageShare)
                && damageInfo.procCoefficient > 0 && !damageInfo.procChainMask.HasModdedProc(DamageShare))
            {
                DamageShareOrb orb = new DamageShareOrb();
                orb.speed = damageShareOrbSpeed;
                orb.damageValue = damageInfo.damage * damageShareCoefficient;
                orb.damageCoefficientPerBounce = damageShareCoefficientPerBounce;
                orb.damageType = damageShareNonLethal ? DamageType.NonLethal : DamageType.Generic;
                // add debuffs to damage share
                if (damageInfo.HasModdedDamageType(NemesisPoisonOnHit))
                {
                    orb.damageType.AddModdedDamageType(NemesisPoisonOnHit);
                }
                if (damageInfo.HasModdedDamageType(RadiationOnHit))
                {
                    orb.damageType.AddModdedDamageType(RadiationOnHit);
                }
                orb.isCrit = damageInfo.crit;
                orb.damageColorIndex = damageShareColor;
                orb.procCoefficient = damageShareProcCoefficient * damageInfo.procCoefficient;
                orb.origin = report.damageInfo.position;
                orb.teamIndex = report.attackerTeamIndex;
                orb.attacker = report.attacker;
                damageInfo.procChainMask.AddModdedProc(DamageShare);
                orb.procChainMask = damageInfo.procChainMask;
                orb.bouncedObjects = new List<HealthComponent>
                    {
                        report.victim
                    };
                HurtBox hurtbox = orb.PickNextTarget(damageInfo.position, report.victim);
                if (hurtbox)
                {
                    orb.target = hurtbox;
                    OrbManager.instance.AddOrb(orb);
                }
            }
        }

        public class DamageShareOrb : Orb
        {
            public float speed;
            public float damageValue;
            public GameObject attacker;
            public GameObject inflictor;
            public List<HealthComponent> bouncedObjects;
            public TeamIndex teamIndex;
            public bool isCrit;
            public ProcChainMask procChainMask;
            public float procCoefficient = 1f;
            public DamageColorIndex damageColorIndex;
            public float damageCoefficientPerBounce = 1f;
            public DamageTypeCombo damageType;
            private BullseyeSearch search;

            public override void Begin()
            {
                float distanceToTarget = 0f;
                if (target) distanceToTarget = Vector3.Distance(target.transform.position, origin);
                duration = distanceToTarget / speed;

                if (damageShareOrbEffectPrefab)
                {
                    EffectData effectData = new EffectData
                    {
                        origin = origin,
                        genericFloat = base.duration
                    };
                    effectData.SetHurtBoxReference(target);

                    EffectManager.SpawnEffect(damageShareOrbEffectPrefab, effectData, true);
                }

            }

            public override void OnArrival()
            {
                if (target)
                {
                    HealthComponent healthComponent = target.healthComponent;
                    if (healthComponent)
                    {
                        DamageInfo damageInfo = new DamageInfo();
                        damageInfo.damage = damageValue;
                        damageInfo.attacker = attacker;
                        damageInfo.inflictor = inflictor;
                        damageInfo.force = Vector3.zero;
                        damageInfo.crit = isCrit;
                        damageInfo.procChainMask = procChainMask;
                        damageInfo.procCoefficient = procCoefficient;
                        damageInfo.position = target.transform.position;
                        damageInfo.damageColorIndex = damageColorIndex;
                        damageInfo.damageType = damageType;
                        healthComponent.TakeDamage(damageInfo);
                        GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                        GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
                    }

                    if (healthComponent.body.HasBuff(SS2Content.Buffs.bdNemCrocoDamageShare))
                    {
                        if (bouncedObjects != null)
                        {
                            bouncedObjects.Add(target.healthComponent);
                        }
                        HurtBox hurtBox = PickNextTarget(target.transform.position, healthComponent);
                        if (hurtBox)
                        {
                            DamageShareOrb orb = new DamageShareOrb();
                            orb.search = search;
                            orb.origin = target.transform.position;
                            orb.target = hurtBox;
                            orb.attacker = attacker;
                            orb.inflictor = inflictor;
                            orb.teamIndex = teamIndex;
                            orb.damageValue = damageValue * damageCoefficientPerBounce;
                            orb.isCrit = isCrit;
                            orb.bouncedObjects = bouncedObjects;
                            orb.procChainMask = procChainMask;
                            orb.procCoefficient = procCoefficient;
                            orb.damageColorIndex = damageColorIndex;
                            orb.damageCoefficientPerBounce = damageCoefficientPerBounce;
                            orb.damageType = damageType;
                            OrbManager.instance.AddOrb(orb);
                        }
                    }

                }
            }
            public HurtBox PickNextTarget(Vector3 position, HealthComponent currentVictim)
            {
                if (search == null)
                {
                    search = new BullseyeSearch();
                }

                HurtBox target = null;
                float bodyRadius = 0f;
                if (currentVictim && currentVictim.body)
                {
                    bodyRadius = currentVictim.body.radius;
                }
                search.searchOrigin = position;
                search.searchDirection = Vector3.zero;
                search.teamMaskFilter = TeamMask.allButNeutral;
                search.teamMaskFilter.RemoveTeam(teamIndex);
                search.filterByLoS = false;
                search.sortMode = BullseyeSearch.SortMode.Distance;

                // First, search in a larger radius for other bodies with the damage share debuff
                search.maxDistanceFilter = bodyRadius + damageShareChainRadius;
                search.RefreshCandidates();
                foreach (HurtBox hurtBox in search.GetResults())
                {
                    if (hurtBox.healthComponent.body.HasBuff(SS2Content.Buffs.bdNemCrocoDamageShare) && !bouncedObjects.Contains(hurtBox.healthComponent))
                    {
                        target = hurtBox;
                    }
                }

                if (!target)
                {
                    search.maxDistanceFilter = bodyRadius + damageShareRadius;
                    search.RefreshCandidates();
                    foreach (HurtBox hurtBox in search.GetResults())
                    {
                        if (!bouncedObjects.Contains(hurtBox.healthComponent))
                        {
                            target = hurtBox;
                        }
                    }
                }

                if (target)
                {
                    bouncedObjects.Add(target.healthComponent);
                }
                return target;
            }
        }
    }
}

