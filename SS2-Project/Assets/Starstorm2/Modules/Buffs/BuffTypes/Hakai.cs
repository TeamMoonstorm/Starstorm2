using Moonstorm.Components;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class Hakai : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdHakai", SS2Bundle.Equipments);

        public sealed class Behavior : BaseBuffBodyBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdHakai;
            public static float baseTimerDur = 5f;
            private float timer;
            private float timerDur;
            private bool expired = false;

            public void Start()
            {
                if (body.hullClassification == HullClassification.BeetleQueen)
                    timerDur = baseTimerDur * 2f;
                else if (body.hullClassification == HullClassification.Golem)
                    timerDur = baseTimerDur * 1.5f;
                else
                    timerDur = baseTimerDur;

                timer = 0;
            }

            public void FixedUpdate()
            {
                timer += Time.fixedDeltaTime;
                if (timer >= timerDur && !expired)
                {
                    Debug.Log("KILL");
                    body.healthComponent.Suicide();
                    expired = true;
                }
            }
        }
    }
}
