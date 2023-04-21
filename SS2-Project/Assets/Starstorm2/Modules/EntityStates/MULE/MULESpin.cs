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
            damageCoefficient = dmgCoefficient;
            duration /= attackSpeedStat;
            maxSpinCount *= attackSpeedStat;
            childLocator = GetModelChildLocator();
            spinFX = childLocator.FindChild(spinFXmuzzle).gameObject;
            if (spinFX)
                spinFX.SetActive(true);
            base.OnEnter();
            animator = GetModelAnimator();
            if (isAuthority)
            {
                characterBody.SetBuffCount(SS2Content.Buffs.bdHiddenSlow20.buffIndex, 2);
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
                    //skillLocator.utility.rechargeStopwatch = 6f;
                    outer.SetNextState(nextState);
                    if (isAuthority)
                        characterBody.SetBuffCount(SS2Content.Buffs.bdHiddenSlow20.buffIndex, 2);
                    return;
                }
                outer.SetNextStateToMain();
                skillLocator.utility.DeductStock(1);
                if (spinCount > minSpinCount)
                {
                    skillLocator.utility.rechargeStopwatch -= ((spinCount - minSpinCount) / 2f);
                }
                
                PlayCrossfade("Gesture, Override", "CancelSpin", "Utility.playbackRate", duration, 0.1f);
                if (isAuthority)
                {
                    characterBody.SetBuffCount(SS2Content.Buffs.bdHiddenSlow20.buffIndex, 0);
                    characterBody.SetBuffCount(RoR2Content.Buffs.SmallArmorBoost.buffIndex, 0);
                    //skillLocator.utility.DeductStock(1);
                }
                if (spinFX)
                    spinFX.SetActive(false);
                
                //skillLocator.utility.cooldownScale = ogCDScale;
            }
        }

        public override void OnExit()
        {
            /*if ((spinCount < 5 && !inputBank.skill3.down) || spinCount < 9)
            {
                MULESpin nextState = new MULESpin();
                nextState.spinCount = spinCount;
                outer.SetNextState(nextState);
                return;
            }
            outer.SetNextStateToMain();
            PlayCrossfade("Gesture, Override", "CancelSpin", "Utility.playbackRate", duration, 0.1f);*/
            
            //skillLocator.utility.stock -= 1;
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
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
