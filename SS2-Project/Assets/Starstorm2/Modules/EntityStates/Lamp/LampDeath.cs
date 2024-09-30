using UnityEngine;
using RoR2;

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
        private bool hasDestroyed;
        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            PlayCrossfade("Body", "Death", "deathPlaybackRate", duration * 1.2f, 0.05f);
            muzzle = GetModelChildLocator().FindChild(muzzleString);
            //Util.PlaySound("FollowerVO", gameObject);
            hasPlayedEffect = false;
            if (characterMotor)
                characterMotor.enabled = false;
            //if (modelLocator && initialEffect)
                //EffectManager.
            isBlue = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken == "SS2_SKIN_LAMP_BLUE";
            var effect = isBlue ? deathVFXblue : deathVFX;
            EffectManager.SimpleEffect(effect, muzzle.position, muzzle.rotation, false);
        }

        public void DestroyLamp()
        {
            if (hasDestroyed) return;

            hasPlayedEffect = true;
            hasDestroyed = true;
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

        public override void OnExit()
        {
            DestroyLamp();
            base.OnExit();          
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration && isAuthority)
            {
                if (!hasPlayedEffect)
                {
                    DestroyLamp();
                }
            }

            if (animator)
            {
                if (animator.GetFloat(mecanimPerameter) > 0.5f && !hasPlayedEffect)
                {
                    DestroyLamp();
                }
            }
        }
    }
}
