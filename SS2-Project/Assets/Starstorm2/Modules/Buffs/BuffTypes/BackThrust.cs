using Moonstorm.Components;
using R2API;
using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class BackThrust : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffBackThruster", SS2Bundle.Equipments);

        public override void Initialize()
        {
            //TODO - replace with custom icon?
            //BuffDef.iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texMovespeedBuffIcon");
            BuffDef.buffColor = Color.yellow;
        }

        public sealed class BackThrustBehavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffBackThruster;

            private float stopwatch;
            private float thrust;
            private float watchInterval = 0.15f;
            private float moveAngle;
            private float lastAngle;
            private float accelCoeff = Equipments.BackThruster.speedCap / (Equipments.BackThruster.accel + 0.00001f);   //Fuck people who put 0 in configs
            private float cutoff = Equipments.BackThruster.maxAngle * Mathf.Deg2Rad;
            private void FixedUpdate()
            {
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch > watchInterval)
                {
                    stopwatch -= watchInterval;
                    moveAngle = Mathf.Atan2(body.characterMotor.velocity.x, body.characterMotor.velocity.z) + Mathf.PI;
                    if (body.notMovingStopwatch < 0.1f && CheckAngle())
                        thrust = Mathf.Min(thrust + (accelCoeff * watchInterval), Equipments.BackThruster.speedCap);
                    else
                        thrust = Mathf.Max(thrust - (accelCoeff * watchInterval * 3), 0f);
                    body.RecalculateStats();
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
                body.RecalculateStats();
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
