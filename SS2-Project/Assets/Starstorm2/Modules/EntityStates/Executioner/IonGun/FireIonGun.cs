using Moonstorm;
using Moonstorm.Starstorm2;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.Executioner
{
    public class FireIonGun : BaseSkillState
    {
        [TokenModifier("SS2_EXECUTIONER_IONGUN_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static float damageCoefficient = 3.8f;
        public static float procCoefficient = 1.0f;
        public static float baseDuration = 0.075f;
        public static float recoil = 0.6f;
        //Idk how to serialize a bool so this isnt in the state config
        public static bool useAimAssist = true;
        public static float aimSnapAngle = 7.5f;
        public static float range = 200f;
        public static float force = 200f;
        public static float spreadBloomValue = 0.8f;
        public static int minimumShotBurst = 5;
        public static string muzzleString = "Muzzle";
        public static GameObject ionEffectPrefab;

        [HideInInspector]
        public static GameObject muzzlePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/MuzzleflashFMJ.prefab").WaitForCompletion();
        [HideInInspector]
        public static GameObject tracerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/TracerCommandoShotgun.prefab").WaitForCompletion();
        [HideInInspector]
        public static GameObject hitPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/HitsparkCommandoShotgun.prefab").WaitForCompletion();

        public int shotsFired = 0;

        private float duration;
        private List<HurtBox> targets;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;

            PlayAnimation("Gesture, Override", "FireIonGun", "Secondary.playbackRate", duration);
            EffectManager.SimpleMuzzleFlash(muzzlePrefab, gameObject, muzzleString, false);

            Shoot();
            activatorSkillSlot.DeductStock(1);
            shotsFired++;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration && isAuthority)
            {
                if (activatorSkillSlot.stock > 0)
                {
                    bool keyDown = IsKeyDownAuthority();
                    bool shotMinimum = shotsFired >= minimumShotBurst || characterBody.HasBuff(SS2Content.Buffs.BuffExecutionerSuperCharged) | characterBody.HasBuff(SS2Content.Buffs.BuffExecutionerArmor);
                    if (!shotMinimum || keyDown)
                    {
                        FireIonGun nextState = new FireIonGun();
                        nextState.shotsFired = shotsFired;
                        nextState.activatorSkillSlot = activatorSkillSlot;
                        outer.SetNextState(nextState);
                        return;
                    }
                }
                outer.SetNextStateToMain();
            }
        }

        private void Shoot()
        {
            //This is desynced, need to network this
            bool isCrit = RollCrit();
            Util.PlayAttackSpeedSound("ExecutionerSecondary", gameObject, attackSpeedStat);

            if (isAuthority)
            {
                characterBody.SetAimTimer(2f);
                AddRecoil(-2f * recoil, 3f * recoil, -1f * recoil, 1f * recoil);
                characterBody.AddSpreadBloom(spreadBloomValue * 1.0f);
                Ray ray = GetAimRay();

                Vector3 vec = ray.direction;

                if (useAimAssist)
                {
                    BullseyeSearch search = new BullseyeSearch();
                    search.teamMaskFilter = TeamMask.GetEnemyTeams(GetTeam());
                    search.filterByLoS = true;
                    search.maxDistanceFilter = range;
                    search.minAngleFilter = 0;
                    search.maxAngleFilter = aimSnapAngle;
                    search.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
                    search.filterByDistinctEntity = true;
                    search.searchOrigin = ray.origin;
                    search.searchDirection = ray.direction;
                    search.RefreshCandidates();
                    targets = search.GetResults().Where(new Func<HurtBox, bool>(Util.IsValid)).Distinct(default(HurtBox.EntityEqualityComparer)).ToList();
                    if (targets.Count > 0 && targets[0].healthComponent)
                    {
                        //idx = (idx + 1) % targets.Count;
                        vec = targets[0].transform.position - ray.origin;
                    }
                }

                var bulletAttack = new BulletAttack
                {
                    aimVector = vec,
                    origin = ray.origin,
                    damage = damageCoefficient * damageStat,
                    damageType = DamageType.Generic,
                    damageColorIndex = DamageColorIndex.Default,
                    minSpread = 0f,
                    maxSpread = 0.2f,
                    falloffModel = BulletAttack.FalloffModel.None,
                    maxDistance = range,
                    force = force,
                    isCrit = isCrit,
                    owner = gameObject,
                    muzzleName = muzzleString,
                    smartCollision = true,
                    procCoefficient = procCoefficient,
                    radius = 0.5f,
                    weapon = gameObject,
                    tracerEffectPrefab = tracerPrefab,
                    hitEffectPrefab = hitPrefab
                    //HitEffectNormal = ClayBruiser.Weapon.MinigunFire.bulletHitEffectNormal
                };
                bulletAttack.Fire();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}