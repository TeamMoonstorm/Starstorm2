using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using System;

namespace EntityStates.LampBoss
{
    public class LampBossDeath : GenericCharacterDeath
    {
        public static GameObject deathVFX;
        public static GameObject initialVFX;
        public static float duration;
        public static string mecanimPerameter;
        private float timer;
        private Animator animator;
        private bool hasPlayedEffect;
        public static string muzzleString;
        private Transform muzzle;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            PlayCrossfade("FullBody, Override", "BufferEmpty", 0.01f);
            muzzle = GetModelChildLocator().FindChild(muzzleString);
            hasPlayedEffect = false;
            if (characterMotor)
                characterMotor.enabled = false;
            //if (modelLocator && initialEffect)
                //EffectManager.
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (animator)
            {
                if (animator.GetFloat(mecanimPerameter) > 0.5f && !hasPlayedEffect)
                {
                    hasPlayedEffect = true;
                    EffectManager.SimpleEffect(deathVFX, muzzle.position, muzzle.rotation, true);
                    DestroyBodyAsapServer();
                    DestroyModel();
                    Destroy(gameObject);
                }
            }
        }
    }
}
