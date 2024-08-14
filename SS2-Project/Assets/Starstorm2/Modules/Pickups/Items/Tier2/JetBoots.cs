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

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Base damage of Prototype Jet Boots' explosion. Burn damage deals an additional 50% of this value. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float baseDamage = 5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Base radius of Prototype Jet Boot's explosion, in meters.")]
        [FormatToken(token, 1)]
        public static float baseRadius = 7.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Stacking radius of Prototype Jet Boots' explosion, in meters.")]
        [FormatToken(token, 2)]
        public static float stackRadius = 5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Cooldown of Prototype Jet Boots' bonus jump, in seconds.")]
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
            IL.RoR2.CharacterBody.RecalculateStats += RecalculateStatsHook; // recalculatestatsapi doesnt have maxjumpcount
            IL.EntityStates.GenericCharacterMain.ProcessJump += ProcessJumpHook;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += RechargeBoots;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        private void RechargeBoots(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == SS2Content.Buffs.BuffJetBootsCooldown) self.AddBuff(SS2Content.Buffs.BuffJetBootsReady);
        }

        private void RecalculateStatsHook(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            // maxJumpCount = baseJumpCount + num9;
            bool b = c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CharacterBody>(nameof(CharacterBody.baseJumpCount)));
            if (b)
            {
                c.Emit(OpCodes.Ldarg_0); // body
                c.Emit(OpCodes.Ldsfld, typeof(SS2Content.Buffs).GetField(nameof(SS2Content.Buffs.BuffJetBootsReady))); // load buff
                c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod(nameof(CharacterBody.GetBuffCount), new Type[] { typeof(BuffDef) })); // get loaded buff on body
                c.Emit(OpCodes.Add); // add buff count to jump count
            }
            else
            {
                SS2Log.Fatal("JetBoots.RecalculateStatsHook: ILHook failed.");
            }
        }


        private void ProcessJumpHook(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            // if (base.characterMotor.jumpCount >= base.characterBody.baseJumpCount)
            bool b = c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchCall<EntityState>("get_characterBody"),
                x => x.MatchLdfld<CharacterBody>(nameof(CharacterBody.baseJumpCount)));
            if (b)
            {
                // replace baseJumpCount with (baseJumpCount + buffCount)
                // ^prevents hopo feather from activating
                // if we had the buff, do the jump
                c.Index -= 1;
                c.Remove();
                c.EmitDelegate<Func<CharacterBody, int>>((body) =>
                {
                    int buffCount = body.GetBuffCount(SS2Content.Buffs.BuffJetBootsReady);

                    if (buffCount > 0 && body.characterMotor && body.characterMotor.jumpCount >= body.baseJumpCount)
                    {
                        DoJump(body);
                        // OOPS i fucked up. tying maxjumpcount to the Ready buff makes it so two jumps are technically "used" here
                        body.characterMotor.jumpCount--; // jank fix to that^^
                        body.RemoveBuff(SS2Content.Buffs.BuffJetBootsReady);
                    }
                    //fucked up again. need to always skip feathers if we have buff. (* bignumber) is jank fix
                    return body.baseJumpCount + buffCount * 7165471;
                });
            }
            else
            {
                SS2Log.Fatal("JetBoots.ProcessJumpHook: ILHook failed.");
            }
        }
        private void DoJump(CharacterBody body)
        {
            if (!body.characterMotor) return;
            //GenericCharacterMain.ApplyJumpVelocity(body.characterMotor, body, 1.5f, 1.5f, false);

            Ray footRay = new Ray(body.footPosition, Vector3.down);
            bool hit = Util.CharacterRaycast(body.gameObject, footRay, out RaycastHit hitInfo, 50f, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal);
            Vector3 position = hit ? hitInfo.point : footRay.GetPoint(50f);

            int stack = body.inventory ? body.inventory.GetItemCount(ItemDef) : 1;
            float blastRadius = baseRadius + stackRadius * (stack - 1);


            EffectManager.SimpleEffect(_effectPrefab, body.footPosition, Quaternion.identity, true);

            JetBoots.Behavior behavior = body.GetComponent<JetBoots.Behavior>();
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

            EffectManager.SpawnEffect(_explosionEffectPrefab, new EffectData
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
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.JetBoots;

            private List<Transform> muzzleTransforms;
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
                if (!NetworkServer.active) return;

                this.body.AddBuff(SS2Content.Buffs.BuffJetBootsReady);
                if (this.body.characterMotor)
                {
                    this.body.characterMotor.onHitGroundServer += (ref CharacterMotor.HitGroundInfo info) =>
                    {
                        // if the jump has been used && the cooldown hasnt been started
                        if (!this.body.HasBuff(SS2Content.Buffs.BuffJetBootsReady) && !this.body.HasBuff(SS2Content.Buffs.BuffJetBootsCooldown))
                        {
                            int i = 1;
                            while (i <= jumpCooldown)
                            {
                                this.body.AddTimedBuff(SS2Content.Buffs.BuffJetBootsCooldown, i);
                                i++;
                            }
                        }
                    };
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
