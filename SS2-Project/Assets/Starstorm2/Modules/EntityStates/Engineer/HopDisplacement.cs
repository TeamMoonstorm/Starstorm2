using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.EngiTurret.EngiTurretWeapon;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using SS2.Survivors;
using R2API;
using System.Collections;
using System;

namespace EntityStates.Engi
{
    public class HopDisplacement : BaseSkillState
    {
        [SerializeField]
        public static float baseDuration = .5f;
        //[FormatToken("SS2_EXECUTIONER_DASH_DESCRIPTION", 0)]
        [SerializeField]
        public static float hopVelocity = 10f;
        [SerializeField]
        public static AnimationCurve jumpCurve;

        private float duration;
        //public float baseDuration = 1f;
        //private float duration;
        //private float fireTimer;
        private Transform modelTransform;
        //private float counter = 0;
        //MuzzleLeft
        //MuzzleRight
        public bool fromDash;
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
        private List<HurtBox> victimsStruck = new List<HurtBox>();
        private OverlapAttack attack;
        public override void OnEnter()
        {
            base.OnEnter();
            //this.duration = this.baseDuration / base.attackSpeedStat;
            Ray aimRay = base.GetAimRay();
            //base.StartAimMode(aimRay, 2f, false);
            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.SprintAnyDirection;
            this.PlayAnimation("Body", "SprintEnter");
            characterBody.isSprinting = true;
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

            HitBoxGroup hitBoxGroup = null;
            if (modelTransform)
            {
                hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "HitboxHop");
                attack = new OverlapAttack();
                attack.attacker = base.gameObject;
                attack.inflictor = base.gameObject;
                attack.teamIndex = base.GetTeam();
                attack.damage = 2 * this.damageStat;
                //attack.hitEffectPrefab = ToolbotDash.impactEffectPrefab;
                attack.forceVector = characterDirection.forward * -1800;
                attack.pushAwayForce = 1000;
                attack.hitBoxGroup = hitBoxGroup;
                attack.isCrit = base.RollCrit();
                attack.damageType = DamageType.Stun1s;
            }

            if (!fromDash)
            {
                Vector3 velocity = characterMotor.velocity;
                velocity.y = characterBody.jumpPower * .75f;
                characterMotor.velocity = velocity;
            }

            EffectData effectDataL = new EffectData
            {
                origin = new Vector3(.5f, 1.25f, -.6f),
                rotation = Quaternion.Euler(aimRay.direction),
                //rotation = muzzleLeft.rotation, //Quaternion.Euler(new Vector3(muzzleLeft.rotation.x + 180, muzzleLeft.rotation.y, muzzleLeft.rotation.z)),
                scale = .5f,
                rootObject = characterBody.gameObject,
                modelChildIndex = 2, //i dunno test it in the morning
                genericFloat = -23,
                
                
            };
            effectDataL.SetNetworkedObjectReference(characterDirection.gameObject);
            EffectManager.SpawnEffect(Engineer.engiPrefabExplosion, effectDataL, transmit: true);

            EffectData effectDataR = new EffectData
            {
                origin = new Vector3(-.5f, 1.25f, -.6f),
                rotation = Quaternion.Euler(aimRay.direction),
                //rotation = muzzleRight.rotation,//Quaternion.Euler(new Vector3(muzzleRight.rotation.x + 180, muzzleRight.rotation.y, muzzleRight.rotation.z)),
                scale = .5f,
                rootObject = characterBody.gameObject,
                modelChildIndex = 2,
                genericFloat = -23

            };
            effectDataR.SetNetworkedObjectReference(characterDirection.gameObject);
            EffectManager.SpawnEffect(Engineer.engiPrefabExplosion, effectDataR, transmit: true);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            //characterBody.isSprinting = true;
            //Debug.Log("yeah " + fixedAge);
            characterBody.isSprinting = true;
            if (characterMotor && characterDirection)
            {
                var curve = jumpCurve.Evaluate(fixedAge / duration);
                var direction = characterDirection.forward;
                direction.y = fromDash ? .175f : .3f;
                direction /= 1.125f; //test this in the morning imn going to bed
                base.characterMotor.rootMotion += (fromDash ? 2.5f : 3.25f) * curve * (this.moveSpeedStat / 2) * Time.fixedDeltaTime * direction;
            }
            if(fixedAge < duration / 4)
            {
                attack.Fire();
            }

            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();
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
            //characterDirection.turnSpeed = 720f;
            //this.PlayAnimation("Body", "IdleIn");
            var token = base.characterBody.gameObject.AddComponent<EngiHopToken>();
            token.body = base.characterBody;
            token.motor = characterMotor;
            //base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.SprintAnyDirection;
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
            return InterruptPriority.Skill;
        }
    }

    public class EngiHopToken : MonoBehaviour
    {
        public CharacterBody body;
        public CharacterMotor motor;
        float timer = 0;

        private void FixedUpdate()
        {
            
            if (motor && motor.isGrounded)
            {
                body.bodyFlags &= ~CharacterBody.BodyFlags.SprintAnyDirection;
                Debug.Log("Byebye!");
                Destroy(this);
            }

            timer += Time.deltaTime;
            if (timer > 2f)
            {
                body.bodyFlags &= ~CharacterBody.BodyFlags.SprintAnyDirection;
                Debug.Log("Byebye! (timer)");
                Destroy(this);
            }
        }
    }

}
