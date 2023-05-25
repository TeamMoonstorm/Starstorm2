using RoR2;
using UnityEngine;

namespace EntityStates.NemHuntress
{
    public class ChargeArrow : BaseSkillState
    {
        public static float baseChargeDuration = 1.75f;
        public static float maxEmission;
        public static float minEmission;
        public static GameObject lightningEffect;
        public static NetworkSoundEventDef sound;

        //private Material swordMat;
        private float chargeDuration;
        //private ChildLocator childLocator;
        //private Animator animator;
        //private Transform modelBaseTransform;
        //private GameObject effectInstance;
        //private GameObject defaultCrosshair;
        //private uint chargePlayID;

        private bool hasPlayedSound = false;

        public override void OnEnter()
        {
            base.OnEnter();
            chargeDuration = baseChargeDuration / attackSpeedStat;
            //childLocator = GetModelChildLocator();
            ////modelBaseTransform = GetModelBaseTransform();
            //animator = GetModelAnimator();
            //defaultCrosshair = characterBody.defaultCrosshairPrefab;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float charge = CalcCharge();

            if (charge > 1f)
            {
                charge = 1f;
                if (sound && !hasPlayedSound)
                {
                    Debug.Log("attempting to play full charge sound");
                    hasPlayedSound = true;
                    EffectManager.SimpleSoundEffect(sound.index, transform.position, true);
                }
            }

            characterBody.SetSpreadBloom(Util.Remap(charge, 0f, 1f, 0f, 1f), true);

            StartAimMode();

            if (isAuthority && !IsKeyDownAuthority() && fixedAge >= 0.1f)
            {
                FireArrow nextState = new FireArrow();
                nextState.charge = charge;
                outer.SetNextState(nextState);
            }
        }

        protected float CalcCharge()
        {
            return Mathf.Clamp01(fixedAge / chargeDuration);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}