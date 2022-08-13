﻿using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class JetBoots : ItemBase
    {
        private const string token = "SS2_ITEM_JETBOOTS_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("JetBoots");

        [ConfigurableField(ConfigDesc = "Base radius of Prototype Jet Boot's explosion, in meters.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float baseRadius = 7.5f;

        [ConfigurableField(ConfigDesc = "Stacking radius of Prototype Jet Boots' explosion, in meters.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float stackRadius = 2.5f;

        [ConfigurableField(ConfigDesc = "Maximum radius of Prototype Jet Boots' explosion, in meters.")]
        public static float maxRadius = 15f;

        [ConfigurableField(ConfigDesc = "Base damage of Prototype Jet Boots' explosion. (1 = 100%)")]
        [TokenModifier(token, StatTypes.Percentage, 2)]
        public static float baseDamage = 1.5f;

        [ConfigurableField(ConfigDesc = "Stacking damage of Prototype Jet Boots' explosion. (1 = 100%)")]
        [TokenModifier(token, StatTypes.Percentage, 3)]
        public static float stackDamage = 1.0f;

        public static GameObject JetBootsBlast;

        //Sometimes, we need these necesary evils, and tbh i'm ok with this one case.
        public override void Initialize()
        {
            JetBootsBlast = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniExplosionVFXCommandoGrenade"), "JetBootsBlast");

            GameObject.DestroyImmediate(JetBootsBlast.transform.Find("ScaledHitsparks 1").gameObject);
            GameObject.DestroyImmediate(JetBootsBlast.transform.Find("UnscaledHitsparks 1").gameObject);
            GameObject.DestroyImmediate(JetBootsBlast.transform.Find("Nova Sphere (1)").gameObject);
            //RemoveEffect(lightJetBootsFX.transform.Find("Point Light"));

            JetBootsBlast.GetComponent<ShakeEmitter>().radius *= 0.5f;
            JetBootsBlast.GetComponent<EffectComponent>().soundName = "JetBootsExplosion";
            JetBootsBlast.GetComponent<EffectComponent>().applyScale = true;
            HG.ArrayUtils.ArrayAppend(ref SS2Content.Instance.SerializableContentPack.effectPrefabs, JetBootsBlast);
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.JetBoots;
            int blastTimes = 0;
            float radius;
            private void Restock(ref CharacterMotor.HitGroundInfo hitGroundInfo)
            {
                blastTimes = 0;
            }
            private void Start()
            {
                if (NetworkServer.active)
                {
                    body.characterMotor.onHitGroundServer += Restock;
                }
                else
                {
                    body.characterMotor.onHitGroundAuthority += Restock;
                }
            }

            private void FixedUpdate()
            {
                if (body.inputBank.jump.justPressed && blastTimes < body.maxJumpCount)
                {
                    blastTimes++;

                    radius = MSUtil.InverseHyperbolicScaling(baseRadius, stackRadius, maxRadius, stack);

                    EffectData bootsEffectData = new EffectData()
                    {
                        color = new Color32(0, 255, 0, 255),
                        scale = radius / 2,
                        origin = body.footPosition
                    };

                    EffectManager.SpawnEffect(JetBootsBlast, bootsEffectData, false);
                    Blast(radius);
                }
            }

            private void Blast(float radius)
            {
                var attacker = body.gameObject;
                var damage = body.damage * (baseDamage + (stackDamage * stack));



                new BlastAttack
                {
                    attacker = attacker,
                    baseDamage = damage,
                    radius = radius,
                    crit = body.RollCrit(),
                    falloffModel = BlastAttack.FalloffModel.None,
                    procCoefficient = 0,
                    teamIndex = body.teamComponent.teamIndex,
                    position = attacker.transform.position,
                    damageType = DamageType.SlowOnHit
                }.Fire();
            }
            private void OnDestroy()
            {
                if (NetworkServer.active)
                {
                    body.characterMotor.onHitGroundServer -= Restock;
                }
                else
                {
                    body.characterMotor.onHitGroundAuthority -= Restock;
                }
            }
        }
    }
}
