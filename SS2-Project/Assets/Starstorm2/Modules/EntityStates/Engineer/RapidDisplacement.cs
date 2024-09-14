using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.EngiTurret.EngiTurretWeapon;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using SS2.Survivors;
using R2API;
using System;

namespace EntityStates.Engi
{
    public class RapidDisplacement : BaseSkillState
    {
        [SerializeField]
        public static float baseDuration = 1;
        [SerializeField]
        public static float speedMultiplier = 2;
        [SerializeField]
        public static float hitRadius = 5f;
        //[FormatToken("SS2_EXECUTIONER_DASH_DESCRIPTION", 0)]

        [SerializeField]
        public static float damageCoeff = .75f;

        [SerializeField]
        public static float procCoeff = 1;

        [SerializeField]
        public static float maxDistance = 25f;


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
        private OverlapAttack attack;
        private List<HurtBox> victimsStruck = new List<HurtBox>();
        int count = 1;
        bool fromDash;
        public override void OnEnter()
        {
            base.OnEnter();
            //this.duration = this.baseDuration / base.attackSpeedStat;
            Ray aimRay = base.GetAimRay();
            duration = baseDuration;
            characterBody.isSprinting = true;
            //this.PlayAnimation("Body", "Sprinting", "walkSpeed", duration);

            Util.PlaySound("Play_engi_R_walkingTurret_laser_start", base.gameObject);
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
            characterDirection.turnSpeed = 300; 

            HopIfAirborne();
            if (!characterMotor.isGrounded)
            {
                var hop = new HopDisplacement { fromDash = false };
                outer.SetNextState(hop);
                fromDash = false;
            }
            else
            {
                base.StartAimMode(aimRay, 1.5f, false);
                HitBoxGroup hitBoxGroup = null;
                if (modelTransform)
                {
                    var multi = modelTransform.GetComponentsInChildren<HitBoxGroup>();
                    hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "HitboxGround");
                }
                attack = new OverlapAttack();
                attack.attacker = base.gameObject;
                attack.inflictor = base.gameObject;
                attack.teamIndex = base.GetTeam();
                attack.damage = 2 * this.damageStat;
                //attack.hitEffectPrefab = ToolbotDash.impactEffectPrefab;
                attack.forceVector = (characterDirection.forward + (Vector3.up/1.125f)) * 1000;
                attack.pushAwayForce = 500;
                attack.hitBoxGroup = hitBoxGroup;
                attack.isCrit = base.RollCrit();
                attack.damageType = DamageType.Stun1s;
                attack.damageColorIndex = DamageColorIndex.Default;
                attack.procChainMask = default(ProcChainMask);
                attack.procCoefficient = 1;
                fromDash = true;
            }
            count = 1;

            //this.PlayAnimation("Body", "Sprinting", "walkSpeed", duration);
        }
        
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterBody.isSprinting = true;
            //Debug.Log("yeah " + fixedAge);
            if (characterMotor.isGrounded && fromDash)
            {
                if (characterDirection && characterMotor)
                    characterMotor.rootMotion += characterDirection.forward * characterBody.moveSpeed * speedMultiplier * Time.fixedDeltaTime;
                if (attack != null)
                {
                    attack.Fire(victimsStruck);
                    if (fixedAge >= (duration/4) * count)
                    {
                        attack.ignoredHealthComponentList = new List<HealthComponent>();
                        ++count;
                    }
                }
            }
            else if(fromDash)
            {
                var hop = new HopDisplacement { fromDash = true };
                outer.SetNextState(hop);
            }



            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();

            }

        }

        public override void OnExit()
        {
            base.OnExit();
            Util.PlaySound("Play_engi_R_walkingTurret_laser_end", base.gameObject);
            //this.PlayAnimation("Gesture, Additive", "Empty");
            //
            //if (leftLaserInstance)
            //    EntityState.Destroy(leftLaserInstance);
            //
            //if (rightLaserInstance)
            //    EntityState.Destroy(rightLaserInstance);
            characterDirection.turnSpeed = 720f;
        }
        private void HopIfAirborne()
        {
            if (!characterMotor.isGrounded)
            {
                SmallHop(characterMotor, 10);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
