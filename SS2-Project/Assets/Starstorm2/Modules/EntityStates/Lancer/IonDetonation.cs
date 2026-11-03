using RoR2;
using SS2.Survivors;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Lancer
{
    public class IonDetonation : BaseSkillState
    {
        public static float baseDuration = 1f;
        public static float novaDamageCoefficient = 8f;
        public static float novaRadius = 12f;
        public static float novaForce = 2000f;
        public static float novaProcCoefficient = 1f;
        public static GameObject novaEffectPrefab;
        public static GameObject novaImpactEffectPrefab;
        public static string novaSoundString = "";

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            PlayAnimation("Gesture, Override", "IonDetonation", "IonDetonation.playbackRate", duration);
            Detonate();
        }

        private void Detonate()
        {
            Vector3 detonationPosition;
            if (gameObject.TryGetComponent(out SS2.Components.LancerController lancerController))
            {
                detonationPosition = lancerController.GetSpearPosition();
            }
            else
            {
                Debug.LogError("IonDetonation: Failed to get LancerController component.");
                detonationPosition = transform.position;
            }

            Util.PlaySound(novaSoundString, gameObject);

            if (novaEffectPrefab)
            {
                EffectManager.SpawnEffect(novaEffectPrefab, new EffectData
                {
                    origin = detonationPosition,
                    scale = novaRadius
                }, false);
            }

            if (NetworkServer.active)
            {
                BlastAttack blastAttack = new BlastAttack();
                blastAttack.attacker = gameObject;
                blastAttack.baseDamage = damageStat * novaDamageCoefficient;
                blastAttack.baseForce = novaForce;
                blastAttack.bonusForce = Vector3.zero;
                blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
                blastAttack.crit = characterBody.RollCrit();
                blastAttack.damageColorIndex = DamageColorIndex.Default;
                blastAttack.damageType = DamageType.Generic;
                blastAttack.damageType.damageSource = DamageSource.Special;
                blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                blastAttack.inflictor = gameObject;
                blastAttack.position = detonationPosition;
                blastAttack.procCoefficient = novaProcCoefficient;
                blastAttack.radius = novaRadius;
                blastAttack.teamIndex = teamComponent.teamIndex;
                if (novaImpactEffectPrefab)
                {
                    blastAttack.impactEffect = EffectCatalog.FindEffectIndexFromPrefab(novaImpactEffectPrefab);
                }
                blastAttack.Fire();

                SphereSearch sphereSearch = new SphereSearch();
                sphereSearch.origin = detonationPosition;
                sphereSearch.radius = novaRadius;
                sphereSearch.mask = LayerIndex.entityPrecise.mask;
                sphereSearch.RefreshCandidates();
                sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(teamComponent.teamIndex));
                sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
                HurtBox[] hurtBoxes = sphereSearch.GetHurtBoxes();
                foreach (HurtBox hurtBox in hurtBoxes)
                {
                    if (hurtBox.healthComponent && hurtBox.healthComponent.body)
                    {
                        hurtBox.healthComponent.body.AddTimedBuff(Lancer.bdIonField, Lancer.ionFieldDuration);
                    }
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
