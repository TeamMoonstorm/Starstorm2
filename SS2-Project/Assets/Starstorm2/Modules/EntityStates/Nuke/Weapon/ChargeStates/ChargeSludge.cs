using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.Nuke.Weapon
{
    public class ChargeSludge : BaseNukeChargeState, SteppedSkillDef.IStepSetter
    {
        public static GameObject chargePrefab;
        public static string leftMuzzleChildName;
        public static string rightMuzzleChildName;

        private static int leftChargeHash = Animator.StringToHash("IrradiateChargeL");
        private static int rightChargeHash = Animator.StringToHash("IrradiateChargeR");
        private static int irradiatePlaybackHash = Animator.StringToHash("irradiate.playbackRate");

        private string _chosenMuzzle;
        private int _chosenAnimationHash;
        private int _step;
        private Transform muzzleTransform;
        private GameObject chargePrefabInstance;
        public override void OnEnter()
        {
            base.OnEnter();

            ChildLocator modelChildLocator = GetModelChildLocator();
            if (modelChildLocator)
            {
                muzzleTransform = modelChildLocator.FindChild(_chosenMuzzle);
            }

            if (muzzleTransform && chargePrefab)
            {
                chargePrefabInstance = GameObject.Instantiate(chargePrefab, muzzleTransform);
                chargePrefabInstance.transform.SetPositionAndRotation(muzzleTransform.position, muzzleTransform.rotation);
            }

            StartAimMode();
            PlayAnimation("UpperBody, Override", _chosenAnimationHash, irradiatePlaybackHash, attackSpeedStat);
        }

        public override void OnExit()
        {
            base.OnExit();
            if (chargePrefabInstance)
            {
                Destroy(chargePrefabInstance);
            }
        }

        protected override SS2.Survivors.Nuke.IChargedState GetFireState()
        {
            var fireSludge = new FireSludge();
            fireSludge.chosenMuzzleString = _chosenMuzzle;
            fireSludge.muzzleTransform = muzzleTransform;
            fireSludge.stepIndex = _step;
            return fireSludge;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public void SetStep(int i)
        {
            _step = i;
            _chosenMuzzle = i == 0 ? leftMuzzleChildName : rightMuzzleChildName;
            _chosenAnimationHash = i == 0 ? leftChargeHash : rightChargeHash;
        }
    }
}