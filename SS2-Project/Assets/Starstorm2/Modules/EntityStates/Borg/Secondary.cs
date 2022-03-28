using RoR2;
using System.Linq;
using UnityEngine;

namespace EntityStates.Borg
{
    public class BorgFireTrackshot : BaseSkillState
    {
        public static float damageCoefficient = 3.0f;
        public float baseDuration = 0.4f;
        public float recoil = 1f;
        [HideInInspector]
        public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerHuntressSnipe");
        [HideInInspector]
        public GameObject effectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/HitsparkCommandoShotgun");
        [HideInInspector]
        public GameObject critEffectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/critspark");

        private float duration;
        private float fireDuration;
        private bool firstShot = false;
        private bool secondShot = false;
        private bool thirdShot = false;
        private Animator animator;
        private string muzzleString;
        private BullseyeSearch search = new BullseyeSearch();


        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            fireDuration = 0.25f * duration;
            characterBody.SetAimTimer(2f);
            animator = GetModelAnimator();
            muzzleString = "Lowerarm.L_end";


            PlayAnimation("Gesture, Override", "FireM2", "FireArrow.playbackRate", duration);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void FireTrackshot()
        {
            characterBody.AddSpreadBloom(0.75f);
            Ray aimRay = GetAimRay();
            Vector3 targetV;
            BullseyeSearch bullseyeSearch = new BullseyeSearch();
            //EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);
            TeamComponent team = characterBody.GetComponent<TeamComponent>();
            bullseyeSearch.teamMaskFilter = TeamMask.all;
            bullseyeSearch.teamMaskFilter.RemoveTeam(team.teamIndex);
            bullseyeSearch.filterByLoS = true;
            //bullseyeSearch.filterByDistinctEntity = true;
            bullseyeSearch.searchOrigin = aimRay.origin;
            bullseyeSearch.searchDirection = aimRay.direction;
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
            bullseyeSearch.maxDistanceFilter = 10000;
            bullseyeSearch.maxAngleFilter = 30f;
            bullseyeSearch.RefreshCandidates();
            targetV = aimRay.direction;
            HurtBox target = bullseyeSearch.GetResults().FirstOrDefault();
            if ((bool)target)
            {
                targetV = target.transform.position - aimRay.origin;

            }

            string soundString = "BorgSecondary";//base.effectComponent.shootSound;
            //if (isCrit) soundString += "Crit";
            Util.PlaySound(soundString, gameObject);
            if (isAuthority)
            {
                new BulletAttack
                {
                    owner = gameObject,
                    weapon = gameObject,
                    origin = aimRay.origin,
                    aimVector = targetV.normalized,
                    minSpread = 0,
                    maxSpread = 0,
                    damage = damageCoefficient * damageStat,
                    force = 100,
                    tracerEffectPrefab = tracerEffectPrefab,
                    muzzleName = muzzleString,
                    hitEffectPrefab = Util.CheckRoll(critStat, characterBody.master) ? critEffectPrefab : effectPrefab,
                    isCrit = Util.CheckRoll(critStat, characterBody.master),
                    damageType = DamageType.Stun1s
                }.Fire();
                //ProjectileManager.instance.FireProjectile(ExampleSurvivor.ExampleSurvivor.bfgProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageCoefficient * damageStat, 0f, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if ((fixedAge >= fireDuration) && !firstShot)
            {
                FireTrackshot();
                firstShot = true;
            }
            if ((fixedAge >= fireDuration * 2) && !secondShot)
            {
                FireTrackshot();
                secondShot = true;
            }
            if ((fixedAge >= fireDuration * 3) && !thirdShot)
            {
                FireTrackshot();
                thirdShot = true;
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}