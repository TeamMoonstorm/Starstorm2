using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2.Projectile;
using RoR2;

namespace Moonstorm.Starstorm2.Components
{
    public class ZapperComponent : MonoBehaviour
    {
        public string chargeSound = "Play_railgunner_shift_chargeUp";
        public float chargeTime = 1.25f;
        public bool charging;
        private float chargeStopwatch;
        void Start()
        {

        }
        public void StartCharge()
        {
            this.charging = true;
            this.chargeStopwatch = this.chargeTime;
        }

        private void FixedUpdate()
        {
            this.chargeStopwatch -= Time.fixedDeltaTime;

            if (this.charging && this.chargeStopwatch <= 0)
            {

            }
        }

        public void FireOrb()
        {

        }
    }
}

