using RoR2;
using RoR2.ContentManagement;
using R2API;
using System;
using UnityEngine;
using UnityEngine.Networking;
using RiskOfOptions.OptionConfigs;
using MSU.Config;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MSU;
using static MSU.BaseBuffBehaviour;
using System.Collections.Generic;

namespace SS2.Interactables
{
    public sealed class EliteCamp : SS2Interactable
    {
        public override SS2AssetRequest<InteractableAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<InteractableAssetCollection>("acEliteCamp", SS2Bundle.Interactables);

        public override bool IsAvailable(ContentPack contentPack)
        {
            return SS2Config.enableBeta;
        }

        public static float powerupMovementSpeed = 0.3f;
        public static float powerupAttackSpeed = 0.25f;
        public static float powerupDamage = 0.2f;

        public static Color fireColor = new Color(255f / 255f, 19f / 255f, 3f / 255f);
        public static Color iceColor = new Color(117f / 255f, 255f / 255f, 241f / 255f);
        public static Color lightningColor = new Color(72f / 255f, 72f / 255f, 255f / 255f);
        public static Color earthColor = new Color(50f / 255f, 255f / 255f, 50f / 255f);

        public static DamageColorIndex fireDamageColor;
        public static DamageColorIndex iceDamageColor;
        public static DamageColorIndex lightningDamageColor;
        public static DamageColorIndex earthDamageColor;

        public static GameObject firePowerupEffect;
        public static GameObject icePowerupEffect;
        public static GameObject lightningPowerupEffect;
        public static GameObject earthPowerupEffect;
        public override void Initialize()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
            IL.RoR2.HealthComponent.HandleDamageDealt += AddDamageNumbers;

            fireDamageColor = R2API.ColorsAPI.RegisterDamageColor(fireColor);
            iceDamageColor = R2API.ColorsAPI.RegisterDamageColor(iceColor);
            lightningDamageColor = R2API.ColorsAPI.RegisterDamageColor(lightningColor);
            earthDamageColor = R2API.ColorsAPI.RegisterDamageColor(earthColor);

            firePowerupEffect = SS2Assets.LoadAsset<GameObject>("FirePowerupEffect", SS2Bundle.Interactables);
            icePowerupEffect = SS2Assets.LoadAsset<GameObject>("IcePowerupEffect", SS2Bundle.Interactables);
            lightningPowerupEffect = SS2Assets.LoadAsset<GameObject>("LightningPowerupEffect", SS2Bundle.Interactables);
            earthPowerupEffect = SS2Assets.LoadAsset<GameObject>("EarthPowerupEffect", SS2Bundle.Interactables);
        }

        #region Powerup Buff
        private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int buffCount = 0;
            if (sender.HasBuff(SS2Content.Buffs.bdPowerupFire)) buffCount++;
            if (sender.HasBuff(SS2Content.Buffs.bdPowerupIce)) buffCount++;
            if (sender.HasBuff(SS2Content.Buffs.bdPowerupLightning)) buffCount++;
            if (sender.HasBuff(SS2Content.Buffs.bdPowerupEarth)) buffCount++;

