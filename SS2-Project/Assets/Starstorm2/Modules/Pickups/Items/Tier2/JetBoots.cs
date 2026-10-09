using RoR2;
using RoR2.Items;
using UnityEngine;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using EntityStates;
using MSU;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using System.Reflection;
using System.Linq;
using UnityEngine.Networking;

namespace SS2.Items
{
    public sealed class JetBoots : SS2Item, IContentPackModifier
    {
        private const string token = "SS2_ITEM_JETBOOTS_DESC";

        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acJetBoots", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Base damage of Prototype Jet Boots' explosion. Burn damage deals an additional 50% of this value. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float baseDamage = 4f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Stacking damage of Prototype Jet Boots' explosion. Burn damage deals an additional 50% of this value. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float stackDamage = 2f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Base radius of Prototype Jet Boot's explosion, in meters.")]
        [FormatToken(token, 2)]
        public static float baseRadius = 7.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Stacking radius of Prototype Jet Boots' explosion, in meters.")]
        [FormatToken(token, 3)]
        public static float stackRadius = 2.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Cooldown of Prototype Jet Boots' bonus jump, in seconds.")]
        [FormatToken(token, 4)]
        public static int jumpCooldown = 10;

        public static float jumpHorizontalBonus = 1.5f;
        public static float jumpVerticalBonus = 1.5f;

        private static GameObject _explosionEffectPrefab = GlobalEventManager.CommonAssets.igniteOnKillExplosionEffectPrefab;// ;
        private static GameObject _tracerPrefab;
        private static GameObject _muzzleFlashPrefab;
        private static GameObject _effectPrefab;

        public override void Initialize()
        {
            _tracerPrefab = AssetCollection.FindAsset<GameObject>("TracerJetBoots");
            _muzzleFlashPrefab = AssetCollection.FindAsset<GameObject>("MuzzleflashJetBoots");
            _effectPrefab = AssetCollection.FindAsset<GameObject>("JetBootsEffect");

            On.EntityStates.GenericCharacterMain.ProcessJump_bool += ProcessJump;
        }

        private void ProcessJump(On.EntityStates.GenericCharacterMain.orig_ProcessJump_bool orig, GenericCharacterMain self, bool ignoreRequirements)
        {
            bool isBoots = false;
            JetBoots.Behavior behavior = self.GetComponent<JetBoots.Behavior>();

            // if we would be doing a feather jump, and jetboots is ready, do jetboots instead.
            if (self.hasCharacterMotor && self.characterMotor.jumpCount >= self.characterBody.baseJumpCount)
            {
                if (behavior && behavior.JumpReady())
                {
                    isBoots = true;
                }
            }

            if (!isBoots)
            {
                orig(self, ignoreRequirements);
                return;
            }

            // do boots jump
            // ignores jump count checks, and doesnt increment characterMotor.jumpCount 
            if (isBoots && self.hasCharacterMotor)
            {
                if (self.jumpInputReceived && self.characterBody)
                {
                    // simplified jump behavior from GenericCharacterMain
                    float horizontalBonus = jumpHorizontalBonus;
                    float verticalBonus = jumpVerticalBonus;

                    GenericCharacterMain.ApplyJumpVelocity(self.characterMotor, self.characterBody, horizontalBonus, verticalBonus, false);
                    if (self.sfxLocator && !string.IsNullOrEmpty(self.sfxLocator.jumpSound))
                    {
                        Util.PlaySound(self.sfxLocator.jumpSound, self.outer.gameObject);
                    }
                    if (self.hasModelAnimator)
                    {
                        int layerIndex = self.modelAnimator.GetLayerIndex("Body");
                        if (layerIndex >= 0)
                        {
                            self.modelAnimator.CrossFadeInFixedTime("Jump", self.smoothingParameters.intoJumpTransitionTime, layerIndex);
                        }
                    }

                    self.characterBody.TriggerJumpEventGlobally();
                    if (behavior) behavior.OnJump();


                    // splosion
                    CharacterBody body = self.characterBody;
                    Ray footRay = new Ray(body.footPosition, Vector3.down);
                    bool hit = Util.CharacterRaycast(body.gameObject, footRay, out RaycastHit hitInfo, 50f, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal);
                    Vector3 position = hit ? hitInfo.point : footRay.GetPoint(50f);

                    int stack = body.inventory ? body.inventory.GetItemCountEffective(SS2Content.Items.JetBoots) : 1;
                    float blastRadius = baseRadius + stackRadius * (stack - 1);


                    EffectManager.SimpleEffect(_effectPrefab, body.footPosition, Quaternion.identity, true);
                    List<Transform> muzzles = behavior ? behavior.GetMuzzleTransforms() : new List<Transform> { body.coreTransform };
                    foreach (Transform muzzle in muzzles)
                    {
                        // overkill but i want it to look nice               
                        bool bootHit = Util.CharacterRaycast(body.gameObject, new Ray(muzzle.position, Vector3.down), out RaycastHit bootHitInfo, 50f, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal);
                        EffectData effectData = new EffectData
                        {
                            origin = bootHit ? bootHitInfo.point : footRay.GetPoint(50f),
                            start = muzzle.position,
                        };
                        EffectManager.SpawnEffect(_tracerPrefab, effectData, true);
                        EffectManager.SimpleEffect(_muzzleFlashPrefab, muzzle.position, muzzle.rotation, true);
                    }
                    EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.igniteOnKillExplosionEffectPrefab, new EffectData
                    {
                        origin = position,
                        scale = blastRadius,
                    }, true);


                    new BlastAttack
                    {
                        attacker = body.gameObject,
                        inflictor = body.gameObject,
                        attackerFiltering = AttackerFiltering.Default,
                        position = position,
                        teamIndex = body.teamComponent.teamIndex,
                        radius = blastRadius,
                        baseDamage = body.damage * (baseDamage + (stackDamage * (stack - 1))),
                        damageType = DamageType.IgniteOnHit,
                        crit = body.RollCrit(),
                        procCoefficient = 1f,
                        procChainMask = default(ProcChainMask),
                        baseForce = 600f,
                        damageColorIndex = DamageColorIndex.Item,
                        falloffModel = BlastAttack.FalloffModel.None,
                        losType = BlastAttack.LoSType.NearestHit,
                    }.Fire();
                }
            }
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.JetBoots;

            private List<Transform> muzzleTransforms;
            private float cooldownTimer;
            public List<Transform> GetMuzzleTransforms()
            {
                if (muzzleTransforms != null) return muzzleTransforms;

                muzzleTransforms = new List<Transform>();
                if (body.modelLocator && body.modelLocator.modelTransform)
                {
                    List<GameObject> displays = body.modelLocator.modelTransform.GetComponent<CharacterModel>().GetItemDisplayObjects(SS2Content.Items.JetBoots.itemIndex);
                    foreach (GameObject display in displays)
                    {
                        Transform muzzle = display.GetComponent<ChildLocator>().FindChild("Muzzle");
                        if (muzzle) muzzleTransforms.Add(muzzle);
                    }
                }
                return muzzleTransforms;
            }
            public bool JumpReady()
            {
                return cooldownTimer <= 0;
            }
            public void OnJump()
            {
                if (body.hasEffectiveAuthority)
                {
                    cooldownTimer = jumpCooldown;
                }
                int buffCount = 0;
                while (buffCount <= jumpCooldown)
                {
                    body.AddTimedBuffAuthority(SS2Content.Buffs.BuffJetBootsCooldown.buffIndex, buffCount);
                    buffCount++;
                }
            }

            private void OnEnable()
            {
                if (NetworkServer.active)
                {
                    body.SetBuffCount(SS2Content.Buffs.BuffJetBootsReady.buffIndex, 1);
                }
            }

            

            private void FixedUpdate()
            {
                if (body.hasEffectiveAuthority)
                {
                    cooldownTimer -= Time.fixedDeltaTime;
                }
                if(NetworkServer.active)
                {
                    if (body.HasBuff(SS2Content.Buffs.BuffJetBootsCooldown) && body.HasBuff(SS2Content.Buffs.BuffJetBootsReady))
                    {
                        body.RemoveBuff(SS2Content.Buffs.BuffJetBootsReady);
                    }
                    else if (!body.HasBuff(SS2Content.Buffs.BuffJetBootsCooldown) && !body.HasBuff(SS2Content.Buffs.BuffJetBootsReady))
                    {
                        body.AddBuff(SS2Content.Buffs.BuffJetBootsReady);
                    }
                }
            }

            private void OnDisable()
            {
                if(NetworkServer.active)
                {
                    if (body.HasBuff(SS2Content.Buffs.BuffJetBootsReady))
                        body.RemoveBuff(SS2Content.Buffs.BuffJetBootsReady);
                }
            }
        }
    }
}
