using Moonstorm;
using RoR2;
using RoR2.Orbs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Executioner
{
    public class FireTaser : BaseSkillState
    {
        [TokenModifier("SS2_EXECUTIONER_TASER_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static float damageCoefficient;
        public static float procCoefficient = 0.75f;
        public static float baseDuration = 0.4f;
        public static float recoil = 0f;
        public static float spreadBloom = 0.75f;
        public static float force = 200f;

        [HideInInspector]
        public static GameObject muzzleEffectPrefab = Resources.Load<GameObject>("prefabs/effects/muzzleflashes/Muzzleflash1");
        [HideInInspector]
        public static GameObject tracerPrefab = Resources.Load<GameObject>("prefabs/effects/tracers/tracercommandodefault");
        [HideInInspector]
        public static GameObject hitPrefab = Resources.Load<GameObject>("prefabs/effects/HitsparkCommando");

        private float duration;
        private float fireDuration;
        private string muzzleString;
        private bool hasFired;
        private Animator animator;
        private BullseyeSearch search;
        private float minAngleFilter = 0;
        private float maxAngleFilter = 45;
        private float attackRange = 28;
        private List<HealthComponent> previousTargets;
        private int attackFireCount = 1;


        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            duration = baseDuration / attackSpeedStat;
            fireDuration = 0.1f * duration;
            characterBody.SetAimTimer(2f);
            muzzleString = "Muzzle";
            hasFired = false;
            characterBody.outOfCombatStopwatch = 0f;

            PlayAnimation("Gesture, Override", "Primary", "Primary.playbackRate", duration);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= fireDuration)
            {
                Shoot();
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        private void Shoot()
        {
            if (!hasFired)
            {
                hasFired = true;
                bool isCrit = RollCrit();

                //Util.PlaySound(Commando.CommandoWeapon.FirePistol2.firePistolSoundString, gameObject);
                string soundString = "ExecutionerPrimary";
                if (isCrit) soundString += "Crit";
                //Util.PlaySound(soundString, gameObject);
                AddRecoil(-0.4f * recoil, -0.8f * recoil, -0.3f * recoil, 0.3f * recoil);

                if (muzzleEffectPrefab)
                    EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, gameObject, muzzleString, false);

                if (NetworkServer.active)
                {
                    float dmg = damageCoefficient * damageStat;
                    Ray r = GetAimRay();
                    search = new BullseyeSearch();
                    search.searchOrigin = transform.position;
                    search.searchDirection = r.direction;
                    search.sortMode = BullseyeSearch.SortMode.Distance;
                    search.teamMaskFilter = TeamMask.allButNeutral;
                    search.teamMaskFilter.RemoveTeam(GetTeam());
                    search.filterByLoS = false;
                    search.minAngleFilter = minAngleFilter;
                    search.maxAngleFilter = maxAngleFilter;
                    search.maxDistanceFilter = attackRange;
                    search.RefreshCandidates();
                    HurtBox hurtBox = search.GetResults().FirstOrDefault();
                    if (hurtBox)
                    {
                        Util.PlaySound(soundString, gameObject);
                        //previousTargets.Add(hurtBox.healthComponent);
                        LightningOrb lightningOrb = new LightningOrb();
                        lightningOrb.bouncedObjects = new List<HealthComponent>();
                        lightningOrb.attacker = gameObject;
                        lightningOrb.inflictor = gameObject;
                        lightningOrb.teamIndex = GetTeam();
                        lightningOrb.damageValue = dmg;
                        lightningOrb.isCrit = isCrit;
                        lightningOrb.origin = transform.position;
                        lightningOrb.bouncesRemaining = 4;
                        lightningOrb.lightningType = LightningOrb.LightningType.Ukulele;
                        lightningOrb.procCoefficient = procCoefficient;
                        lightningOrb.target = hurtBox;
                        lightningOrb.damageColorIndex = DamageColorIndex.Default;
                        lightningOrb.damageType = DamageType.Generic;
                        OrbManager.instance.AddOrb(lightningOrb);
                    }
                }
                characterBody.AddSpreadBloom(spreadBloom);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}