using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using UnityEngine;
using RoR2.CameraModes;
using System.Reflection;
using static RoR2.ExplicitPickupDropTable;
namespace SS2.Monsters
{
    public sealed class LampBoss : SS2Monster
    {
        public override SS2AssetRequest<MonsterAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<MonsterAssetCollection>("acLampBoss", SS2Bundle.Monsters);

        public static BodyIndex BodyIndex;
        public static GameObject lampBuffEffectPrefab;
        public override void Initialize()
        {

            lampBuffEffectPrefab = SS2Assets.LoadAsset<GameObject>("LampBuffEffect", SS2Bundle.Monsters);
            RoR2Application.onLoad += () => BodyIndex = BodyCatalog.FindBodyIndex("LampBossBody");
            R2API.RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
            On.RoR2.CameraModes.CameraModeBase.ApplyLookInput += CameraModeBase_ApplyLookInput;
        }

        public static float pullSpeed = 1000f; // idkkkkkkkkkkkkkkkkkkkkk
        public static AnimationCurve pullCurve = AnimationCurve.EaseInOut(0, 1, 1, 1); //idk
        public static float distanceSqrForMaxPull = 0f; //idk
        private static bool log = false;

        private void CameraModeBase_ApplyLookInput(On.RoR2.CameraModes.CameraModeBase.orig_ApplyLookInput orig, CameraModeBase self, ref CameraModeBase.CameraModeContext context, ref CameraModeBase.ApplyLookInputArgs args)
        {
            if (context.targetInfo.body)
            {
                var lamp = LampCameraPullAttachment.GetAttachmentForBody(context.targetInfo.body.gameObject);
                if (lamp && lamp.target)
                {
                    Vector2 currentInput = args.lookInput;
                    // Get position of lamp on screen
                    Vector2 center = new Vector2(0.5f, 0.5f);
                    Vector3 screenPoint = context.cameraInfo.sceneCam.WorldToViewportPoint(lamp.target.transform.position);
                    Vector2 between = new Vector2(screenPoint.x, screenPoint.y) - center;

                    float distanceThing = Mathf.Clamp01(between.sqrMagnitude / distanceSqrForMaxPull);
                    float mult = pullCurve.Evaluate(distanceThing);
                    // if target is behind the camera, rotate in opposite direction
                    // pretty sure this is a bandaid fix. im stupid
                    if (screenPoint.z < 0) 
                    {
                        mult *= -1f;
                    }

                    args.lookInput += between * pullSpeed * mult * Time.deltaTime;

                    if (log)
                    {
                        SS2Log.Debug($"current = {currentInput} || screenPoint = {screenPoint} || between = {between} || distanceThing = {distanceThing} || mult = {mult} || result = {args.lookInput}");
                    }
                }
            }
            
            orig(self, ref context, ref args);
        }

        private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int buffCount = sender.GetBuffCount(SS2Content.Buffs.bdLampBuff);
            if (buffCount > 0 && sender.bodyIndex != BodyIndex)
            {
                args.cooldownMultAdd += 0.33f;
                args.attackSpeedMultAdd += 0.5f;
                args.moveSpeedMultAdd += 0.5f;
            }
        }
        // temporaryvisualeffects are hard coded :(
        // literally just holds the instance
        private static string effectChildString = "Head";
        public sealed class LampBuffBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation()]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdLampBuff;

            private TemporaryVisualEffect instance;
            private void FixedUpdate()
            {
                if (characterBody && lampBuffEffectPrefab)
                {
                    characterBody.UpdateSingleTemporaryVisualEffect(ref instance, lampBuffEffectPrefab, characterBody.radius, characterBody.HasBuff(SS2Content.Buffs.bdLampBuff), effectChildString);
                }
            }
            private void OnEnable()
            {
                if (characterBody && lampBuffEffectPrefab)
                {
                    characterBody.UpdateSingleTemporaryVisualEffect(ref instance, lampBuffEffectPrefab, characterBody.radius, characterBody.HasBuff(SS2Content.Buffs.bdLampBuff), effectChildString);
                }
            }
            private void OnDisable()
            {
                if (characterBody && lampBuffEffectPrefab)
                {
                    characterBody.UpdateSingleTemporaryVisualEffect(ref instance, lampBuffEffectPrefab, characterBody.radius, characterBody.HasBuff(SS2Content.Buffs.bdLampBuff), effectChildString);
                }
            }

        }

        [Serializable]
        public class MoonPhaseData
        {
            public PhaseData[] phasedata;
        }

        [Serializable]
        public class PhaseData
        {
            public string phase;
        }
    }
}
