//using RoR2;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace EntityStates.Nuke
//{
//    public class FireFusionLaunch : BaseNukeUtilityState
//    {
//        public static float awayForceMagnitude;
//        public static float upwardForceMagnitude;
//        public static float hitPauseDuration;
//        public static float recoilAmplitude;

//        private float _hitPauseTimer;
//        private Vector3 _idealDirection;
//        private OverlapAttack _attack;
//        private bool _inHitPause;
//        private List<HurtBox> _victimsStruck = new List<HurtBox>();
//        private int _origLayer;

//        public override void OnEnter()
//        {
//            base.OnEnter();
//            if(characterMotor) //Nasty fucking hack
//            {
//                _origLayer = characterMotor.capsuleCollider.gameObject.layer;
//                characterMotor.capsuleCollider.gameObject.layer = LayerIndex.fakeActor.intVal;
//                characterMotor.Motor.RebuildCollidableLayers();
//            }
//            characterBody.AddSpreadBloom(charge);
//            HitBoxGroup hitboxGroup = null;
//            Transform modelTransform = GetModelTransform();
//            if(modelTransform)
//            {
//                hitboxGroup = modelTransform.GetComponent<HitBoxGroup>();
//            }
//            _attack = new OverlapAttack
//            {
//                attacker = gameObject,
//                inflictor = gameObject,
//                teamIndex = GetTeam(),
//                damage = charge * damageStat,
//                forceVector = Vector3.up * upwardForceMagnitude,
//                pushAwayForce = awayForceMagnitude,
//                hitBoxGroup = hitboxGroup,
//                isCrit = RollCrit()
//            };
//        }

//        public override void FixedUpdate()
//        {
//            base.FixedUpdate();
//            if (!isAuthority)
//                return;

//            if(!_inHitPause)
//            {
//                if(!_attack.Fire(_victimsStruck))
//                {
//                    return;
//                }
//                _inHitPause = true;
//                _hitPauseTimer = hitPauseDuration;
//                AddRecoil(-0.5f * recoilAmplitude, -0.5f * recoilAmplitude, -0.5f * recoilAmplitude, 0.5f * recoilAmplitude);
//            }
//            else
//            {
//                _hitPauseTimer -= Time.fixedDeltaTime;
//                if (_hitPauseTimer < 0f)
//                {
//                    _inHitPause = false;
//                }
//            }
//        }

//        public override void OnExit()
//        {
//            if (characterMotor) //Nasty fucking hack
//            {
//                characterMotor.capsuleCollider.gameObject.layer = _origLayer;
//                characterMotor.Motor.RebuildCollidableLayers();
//            }
//            base.OnExit();
//        }
//    }
//}
