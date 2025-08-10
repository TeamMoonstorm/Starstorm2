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
    /// <summary>
    /// Implementation of nucleator's primary.
    /// <br></br>
    /// See <see cref="FireSludge"/>
    /// </summary>
    public class ChargeSludge : BaseNukeChargeState, SteppedSkillDef.IStepSetter
    {
        public static GameObject chargePrefab;
        public static string leftMuzzleChildName;
        public static string rightMuzzleChildName;

        //Hash them for performance reasons
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

            //Spawn the charge vfx
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

        //Exit to FireSludge, give it the chosen strings, transform and step index
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

        /// <summary>
        /// Nucleator's stepped skill goes Left -> Right -> Left -> Right -> left...
        /// <br></br>
        /// He doesnt use one single cannon for the sludge, he fires from both in an alternative pattern
        /// </summary>
        /// <param name="i"></param>
        public void SetStep(int i)
        {
            _step = i;
            _chosenMuzzle = i == 0 ? leftMuzzleChildName : rightMuzzleChildName;
            _chosenAnimationHash = i == 0 ? leftChargeHash : rightChargeHash;
        }
    }
}