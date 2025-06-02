using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using SS2;
using SS2.Orbs;
using RoR2.Orbs;
namespace EntityStates.Executioner2
{
    public class ChargeBloodletting : BaseSkillState
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
            PlayAnimation("Gesture, Override", "Bloodletting", "Utility.playbackRate", duration);
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
            if (isAuthority && fixedAge > duration)
            {
                outer.SetNextState(new Bloodletting());
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

    public class Bloodletting : BaseSkillState
    {
        private static float baseDuration = 0.25f;
        private static float healthFraction = 0.25f;
        private static float orbDuration = 0.25f;
        private static float debuffDuration = 3f;
        private static float debuffRadius = 16f;
        private static int chargesToGrant = 3;
        private static string activationSoundString = "Play_voidman_R_pop";
        public static GameObject effectPrefab;
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
            if (NetworkServer.active)
            {
                DamageInfo damageInfo = new DamageInfo();
                damageInfo.damage = healthComponent.combinedHealth * healthFraction;
                damageInfo.position = base.characterBody.corePosition;
                damageInfo.force = Vector3.zero;
                damageInfo.damageColorIndex = DamageColorIndex.Default;
                damageInfo.crit = false;
                damageInfo.attacker = null;
                damageInfo.inflictor = null;
                damageInfo.damageType = (DamageType.NonLethal | DamageType.BypassArmor);
                damageInfo.procCoefficient = 0f;
                damageInfo.procChainMask = default(ProcChainMask);
                healthComponent.TakeDamage(damageInfo);


                for (int i = 0; i < chargesToGrant; i++)
                {
                    ExecutionerBloodOrb ionOrb = new ExecutionerBloodOrb();
                    ionOrb.duration = orbDuration;
                    ionOrb.origin = characterBody.corePosition;
                    ionOrb.target = characterBody.mainHurtBox;
                    OrbManager.instance.AddOrb(ionOrb);
                }

                CreateFearAoe();
            }
        }

        private void CreateFearAoe()
        {
            SphereSearch fearSearch = new SphereSearch();
            fearSearch.mask = LayerIndex.entityPrecise.mask;
            fearSearch.radius = debuffRadius;
            fearSearch.origin = characterBody.corePosition;
            fearSearch.RefreshCandidates();
            fearSearch.FilterCandidatesByDistinctHurtBoxEntities();
            fearSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(teamComponent.teamIndex));
            foreach (HurtBox h in fearSearch.GetHurtBoxes())
            {
                HealthComponent hp = h.healthComponent;
                if (hp)
                {
                    SetStateOnHurt ssoh = hp.GetComponent<SetStateOnHurt>();
                    if (ssoh)
                    {
                        ssoh.SetStun(-1f);
                    }

                    CharacterBody body = hp.body;
                    if (body && body != base.characterBody)
                    {
                        body.AddTimedBuff(SS2Content.Buffs.BuffFear, debuffDuration);

                        if (body.master && body.master.aiComponents.Length > 0 && body.master.aiComponents[0])
                        {
                            body.master.aiComponents[0].stateMachine.SetNextState(new AI.Walker.Fear { fearTarget = base.gameObject });
                        }
                    }
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}
