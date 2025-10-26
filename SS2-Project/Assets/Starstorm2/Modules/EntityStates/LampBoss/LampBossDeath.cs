using UnityEngine;
using RoR2;
using UnityEngine.Networking;

namespace EntityStates.LampBoss
{
    public class LampBossDeath : GenericCharacterDeath
    {
        public static GameObject deathVFX;
        public static GameObject deathVFXblue;
        public static GameObject initialVFX;
        public static GameObject initialVFXblue;
        public static float duration;
        public static string mecanimPerameter;
        private float timer;
        private Animator animator;
        private bool hasPlayedEffect;
        public static string muzzleString;
        private GameObject particles;
        private Transform muzzle;

        private bool isBlue;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            PlayCrossfade("FullBody, Override", "BufferEmpty", 0.01f);
            muzzle = GetModelChildLocator().FindChild(muzzleString);
            Util.PlaySound("WayfarerVO", gameObject);
            hasPlayedEffect = false;
            if (characterMotor)
                characterMotor.enabled = false;

            isBlue = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken == "SS2_SKIN_LAMP_BLUE";

            hasPlayedEffect = true;
            var effect = isBlue ? deathVFXblue : deathVFX;
            if (NetworkServer.active)
                EffectManager.SimpleEffect(effect, muzzle.position, muzzle.rotation, true);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if ((animator.GetFloat(mecanimPerameter) > 0.5f) || (fixedAge > 2.7f) && !hasPlayedEffect && NetworkServer.active)
            {
                hasPlayedEffect = true;
                var effect = isBlue ? deathVFXblue : deathVFX;
                Util.PlaySound("WayfarerDeath", gameObject);

                if (NetworkServer.active)
                    EffectManager.SimpleEffect(effect, muzzle.position, muzzle.rotation, true);

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

        public override void OnExit()
        {
            DestroyModel();
            base.OnExit();
        }
    }
}
