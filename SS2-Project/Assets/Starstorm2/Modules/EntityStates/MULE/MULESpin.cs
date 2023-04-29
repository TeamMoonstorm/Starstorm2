using Moonstorm;
using Moonstorm.Starstorm2;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.MULE
{
    class MULESpin : BasicMeleeAttack
    {
        [TokenModifier("SS2_MULE_PRIMARY_SPIN_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static float dmgCoefficient;
        public static float minSpinCount;
        public static float maxSpinCount;
        public static float swingTimeCoefficient = 1f;
        public static float duration = 0.3f;
        public int spinCount = 0;
        private string spinFXmuzzle = "SpinFX";
        private ChildLocator childLocator;
        private float ogCDScale;
        private GameObject spinFX;

        public override void OnEnter()
        {
            spinCount++;
            //if (ogCDScale == null) - LOUD INCORRECT BUZZER
                //ogCDScale = skillLocator.utility.cooldownScale;
            //skillLocator.utility.cooldownScale = 0f; 

            Debug.Log("spinCount: " + spinCount);

            characterBody.AddTimedBuffAuthority(SS2Content.Buffs.bdHiddenSpeed5.buffIndex, (0.5f + (spinCount / 3))); //>
            characterBody.RecalculateStats();

            damageCoefficient = dmgCoefficient;
            duration /= attackSpeedStat;
            if (duration <= 0.125f)
                duration = 0.125f;
            //maxSpinCount *= (attackSpeedStat / 6) + 1f;
            childLocator = GetModelChildLocator();

            if (!isGrounded)
            {
                if (spinCount == 1)
                    SmallHop(characterMotor, 15f);
                else
                    SmallHop(characterMotor, 2.5f);
            }

            spinFX = childLocator.FindChild(spinFXmuzzle).gameObject;
            if (spinFX)
                spinFX.SetActive(true);

            base.OnEnter();

            animator = GetModelAnimator();

            if (isAuthority)
            {
                //characterBody.SetBuffCount(SS2Content.Buffs.bdHiddenSpeed5.buffIndex, spinCount);
                characterBody.SetBuffCount(RoR2Content.Buffs.SmallArmorBoost.buffIndex, 1);
            }

            hitEffectPrefab = EntityStates.Bison.Headbutt.hitEffectPrefab;
        }
        public override void PlayAnimation()
        {
            PlayCrossfade("Gesture, Override", "Spin", "Utility.playbackRate", duration, 0.1f);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration && isAuthority)
            {
                if (isAuthority && ((spinCount < minSpinCount) || (spinCount < maxSpinCount && inputBank.skill3.down)))
                {
                    MULESpin nextState = new MULESpin();
                    nextState.spinCount = spinCount;
                    nextState.ogCDScale = ogCDScale;
                    outer.SetNextState(nextState);
                }
                else if (isAuthority)
                {

                    outer.SetNextStateToMain();
                    skillLocator.utility.DeductStock(1);
                    PlayCrossfade("Gesture, Override", "CancelSpin", "Utility.playbackRate", duration, 0.1f);

                    if (spinCount > minSpinCount)
                    {
                        skillLocator.utility.rechargeStopwatch -= ((spinCount - minSpinCount) / 2f);
                    }


                    if (isAuthority)
                    {
                        characterBody.SetBuffCount(SS2Content.Buffs.bdHiddenSlow20.buffIndex, 0);
                        //characterBody.SetBuffCount(SS2Content.Buffs.bdHiddenSpeed5.buffIndex, 0);
                        characterBody.SetBuffCount(RoR2Content.Buffs.SmallArmorBoost.buffIndex, 0);
                        //skillLocator.utility.DeductStock(1);
                    }
                    if (spinFX)
                        spinFX.SetActive(false);
                }
                
                //skillLocator.utility.cooldownScale = ogCDScale;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            if (spinCount <= maxSpinCount - 1)
            {
                overlapAttack.forceVector *= -1f;
                overlapAttack.pushAwayForce *= -1f;
            }
            else
            {
                overlapAttack.forceVector *= 2f;
                overlapAttack.pushAwayForce *= 2f;
            }
            overlapAttack.damageType = DamageType.Stun1s;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
