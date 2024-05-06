using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS2.Equipments
{
    public sealed class BackThruster : SS2Equipment, IContentPackModifier
    {
        public override SS2AssetRequest<EquipmentAssetCollection> AssetRequest<EquipmentAssetCollection>()
        {
            return SS2Assets.LoadAssetAsync<EquipmentAssetCollection>("acBackThruster", SS2Bundle.Equipments);
        }

        private const string token = "SS2_EQUIP_BACKTHRUSTER_DESC";

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "How long the Thruster buff lasts, in seconds.")]
        [FormatToken(token)]
        public static float thrustDuration = 8f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Maximum speed bonus from Thruster (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float speedCap = 2f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "How long it takes to reach maximum speed, in seconds")]
        public static float accel = 1.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Maximum turning angle before losing built up speed")]
        public static float maxAngle = 15f;

        private BuffDef _buffDef;

        public override bool Execute(EquipmentSlot slot)
        {
            var characterMotor = slot.characterBody.characterMotor;
            if (characterMotor)
            {
                slot.characterBody.AddTimedBuff(SS2Content.Buffs.BuffBackThruster, thrustDuration);
                return true;
            }
            return false;
        }

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        public sealed class BackThrusterBuffBehaviour : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffBackThruster;

            private float stopwatch;
            private float thrust;
            private float watchInterval = 0.15f;
            private float moveAngle;
            private float lastAngle;
            private float accelCoeff = Equipments.BackThruster.speedCap / (Equipments.BackThruster.accel + 0.00001f);   //Fuck people who put 0 in configs
            private float cutoff =  maxAngle * Mathf.Deg2Rad;
            private void FixedUpdate()
            {
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch > watchInterval)
                {
                    stopwatch -= watchInterval;
                    moveAngle = Mathf.Atan2(CharacterBody.characterMotor.velocity.x, CharacterBody.characterMotor.velocity.z) + Mathf.PI;
                    if (CharacterBody.notMovingStopwatch < 0.1f && CheckAngle())
                        thrust = Mathf.Min(thrust + (accelCoeff * watchInterval), Equipments.BackThruster.speedCap);
                    else
                        thrust = Mathf.Max(thrust - (accelCoeff * watchInterval * 3), 0f);
                    CharacterBody.RecalculateStats();
                    lastAngle = moveAngle;
                }
            }
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.moveSpeedMultAdd += thrust;
            }
            public void OnDestroy()
            {
                thrust = 0f;
                lastAngle = 0f;
                CharacterBody.RecalculateStats();
            }
            private bool CheckAngle()
            {
                float delta = Mathf.Abs(moveAngle - lastAngle);
                if (delta <= cutoff || delta > (2 * Mathf.PI - cutoff))
                    return true;
                return false;
            }
        }
    }
}