            if (buffCount > 0)
            {
                args.damageMultAdd += powerupDamage * buffCount;
                args.attackSpeedMultAdd += powerupAttackSpeed * buffCount;
                args.moveSpeedMultAdd += powerupMovementSpeed * buffCount;
            }
        }



        private void AddDamageNumbers(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            // teamComponent = damageDealtMessage.attacker.GetComponent<TeamComponent>();
            int damageDealtMessageVarIndex = -1;
            int teamComponentVarIndex = -1;
            bool b = c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(out damageDealtMessageVarIndex),
                x => x.MatchLdfld<DamageDealtMessage>(nameof(DamageDealtMessage.attacker)),
                x => x.MatchCallvirt<GameObject>(nameof(GameObject.GetComponent)),
                x => x.MatchStloc(out teamComponentVarIndex));
            if (b)
            {
                c.Emit(OpCodes.Ldloc, teamComponentVarIndex);
                c.Emit(OpCodes.Ldloc, damageDealtMessageVarIndex);
                c.EmitDelegate<Action<TeamComponent, DamageDealtMessage>>((t, d) =>
                {
                    float totalDamageFromPowerups = 0;

                    if (d.damageType.IsDamageSourceSkillBased && t && t.body)
                    {
                        float bodyDamage = t.body.damage;
                        float damageWithoutPowerup = d.damage / (1 + EliteCamp.powerupDamage);
                        float powerupDamage = d.damage - damageWithoutPowerup;

                        bool hasFire = t.body.HasBuff(SS2Content.Buffs.bdPowerupFire);
                        bool hasIce = t.body.HasBuff(SS2Content.Buffs.bdPowerupIce);
                        bool hasLightning = t.body.HasBuff(SS2Content.Buffs.bdPowerupLightning);
                        bool hasEarth = t.body.HasBuff(SS2Content.Buffs.bdPowerupEarth);

                        if (hasFire)
                        {
                            totalDamageFromPowerups += powerupDamage;
                            DamageNumberManager.instance.SpawnDamageNumber(powerupDamage, d.position + Vector3.up * 0.5f, d.crit, t.teamIndex, fireDamageColor);
                        }
                        if (hasIce)
                        {
                            totalDamageFromPowerups += powerupDamage;
                            DamageNumberManager.instance.SpawnDamageNumber(powerupDamage, d.position + Vector3.up * 0.6f, d.crit, t.teamIndex, iceDamageColor);
                        }
                        if (hasLightning)
                        {
                            totalDamageFromPowerups += powerupDamage;
                            DamageNumberManager.instance.SpawnDamageNumber(powerupDamage, d.position + Vector3.up * 0.7f, d.crit, t.teamIndex, lightningDamageColor);
                        }
                        if (hasEarth)
                        {
                            totalDamageFromPowerups += powerupDamage;
                            DamageNumberManager.instance.SpawnDamageNumber(powerupDamage, d.position + Vector3.up * 0.8f, d.crit, t.teamIndex, earthDamageColor);
                        }
                    }
                    d.damage -= totalDamageFromPowerups;
                });
            }
            else
            {
                SS2Log.Fatal("EliteCamp.AddDamageNumbers: ILHook failed.");
            }
        }
        #endregion

        #region Powerup VFX
        private static int maxBalls = 6;
        private static float maxBuffDuration = 30f;
        public class PowerupEffectBehavior : BaseBuffBehaviour
        {
            public virtual BuffDef buffDef => null;
            public virtual GameObject effectPrefab => null;

            private GameObject effectInstance;
            private List<Transform> balls = new List<Transform>();
            private int ballCount;
            private GameObject ball;
            private void OnEnable()
            {
                if (effectPrefab)
                {
                    effectInstance = GameObject.Instantiate(effectPrefab, characterBody.coreTransform.position, Quaternion.identity);
                    ball = effectInstance.transform.Find("BallPrefab")?.gameObject;
                }
                
                ////effectInstance.transform.localScale = Vector3.one * characterBody.radius;
            }
            private void OnDisable()
            {
                Destroy(effectInstance);
                balls = new List<Transform>();
                ballCount = 0;
            }
            private void FixedUpdate()
            {
                UpdateBallCount();

                if (!base.characterBody.healthComponent.alive)
                    Destroy(this.effectInstance);
            }

            private void Update()
            {
                if (effectInstance)
                {
                    Vector3 a = characterBody.corePosition;
                    effectInstance.transform.position = a;
                }
            }

            private void SetNewBallCount(int stack)
            {
                //float rotation = 360f / stack; // evenly spaced balls always
                float rotation = 360f / maxBalls; // balls leave a gap when disappearing

                for (int i = balls.Count; i < stack; i++) // if stack > balls
                {
                    if (effectInstance)
                    {
                        balls.Add(GameObject.Instantiate(ball, effectInstance.transform).transform);
                    }
                }

                for (int i = balls.Count - 1; i > stack; i--) // if balls > stack
                {
                    if (balls[i] != null)
                    {
                        GameObject.Destroy(balls[i].gameObject);
                        balls.RemoveAt(i);
                    }
                }

                for (int i = 0; i < balls.Count; i++)
                {
                    if (balls[i] != null)
                    {
                        balls[i].rotation = Quaternion.Euler(0, rotation * i, 0);
                    }
                }
            }

            private void UpdateBallCount()
            {
                if (this == null || buffCount == 0) return;

                int newBallCount = 0;

                characterBody.TryGetTimedBuffHighestTotalDuration(buffDef.buffIndex, out float timeRemaining);
                float fractionRemaining = timeRemaining / maxBuffDuration;
                newBallCount = Mathf.CeilToInt(fractionRemaining * maxBalls);

                if (newBallCount != ballCount)
                {
                    SetNewBallCount(newBallCount);
                    ballCount = newBallCount;
                }
            }
        }

        public class FirePowerup : PowerupEffectBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdPowerupFire;
            public override GameObject effectPrefab => firePowerupEffect;
            public override BuffDef buffDef => SS2Content.Buffs.bdPowerupFire;
        }
        public class IcePowerup : PowerupEffectBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdPowerupIce;
            public override GameObject effectPrefab => icePowerupEffect;
            public override BuffDef buffDef => SS2Content.Buffs.bdPowerupIce;
        }
        public class LightningPowerup : PowerupEffectBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdPowerupLightning;
            public override GameObject effectPrefab => lightningPowerupEffect;
            public override BuffDef buffDef => SS2Content.Buffs.bdPowerupLightning;
        }
        public class EarthPowerup : PowerupEffectBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdPowerupEarth;
            public override GameObject effectPrefab => earthPowerupEffect;
            public override BuffDef buffDef => SS2Content.Buffs.bdPowerupEarth;
        }
        #endregion

        
    }
}
