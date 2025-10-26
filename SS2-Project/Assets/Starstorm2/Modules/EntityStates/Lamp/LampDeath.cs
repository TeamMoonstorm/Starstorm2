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
        private static float duration = 0.6f;
        private static string mecanimPerameter = "Death";
        
        private static string muzzleString = "Head";
        private static string deathSoundString = "FollowerDeath";
        private static string deathVoiceSoundString; // yeah we do our sound mixing at runtime

        private static float printTime = 1f;
        private static float printStartHeight = -25f;
        private static float printMaxHeight = -25f;
        private static float printStartBias = 0.9f;
        private static float printMaxBias = 3.33f;

        private Transform muzzle;
        private float timer;
        private Animator animator;
        private bool hasPlayedEffect;
        private bool isBlue;
        private bool hasDestroyed;
        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            PlayAnimation("Body", "Death");
            muzzle = GetModelChildLocator().FindChild(muzzleString);
            Util.PlaySound(deathSoundString, gameObject);
            Util.PlaySound(deathVoiceSoundString, gameObject);

            isBlue = SkinCatalog.FindCurrentSkinDefForBodyInstance(gameObject).skinIndex == SS2.Monsters.Lamp.HesBlue;
            var effect = isBlue ? deathVFXblue : deathVFX;
            EffectManager.SimpleEffect(effect, muzzle.position, muzzle.rotation, false);

            Transform modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                var printController = modelTransform.GetComponent<PrintController>();
                printController.enabled = false;
                printController.printTime = printTime;
                printController.startingPrintHeight = printStartHeight;
                printController.maxPrintHeight = printMaxHeight;
                printController.startingPrintBias = printStartBias;
                printController.maxPrintBias = printMaxBias;
                printController.enabled = true;
            }

            Transform glass = FindModelChild("GlassMesh");
            if (glass)
            {
                glass.gameObject.SetActive(false);
            }
        }

        public void DestroyLamp()
        {
            if (hasDestroyed) return;

            hasPlayedEffect = true;
            hasDestroyed = true;
            //var effect = isBlue ? deathVFXblue : deathVFX;
            //EffectManager.SimpleEffect(effect, muzzle.position, muzzle.rotation, true);
            //Util.PlaySound("LampImpact", gameObject);

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
