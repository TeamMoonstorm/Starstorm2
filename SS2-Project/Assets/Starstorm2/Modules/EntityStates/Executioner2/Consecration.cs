using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using SS2;
namespace EntityStates.Executioner2
{
    public class ChargeConsecration : BaseSkillState
    {
        private static float baseDuration = 0.5f;
        public static GameObject effectPrefab;
        private static string muzzle;
        private static string chargeSoundString = "Play_voidman_R_activate";
        private float duration;
        private uint soundID;
        private GameObject effectInstance;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            soundID = Util.PlayAttackSpeedSound(chargeSoundString, base.gameObject, attackSpeedStat);
            PlayAnimation("Gesture, Override", "Consecration", "Secondary.playbackRate", duration);
            characterBody.SetAimTimer(duration + 1f);
            Transform muzzleTransform = FindModelChild(muzzle) ?? characterBody.coreTransform;
            if (muzzleTransform && effectPrefab)
            {
                effectInstance = UnityEngine.Object.Instantiate<GameObject>(effectPrefab, muzzleTransform.position, muzzleTransform.rotation);
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
        private static float buffDuration = 8f;
        private static float cooldownDeduction = 1f;
        private static string activationSoundString = "Play_voidman_R_pop";
        public static GameObject effectPrefab;
        public static GameObject orbEffectPrefab;
        private float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Util.PlaySound(activationSoundString, gameObject);
            if (effectPrefab)
            {
                EffectManager.SimpleEffect(effectPrefab, characterBody.coreTransform.position, Quaternion.identity, false);
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
                    characterBody.AddTimedBuff(SS2Content.Buffs.bdConsecration, buffDuration);
                    EffectData effectData = new EffectData
                    {
                        origin = characterBody.corePosition,
                        genericFloat = 0.1f
                    };
                    effectData.SetHurtBoxReference(characterBody.mainHurtBox);
                    EffectManager.SpawnEffect(orbEffectPrefab, effectData, true);
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
