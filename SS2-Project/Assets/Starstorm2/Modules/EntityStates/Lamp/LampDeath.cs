using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using System;
using UnityEngine.Networking;

namespace EntityStates.Lamp
{
    public class LampDeath : GenericCharacterDeath
    {
        public static GameObject deathVFX;
        public static GameObject deathVFXblue;
        //public static GameObject initialVFX;
        //public static GameObject initialVFXblue;
        public static float duration;
        public static string mecanimPerameter;
        private float timer;
        private Animator animator;
        private bool hasPlayedEffect;
        public static string muzzleString;
        private Transform muzzle;

        private bool isBlue;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            PlayCrossfade("Body", "Death", 0.01f);
            muzzle = GetModelChildLocator().FindChild(muzzleString);
            //Util.PlaySound("FollowerVO", gameObject);
            hasPlayedEffect = false;
            if (characterMotor)
                characterMotor.enabled = false;
            //if (modelLocator && initialEffect)
                //EffectManager.
            isBlue = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken == "SS2_SKIN_LAMP_BLUE";
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (animator)
            {
                if (animator.GetFloat(mecanimPerameter) > 0.5f && !hasPlayedEffect)
                {
                    hasPlayedEffect = true;
                    var effect = isBlue ? deathVFXblue : deathVFX;
                    EffectManager.SimpleEffect(effect, muzzle.position, muzzle.rotation, true);
                    Util.PlaySound("LampImpact", gameObject);

                    //YOU SHOULD KILL YOURSELF NOW
                    //NetworkServer.Destroy(gameObject);

                    if (cachedModelTransform)
                    {
                        Destroy(cachedModelTransform.gameObject);
                        cachedModelTransform = null;
                    }

                    DestroyBodyAsapServer();
                    DestroyModel();
                    Destroy(gameObject);
                }
            }
        }
    }
}
