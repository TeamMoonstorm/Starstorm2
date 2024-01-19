using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using EntityStates;
namespace Moonstorm.Starstorm2.Items
{
    public sealed class JetBoots : ItemBase
    {
        private const string token = "SS2_ITEM_JETBOOTS_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("JetBoots", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Base radius of Prototype Jet Boot's explosion, in meters.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float baseRadius = 7.5f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Stacking radius of Prototype Jet Boots' explosion, in meters.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float stackRadius = 2.5f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Base damage of Prototype Jet Boots' explosion. Burn damage deals an additional 50% of this value. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 2, "100")]
        public static float baseDamage = 5f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Cooldown of Prototype Jet Boots' bonus jump, in seconds.")]
        [TokenModifier(token, StatTypes.MultiplyByN, 3, "100")]
        public static float jumpCooldown = 5f;

        public static GameObject explosionEffectPrefab = GlobalEventManager.CommonAssets.igniteOnKillExplosionEffectPrefab;// SS2Assets.LoadAsset<GameObject>("JetBootsExplosion", SS2Bundle.Items);
        public static GameObject tracerPrefab = SS2Assets.LoadAsset<GameObject>("TracerJetBoots", SS2Bundle.Items);
        public static GameObject muzzleFlashPrefab = SS2Assets.LoadAsset<GameObject>("MuzzleflashJetBoots", SS2Bundle.Items);

        public BuffDef buffCooldown { get; } = SS2Assets.LoadAsset<BuffDef>("BuffJetBootsCooldown", SS2Bundle.Items);
        public override void Initialize()
        {
            IL.EntityStates.GenericCharacterMain.ProcessJump += ProcessJumpHook;
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
                    if(buffCount > 0)
                    {
                        DoJump(body);
                        body.RemoveBuff(SS2Content.Buffs.BuffJetBootsReady);
                    }
                    return body.baseJumpCount + buffCount; 
                });
            }
            else
            {
                SS2Log.Warning("JetBoots.ProcessJumpHook: ILHook failed.");
            }
        }

        private void DoJump(CharacterBody body)
        {
            if (!body.characterMotor) return;

            SS2Log.Info("yump :3");
            GenericCharacterMain.ApplyJumpVelocity(body.characterMotor, body, 1.5f, 1.5f, false);

            Ray footRay = new Ray(body.footPosition, Vector3.down);
            bool hit = Util.CharacterRaycast(body.gameObject, footRay, out RaycastHit hitInfo, 50f, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal);
            Vector3 position = hit ? hitInfo.point : footRay.GetPoint(50f);

            int stack = body.inventory ? body.inventory.GetItemCount(ItemDef) : 1;
            float blastRadius = baseRadius + stackRadius * (stack - 1);

            EffectManager.SpawnEffect(explosionEffectPrefab, new EffectData
            {
                origin = position,
                scale = blastRadius,
            }, true);

            JetBoots.Behavior behavior = body.GetComponent<JetBoots.Behavior>();
            List<Transform> muzzles = behavior ? behavior.GetMuzzleTransforms() : new List<Transform> { body.coreTransform };
            foreach(Transform muzzle in muzzles)
            {
                EffectData effectData = new EffectData
                {
                    origin = position,
                    start = muzzle.position,
                };
                effectData.SetChildLocatorTransformReference(body.gameObject, 0); // muzzle is always at 0
                EffectManager.SpawnEffect(tracerPrefab, effectData, true);

                EffectManager.SimpleEffect(muzzleFlashPrefab, muzzle.position, muzzle.rotation, true);
            }

            new BlastAttack
            {
                attacker = body.gameObject,
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
                falloffModel = BlastAttack.FalloffModel.Linear,
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
                this.body.AddBuff(SS2Content.Buffs.BuffJetBootsReady);
                if(this.body.characterMotor)
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
        }
    }
}
