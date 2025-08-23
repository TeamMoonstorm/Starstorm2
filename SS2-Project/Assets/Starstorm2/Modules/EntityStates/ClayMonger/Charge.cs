using RoR2;
using SS2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.ClayMonger
{
    public class Charge : GenericCharacterMain
    {
        public static float stateDuration;
        public static float damageCoefficient;
        public static float upwardForceMagnitude;
        public static float awayForceMagnitude;
        public static string hitboxGroupName;

        private OverlapAttack _attack;
        private HitBoxGroup _hitboxGroup;

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.AddBuff(SS2.SS2Content.Buffs.bdMongerSlippery);
            ResetOverlapAttack();
        }

        private void ResetOverlapAttack()
        {
            if (!_hitboxGroup)
            {
                _hitboxGroup = HitBoxGroup.FindByGroupName(GetModelTransform().gameObject, hitboxGroupName);
            }
            _attack = new OverlapAttack()
            {
                attacker = gameObject,
                inflictor = gameObject,
                teamIndex = TeamComponent.GetObjectTeam(gameObject),
                damage = damageStat * damageCoefficient,
                forceVector = Vector3.up * upwardForceMagnitude,
                pushAwayForce = awayForceMagnitude,
                hitBoxGroup = _hitboxGroup
            };
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            List<HurtBox> results = new List<HurtBox>();
            if(isAuthority && _attack.Fire(results))
            {
                foreach(var hb in results)
                {
                    if (!hb.healthComponent)
                        continue;

                    var body = hb.healthComponent.body;
                    if (body.bodyFlags.HasFlag(CharacterBody.BodyFlags.ImmuneToGoo))
                        continue;

                    if (!body.HasBuff(SS2Content.Buffs.bdMongerTar))
                    {
                        body.AddTimedBuff(SS2Content.Buffs.bdMongerTar, 5);
                    }
                }
            }
            if(fixedAge > stateDuration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            characterBody.RemoveBuff(SS2.SS2Content.Buffs.bdMongerSlippery);
            base.OnExit();
        }
    }
}