using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.EngiTurret.EngiTurretWeapon;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using SS2.Survivors;
using R2API;

namespace EntityStates.Engi
{
    public class StartupDisplacement : BaseSkillState
    {
        [SerializeField]
        public static float baseDuration = .15f;
        //[FormatToken("SS2_EXECUTIONER_DASH_DESCRIPTION", 0)]
        [SerializeField]
        public static float hopVelocity = 10f;


        private float duration;
        //public float baseDuration = 1f;
        //private float duration;
        //private float fireTimer;
        private Transform modelTransform;
        //private float counter = 0;
        //MuzzleLeft
        //MuzzleRight

        //public GameObject hitEffectPrefab = FireBarrage.hitEffectPrefab;
        //public GameObject tracerEffectPrefab = FireBarrage.tracerEffectPrefab;

        //public GameObject hitsparkPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/Hitspark1.prefab").WaitForCompletion();
        //public GameObject hitsparkPrefab2 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/HitsparkRailgunnerPistol.prefab").WaitForCompletion();
        //public GameObject laserPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/LaserEngiTurret.prefab").WaitForCompletion();
        //
        //private GameObject leftLaserInstance;
        //private Transform leftLaserInstanceEnd;
        //
        //private GameObject rightLaserInstance;
        //private Transform rightLaserInstanceEnd;

        private Transform muzzleLeft;
        private Transform muzzleRight;


        public override void OnEnter()
        {
            base.OnEnter();
            //this.duration = this.baseDuration / base.attackSpeedStat;
            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 2f, false);

            this.PlayAnimation("Body", "SprintEnter");

            //Util.PlaySound("Play_engi_R_walkingTurret_laser_start", base.gameObject);
            //fireTimer = 0;
            //counter = 0;
            modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                ChildLocator component = this.modelTransform.GetComponent<ChildLocator>();
                if (component)
                {
                    muzzleLeft = component.FindChild("MuzzleLeft");
                    muzzleRight = component.FindChild("MuzzleRight");
                }
            }

            duration = baseDuration;
            //characterDirection.turnSpeed = 240f; 
            Debug.Log("begin " + fixedAge + " | " + duration);
            if(duration < .15f)
            {
                duration = .15f;
            }
            HopIfAirborne();
        }
        
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            //characterBody.isSprinting = true;
            Debug.Log("yeah " + fixedAge);

            //this.PlayAnimation("Body", "Sprinting");

            //if (characterDirection && characterMotor)
            //    characterMotor.rootMotion += characterDirection.forward * characterBody.moveSpeed * speedMultiplier * Time.fixedDeltaTime;

            if (fixedAge >= duration)
            {
                var rapid = new RapidDisplacement();
                outer.SetNextState(rapid);

                Debug.Log("goodbye " + fixedAge + " | " + duration);
            }

        }

        public override void OnExit()
        {
            base.OnExit();
            //Util.PlaySound("Play_engi_R_walkingTurret_laser_end", base.gameObject);
            //this.PlayAnimation("Gesture, Additive", "Empty");
            //
            //if (leftLaserInstance)
            //    EntityState.Destroy(leftLaserInstance);
            //
            //if (rightLaserInstance)
            //    EntityState.Destroy(rightLaserInstance);
            Debug.Log("i have been killed " + fixedAge + " | " + duration);
            //characterDirection.turnSpeed = 720f;
            //this.PlayAnimation("Body", "IdleIn");
        }

        private void HopIfAirborne()
        {
            if (!characterMotor.isGrounded)
            {
                SmallHop(characterMotor, hopVelocity);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
