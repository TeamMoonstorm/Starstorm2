using SS2.Components;
using RoR2;
using System;
using UnityEngine;
using MSU;
using R2API;
using RoR2.ContentManagement;
using static R2API.DamageAPI;
using RoR2.Projectile;
using UnityEngine.Networking;

namespace SS2.Survivors
{
    public sealed class Pyro : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acPyro", SS2Bundle.Indev);
        public static ModdedDamageType FlamethrowerDamageType { get; private set; }
        public static ModdedDamageType FireballDamageType { get; private set; }
        public static ModdedDamageType FireballImpactDamageType { get; private set; }

        public static GameObject _pyroBody;
        public static GameObject _hotFireVFX;
        public static GameObject _fireballExplosionVFX;
        public static HeatSkillDef _jetpackOverrideDef;
        public static BuffDef _bdPyroManiac;
        public static BuffDef _bdPyroJet;
        public static BuffDef _bdPyroJetHiddenBoost;

        private BodyIndex pyroIndex;

        public override void Initialize()
        {
            ModifyPrefab();

            FlamethrowerDamageType = ReserveDamageType();
            FireballDamageType = ReserveDamageType();

            _pyroBody = AssetCollection.FindAsset<GameObject>("PyroBody");
            pyroIndex = BodyCatalog.FindBodyIndex(_pyroBody);

            _hotFireVFX = AssetCollection.FindAsset<GameObject>("PyroHotFireVFX");
            _fireballExplosionVFX = AssetCollection.FindAsset<GameObject>("PyroFireballExplosionVFX");
            _bdPyroManiac = AssetCollection.FindAsset<BuffDef>("bdPyroManiac");
            _bdPyroJet = AssetCollection.FindAsset<BuffDef>("bdPyroJet");
            _bdPyroJetHiddenBoost = AssetCollection.FindAsset<BuffDef>("bdPyroJetHidden");
            _jetpackOverrideDef = AssetCollection.FindAsset<HeatSkillDef>("sdPyro3a");

            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.DotController.AddDot_GameObject_float_DotIndex_float_Nullable1_Nullable1_Nullable1 += DotController_AddDot_GameObject_float_DotIndex_float_Nullable1_Nullable1_Nullable1;
        }

        private void DotController_AddDot_GameObject_float_DotIndex_float_Nullable1_Nullable1_Nullable1(On.RoR2.DotController.orig_AddDot_GameObject_float_DotIndex_float_Nullable1_Nullable1_Nullable1 orig, DotController self, GameObject attackerObject, float duration, DotController.DotIndex dotIndex, float damageMultiplier, uint? maxStacksFromAttacker, float? totalDamage, DotController.DotIndex? preUpgradeDotIndex)
        {
            if (self.victimBody.bodyIndex == pyroIndex && (dotIndex == DotController.DotIndex.Burn || dotIndex == DotController.DotIndex.PercentBurn || dotIndex == DotController.DotIndex.StrongerBurn))
            {
                return;
            }

            orig(self, attackerObject, duration, dotIndex, damageMultiplier, maxStacksFromAttacker, totalDamage, preUpgradeDotIndex);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.HasModdedDamageType(FlamethrowerDamageType))
            {
                CharacterBody attackerBody = damageInfo.attacker.transform.GetComponent<CharacterBody>();
                if (attackerBody)
                {
                    PyroController pc = attackerBody.GetComponent<PyroController>();
                    if (pc != null)
                    {
                        float distance = Vector3.Distance(damageInfo.position, attackerBody.corePosition);

                        if (distance < 6f)
                        {
                            damageInfo.damage *= 1.5f;
                            damageInfo.force *= 3f;
                            damageInfo.damageColorIndex = DamageColorIndex.WeakPoint;
                            EffectManager.SimpleEffect(_hotFireVFX, damageInfo.position, Quaternion.identity, true);
                            if (Util.CheckRoll(75f, attackerBody.master) && pc.heat >= 35f)
                                damageInfo.damageType = DamageType.IgniteOnHit;
                        }
                        else if (Util.CheckRoll(50f, attackerBody.master) && pc.heat >= 65f)
                        {
                            damageInfo.damageType = DamageType.IgniteOnHit;
                        }
                    }
                }
            }

            

            if (self.body.bodyIndex == pyroIndex)
            {   
                // only clears visual buff, not dot..
                if (self.body.HasBuff(RoR2Content.Buffs.OnFire) || self.body.HasBuff(DLC1Content.Buffs.StrongerBurn))
                {
                    self.body.SetBuffCount(RoR2Content.Buffs.OnFire.buffIndex, 0);
                    self.body.SetBuffCount(DLC1Content.Buffs.StrongerBurn.buffIndex, 0);
                }
            }

            orig(self, damageInfo);
        }

        public void ModifyPrefab()
        {
            var cb = CharacterPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
        }

        public sealed class PyromaniacBuffBehavior : BaseBuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => _bdPyroManiac;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (hasAnyStacks)
                {
                    args.armorAdd += 4f * characterBody.GetBuffCount(GetBuffDef());
                    args.regenMultAdd += 0.2f * characterBody.GetBuffCount(GetBuffDef());
                }
            }
        }

        public sealed class PyroJetBuffBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => _bdPyroJet;

            private bool isOverridingSkill;
            private HeatSkillDef overrideSkill = _jetpackOverrideDef;
            private SkillLocator skillLocator;
            private Rigidbody rb;
            private float maxFallSpeed = 5f;

            private ParticleSystem hoverL;
            private ParticleSystem hoverR;
            private bool hoverActive;

            public void OnStart()
            {
                skillLocator = characterBody.skillLocator;
                rb = characterBody.rigidbody;

                GameObject charModel = characterBody.modelLocator.modelTransform.gameObject;
                if (charModel != null && charModel.TryGetComponent(out ChildLocator cl))
                {
                    cl.FindChild("HoverLParticles").TryGetComponent(out ParticleSystem left);
                    {
                        hoverL = left;
                    }
                    cl.FindChild("HoverRParticles").TryGetComponent(out ParticleSystem right);
                    {
                        hoverR = right;
                    }
                }
            }

            public void FixedUpdate()
            {
                if (skillLocator == null)
                {
                    skillLocator = characterBody.skillLocator;
                }

                if (hasAnyStacks && !isOverridingSkill)
                {
                    isOverridingSkill = true;

                    skillLocator.utility.SetSkillOverride(this, overrideSkill, GenericSkill.SkillOverridePriority.Contextual);
                }

                if (!hasAnyStacks && isOverridingSkill)
                {
                    isOverridingSkill = false;

                    skillLocator.utility.UnsetSkillOverride(this, overrideSkill, GenericSkill.SkillOverridePriority.Contextual);
                }

                if (characterBody.characterMotor.isGrounded && isOverridingSkill)
                {
                    // clients dont have authority for this normally
                    CmdRemoveBuff();

                    ToggleHover(false);

                    isOverridingSkill = false;

                    skillLocator.utility.UnsetSkillOverride(this, overrideSkill, GenericSkill.SkillOverridePriority.Contextual);
                }
            }

            public void ToggleHover(bool active)
            {
                if (hoverL != null && hoverR != null)
                {
                    hoverActive = active;

                    if (hoverActive)
                    {
                        hoverL.Play();
                        hoverR.Play();
                    }

                    if (!hoverActive)
                    {
                        hoverL.Stop();
                        hoverR.Stop();
                    }
                }
            }

            [Server]
            public void CmdRemoveBuff()
            {
                characterBody.SetBuffCount(buffIndex, 0);
            }
        }
    }
}