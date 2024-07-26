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
    public class ChargeSludge : BaseNukeWeaponChargeState, SteppedSkillDef.IStepSetter
    {
        public static GameObject chargePrefab;
        public static string leftMuzzleChildName;
        public static string rightMuzzleChildName;

        private string chosenMuzzle;
        private Transform muzzleTransform;
        private GameObject chargePrefabInstance;
        public override void OnEnter()
        {
            base.OnEnter();

            ChildLocator modelChildLocator = GetModelChildLocator();
            if(modelChildLocator)
            {
                muzzleTransform = modelChildLocator.FindChild(chosenMuzzle);
            }

            if(muzzleTransform && chargePrefab)
            {
                chargePrefabInstance = GameObject.Instantiate(chargePrefab, muzzleTransform);
                chargePrefabInstance.transform.SetPositionAndRotation(muzzleTransform.position, muzzleTransform.rotation);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if(chargePrefabInstance)
            {
                Destroy(chargePrefabInstance);
            }
        }

        protected override BaseNukeWeaponFireState GetFireState()
        {
            var fireSludge = new FireSludge();
            fireSludge.chosenMuzzleString = chosenMuzzle;
            fireSludge.muzzleTransform = muzzleTransform;
            return fireSludge;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public void SetStep(int i)
        {
            chosenMuzzle = i == 0 ? leftMuzzleChildName : rightMuzzleChildName;
        }
    }
}