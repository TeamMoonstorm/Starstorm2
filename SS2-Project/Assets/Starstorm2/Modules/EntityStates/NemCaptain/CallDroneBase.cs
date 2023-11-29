using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RoR2;
using RoR2.Projectile;
using UnityEngine.Networking;
using Moonstorm.Starstorm2.Components;

namespace EntityStates.NemCaptain.Weapon
{
    public class CallDroneBase : BaseSkillState
    {
        //[SerializeField]
        //public GameObject muzzleFlashEffect;

        [SerializeField]
        public GameObject dronePrefab;

        //public static string muzzleString;
        public static float baseDuration;
        public SetupDroneOrders.PlacementInfo placementInfo;

        private NemCaptainController ncc;


        private float duration
        {
            get
            {
                return baseDuration / attackSpeedStat;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();

            Debug.Log("base call drone start");
 
            ncc = characterBody.GetComponent<NemCaptainController>();
            activatorSkillSlot.UnsetSkillOverride(gameObject, activatorSkillSlot.skillDef, GenericSkill.SkillOverridePriority.Replacement);
            activatorSkillSlot.SetSkillOverride(gameObject, ncc.nullSkill, GenericSkill.SkillOverridePriority.Loadout);
            if (isAuthority)
            {
                placementInfo = SetupDroneOrders.GetPlacementInfo(GetAimRay(), gameObject);
                if (placementInfo.ok)
                {
                    Debug.Log("uhhh i hope this is right.. some other info: ");
                    Debug.Log("activatorSkillSlot name : " + activatorSkillSlot.name);
                }
            }
            if (placementInfo.ok)
            {
                //SimpleMuzzleFlash(muzzleFlashEffect, gameObject, muzzleString, false);
                characterBody.SetAimTimer(3f);
                //animation
                if (NetworkServer.active && dronePrefab != null)
                {
                    Debug.Log("deploying!");
                    GameObject droneObject = UnityEngine.Object.Instantiate<GameObject>(dronePrefab, placementInfo.position, placementInfo.rotation);
                    droneObject.GetComponent<TeamFilter>().teamIndex = teamComponent.teamIndex;
                    droneObject.GetComponent<GenericOwnership>().ownerObject = gameObject;

                    Deployable droneDeployable = gameObject.GetComponent<Deployable>();

                    if (droneDeployable && characterBody.master)
                    {
                        //characterBody.master.AddDeployable(component,)
                    }

                    NetworkServer.Spawn(gameObject);
                }
            }
            else
            {
                //empty animations? idk
                Debug.Log("placement info was NOT ok... >:(");
            }
            EntityStateMachine esm = EntityStateMachine.FindByCustomName(gameObject, "Skillswap");
            if (esm)
                esm.SetNextStateToMain();            
        }

        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("base call drone exit");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge > duration)
                outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }

        //it just works!
        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            placementInfo.Serialize(writer);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            placementInfo.Deserialize(reader);
        }
    }
}
