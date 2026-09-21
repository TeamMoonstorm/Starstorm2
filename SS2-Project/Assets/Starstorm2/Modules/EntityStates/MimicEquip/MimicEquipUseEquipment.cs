using RoR2;
using RoR2.Hologram;
using SS2;
using SS2.Components;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace EntityStates.MimicEquip
{
    public class MimicEquipUseEquipment : BaseState
    {
        public static float baseDuration;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
