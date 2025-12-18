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
        public static float baseDamage = 5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Base radius of Prototype Jet Boot's explosion, in meters.")]
        [FormatToken(token, 1)]
        public static float baseRadius = 7.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Stacking radius of Prototype Jet Boots' explosion, in meters.")]
        [FormatToken(token, 2)]
        public static float stackRadius = 5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Cooldown of Prototype Jet Boots' bonus jump, in seconds.")]
        [FormatToken(token, 3)]
        public static float jumpCooldown = 5f;

        private static GameObject _explosionEffectPrefab = GlobalEventManager.CommonAssets.igniteOnKillExplosionEffectPrefab;// ;
        private static GameObject _tracerPrefab;
        private static GameObject _muzzleFlashPrefab;
        private static GameObject _effectPrefab;

        public override void Initialize()
        {
            _tracerPrefab = AssetCollection.FindAsset<GameObject>("TracerJetBoots");
            _muzzleFlashPrefab = AssetCollection.FindAsset<GameObject>("MuzzleflashJetBoots");
            _effectPrefab = AssetCollection.FindAsset<GameObject>("JetBootsEffect");

            // this will interfere with other bonus jump items but they can be unified in a similar way to this
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateMaxJumpCount;
            IL.EntityStates.GenericCharacterMain.ProcessJump_bool += ProcessJumpBoolHook;
        }

        private void RecalculateMaxJumpCount(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.jumpCountAdd += sender.GetBuffCount(SS2Content.Buffs.BuffJetBootsReady);
        }

        private void ProcessJumpBoolHook(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            // Find the first instance of characterBody.baseJumpCount and override its value
            // if (base.characterMotor.jumpCount >= base.characterBody.baseJumpCount)
            //
            // TODO: I wonder how likely it is for the code to change and reference this
            // field earlier in the method than where we want to be. It might be a good
            // idea to first go to something nearby preceeding this, such as
            // GetItemCountEffective(RoR2Content.Items.JumpBoost).
            bool b = c.TryGotoNext(MoveType.After,
                x => x.MatchLdfld<CharacterBody>(nameof(CharacterBody.baseJumpCount)));
            if (b)
            {
                // replace baseJumpCount with (baseJumpCount + buffCount)
                // ^prevents hopoo feather from activating
                // if we had the buff, do the jump
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<int, GenericCharacterMain, int>>((baseJumpCount, characterMainState) =>
                {
                    CharacterBody body = characterMainState.characterBody;

                    int buffCount = body.GetBuffCount(SS2Content.Buffs.BuffJetBootsReady);

                    if (buffCount > 0 && body.characterMotor && body.characterMotor.jumpCount >= body.baseJumpCount)
                    {
                        DoJumpAuthority(body);
                        // OOPS i fucked up. tying maxjumpcount to the Ready buff makes it so two jumps are technically "used" here
                        body.characterMotor.jumpCount--; // jank fix to that^^
                    }
                    //fucked up again. need to always skip feathers if we have buff. (* bignumber) is jank fix
                    return body.baseJumpCount + buffCount * 7165471;
                });
            }
            else
            {
                //SS2Log.Fatal("JetBoots.ProcessJumpHook: ILHook failed.");
            }
        }
        private void DoJumpAuthority(CharacterBody body)
        {
            //SS2Log.Info("DEBUGGING Entering JetBoots jump authority");

            if (!body.characterMotor) return;

            body.SetBuffCount(SS2Content.Buffs.BuffJetBootsReady.buffIndex, 0); // dont care FUCK YOU

            Ray footRay = new Ray(body.footPosition, Vector3.down);
            bool hit = Util.CharacterRaycast(body.gameObject, footRay, out RaycastHit hitInfo, 50f, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal);
            Vector3 position = hit ? hitInfo.point : footRay.GetPoint(50f);

            int stack = body.inventory ? body.inventory.GetItemCount(ItemDef) : 1;
            float blastRadius = baseRadius + stackRadius * (stack - 1);


            EffectManager.SimpleEffect(_effectPrefab, body.footPosition, Quaternion.identity, true);

            JetBoots.Behavior behavior = body.GetComponent<JetBoots.Behavior>();
            if (behavior) behavior.canHaveReadyBuff = false;
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
                //effectData.SetChildLocatorTransformReference(muzzle.parent.gameObject, 0); // im retarded
                EffectManager.SpawnEffect(_tracerPrefab, effectData, true);
                EffectManager.SimpleEffect(_muzzleFlashPrefab, muzzle.position, muzzle.rotation, true);
            }

            //SS2Log.Info("DEBUGGING Getting blast position");

            EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.igniteOnKillExplosionEffectPrefab, new EffectData ///////////////////////////////////////////
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
                baseDamage = body.damage * baseDamage,
                damageType = DamageType.IgniteOnHit,
                crit = body.RollCrit(),
                procCoefficient = 1f,
                procChainMask = default(ProcChainMask),
                baseForce = 600f,
                damageColorIndex = DamageColorIndex.Item,
                falloffModel = BlastAttack.FalloffModel.None,
                losType = BlastAttack.LoSType.NearestHit,
            }.Fire();

            //SS2Log.Info("DEBUGGING Exiting JetBoots jump authority");
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.JetBoots;

            private List<Transform> muzzleTransforms;
            public float cooldownTimer;
            public bool canHaveReadyBuff; // server overwrites client buffs (obviously) so we need to keep giving it back per frame lol
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

            private void Start()
            {
                //SS2Log.Info("DEBUGGING Entering JetBoots behavior start");
                if (!this.body.hasEffectiveAuthority) return;

                this.body.SetBuffCount(SS2Content.Buffs.BuffJetBootsReady.buffIndex, 1);
                canHaveReadyBuff = true;
                if (this.body.characterMotor)
                {
                    //SS2Log.Info("DEBUGGING Adding hit ground authority");
                    this.body.characterMotor.onHitGroundAuthority += (ref CharacterMotor.HitGroundInfo info) =>
                    {
                        // if the jump has been used && the cooldown hasnt been started
                        if (!this.body.HasBuff(SS2Content.Buffs.BuffJetBootsReady) && !this.body.HasBuff(SS2Content.Buffs.BuffJetBootsCooldown))
                        {
                            //SS2Log.Info("DEBUGGING hitground authority");
                            this.cooldownTimer = 5f;
                            canHaveReadyBuff = true;
                        }
                    
                    };
                }
            }

            private void FixedUpdate()
            {
                if(this.body.hasEffectiveAuthority)
                {
                    cooldownTimer -= Time.fixedDeltaTime;
                    int stack = Mathf.CeilToInt(cooldownTimer);
                    if (stack <= 0 && canHaveReadyBuff)
                    {
                        //SS2Log.Info("DEBUGGING Setting buff to 1");
                        body.SetBuffCount(SS2Content.Buffs.BuffJetBootsReady.buffIndex, 1); // :(
                    }

                    //SS2Log.Info("DEBUGGING Setting cooldown buff to " + stack);
                    this.body.SetBuffCount(SS2Content.Buffs.BuffJetBootsCooldown.buffIndex, stack < 0 ? 0 : stack);// dont care stfu fuck you
                }
            }

            private void OnDestroy()
            {
                if(NetworkServer.active)
                {
                    if (this.body.HasBuff(SS2Content.Buffs.BuffJetBootsReady))
                        this.body.RemoveBuff(SS2Content.Buffs.BuffJetBootsReady);
                }
            }
        }
    }
}
