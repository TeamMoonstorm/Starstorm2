using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using SS2;
using SS2.Components;

namespace EntityStates.Executioner2
{
    public class ChargeConsecration : BaseSkillState
    {
        private static float baseDuration = 0.5f;
        public static GameObject effectPrefab;
        public static GameObject effectPrefabMastery;
        private static string muzzle = "MuzzleCrush";
        private static string chargeSoundString = "Play_voidman_m2_chargeUp";
        private float duration;
        private uint soundID;
        private GameObject effectInstance;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            soundID = Util.PlayAttackSpeedSound(chargeSoundString, base.gameObject, attackSpeedStat);
            PlayAnimation("Gesture, Override", "Crush", "Secondary.playbackRate", duration);
            characterBody.SetAimTimer(duration + 1f);
            Transform muzzleTransform = FindModelChild(muzzle) ?? characterBody.coreTransform;
            GameObject effect = GetComponent<ExecutionerController>() && GetComponent<ExecutionerController>().inMasterySkin ? effectPrefabMastery : effectPrefab;
            if (muzzleTransform && effect)
            {
                effectInstance = UnityEngine.Object.Instantiate<GameObject>(effect, muzzleTransform.position, muzzleTransform.rotation);
                effectInstance.transform.parent = muzzleTransform;
                ScaleParticleSystemDuration component = effectInstance.GetComponent<ScaleParticleSystemDuration>();
                ObjectScaleCurve component2 = effectInstance.GetComponent<ObjectScaleCurve>();
                if (component)
                {
                    component.newDuration = duration;
                }
                if (component2)
                {
                    component2.timeMax = duration;
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(isAuthority && fixedAge > duration)
            {
                outer.SetNextState(new Consecration());
            }
        }
        public override void OnExit()
        {
            if (effectInstance)
            {
                Destroy(effectInstance);
            }
            AkSoundEngine.StopPlayingID(soundID);
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
    public class Consecration : BaseSkillState
    {
        private static float baseDuration = 0.25f;
        private static float healFraction = 0.05f;
        private static float buffDuration = 6f;
        private static float cooldownDeduction = 1f;
        private static string activationSoundString = "Play_voidman_m2_shoot_fullCharge";
        private static string muzzleString = "MuzzleCrush";
        public static GameObject effectPrefab;
        public static GameObject effectPrefabMastery;
        public static GameObject orbEffectPrefab;
        public static GameObject orbEffectPrefabMastery;
        private float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Util.PlaySound(activationSoundString, gameObject);

            GameObject effect = effectPrefab;
            GameObject orb = orbEffectPrefab;

            if (GetComponent<ExecutionerController>() && gameObject.GetComponent<ExecutionerController>().inMasterySkin)
            {
                effect = effectPrefabMastery;
                orb = orbEffectPrefabMastery;
            }
            
            if (effect)
            {
                EffectManager.SimpleMuzzleFlash(effect, gameObject, muzzleString, false);
            }

            int stock = skillLocator.secondary.stock;
            int maxStock = skillLocator.secondary.maxStock;
            skillLocator.secondary.stock = 0;
            if(isAuthority)
            {
                for(int i = 0; i < stock; i++)
                {
                    skillLocator.DeductCooldownFromAllSkillsAuthority(cooldownDeduction);
                }
            }
            
            if (NetworkServer.active)
            {
                float missingHealth = healthComponent.fullHealth - healthComponent.health;
                healthComponent.Heal(missingHealth * healFraction * stock, default(ProcChainMask));
                

                for(int i = 0; i < stock; i++)
                {
                    if(buffDuration > 0)
                        characterBody.AddTimedBuff(SS2Content.Buffs.bdConsecration, buffDuration);

                    Transform muzzle = FindModelChild(muzzleString);
                    Vector3 position = muzzle ? muzzle.position : characterBody.corePosition;
                    EffectData effectData = new EffectData
                    {
                        origin = position,
                        genericFloat = 0.4f
                    };
                    effectData.SetHurtBoxReference(characterBody.mainHurtBox);
                    EffectManager.SpawnEffect(orb, effectData, true);
                }

                //SS2Util.RefreshAllBuffStacks(characterBody, SS2Content.Buffs.bdConsecration, buffDuration);
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}
