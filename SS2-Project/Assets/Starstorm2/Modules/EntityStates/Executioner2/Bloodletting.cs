using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using SS2;
using SS2.Orbs;
using RoR2.Orbs;
using SS2.Components;

namespace EntityStates.Executioner2
{
    public class ChargeBloodletting : BaseSkillState
    {
        private static float baseDuration = 0.5f;
        public static GameObject effectPrefab;
        public static GameObject effectPrefabMastery;
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
            GameObject vfx = GetComponent<ExecutionerController>() && GetComponent<ExecutionerController>().inMasterySkin ? effectPrefabMastery : effectPrefab;
            if (muzzleTransform && vfx)
            {
                effectInstance = UnityEngine.Object.Instantiate<GameObject>(vfx, muzzleTransform.position, muzzleTransform.rotation);
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
        private static float buffDuration = 8f;
        private static int chargesToGrant = 0;
        private static string activationSoundString = "Play_voidman_R_pop";
        public static GameObject effectPrefab;
        public static GameObject effectPrefabMastery;
        private float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Util.PlaySound(activationSoundString, gameObject);
            GameObject vfx = GetComponent<ExecutionerController>() && GetComponent<ExecutionerController>().inMasterySkin ? effectPrefabMastery : effectPrefab;
            if (vfx)
            {
                EffectManager.SimpleEffect(vfx, characterBody.coreTransform.position, Quaternion.identity, false);
            }
            if (NetworkServer.active)
            {
                DamageInfo damageInfo = new DamageInfo();
                damageInfo.damage = healthComponent.fullHealth * healthFraction;
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

                for(int i = 0; i < buffDuration; i++)
                {
                    characterBody.AddTimedBuff(SS2Content.Buffs.bdConsecration, i + 1);
                }
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
