using MSU;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static SS2.Items.ShackledLamp;

namespace EntityStates.Executioner2
{
    public class FireChargeGun : BaseSkillState
    {
        [FormatToken("SS2_EXECUTIONER_IONGUN_DESCRIPTION", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        private static float damageCoefficient = 3.5f;
        private static float damageBurstCoefficient = 0.5f;
        private static float procCoefficient = 1.0f;
        private static float baseDuration = 0.115f;
        private static float extraDurationOnLastShot = 0.42f; // same duration as FirePistol
        private static float recoil = 0.6f;
        private static bool useAimAssist = true;
        private static float aimSnapAngle = 15f;
        private static float aimSnapAnglePerShot = 1f;
        private static float aimSnapRange = 70f;
        private static float range = 200f;
        private static float force = 200f;
        private static float spreadBloomValue = 0.4f;
        //public static int minimumShotBurst = 5;
        private static string muzzleString = "Muzzle";
        public static GameObject ionEffectPrefab;
        public static GameObject muzzlePrefabMastery;
        public static GameObject tracerPrefabMastery;
        public static GameObject hitPrefabMastery;
        private string skinNameToken;
        public bool fullBurst = false;
        public bool firstShot = true;

        public static GameObject muzzlePrefab;
        public static GameObject tracerPrefab;
        public static GameObject hitPrefab;

        public int shotsToFire;
        public int shotsFired = 0;

        private float duration;
        private float extraDuration;
        private LampBehavior lamp = null;

        public override void OnEnter()
        {
            base.OnEnter();

            if (firstShot)
                shotsToFire = skillLocator.secondary.stock;

            if (skillLocator.secondary.stock == 0 || shotsToFire == shotsFired)
                return;

            duration = baseDuration / attackSpeedStat;
            extraDuration = extraDurationOnLastShot / attackSpeedStat;

            if (!fullBurst)
            {
                if (skillLocator.secondary.stock >= 10)
                {
                    //fullBurst = true;
                }
            }

            skinNameToken = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

            lamp = GetComponent<LampBehavior>();

            PlayAnimation("Gesture, Override", "FireIonGun");

            if (skinNameToken == "SS2_SKIN_EXECUTIONER2_MASTERY")
            {
                EffectManager.SimpleMuzzleFlash(muzzlePrefabMastery, gameObject, muzzleString, false);
            }
            else
            {
                EffectManager.SimpleMuzzleFlash(muzzlePrefab, gameObject, muzzleString, false);
            }

            Shoot();

            skillLocator.secondary.DeductStock(1);
            shotsFired++;
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration && isAuthority)
            {
                if (skillLocator.secondary.stock > 0 && shotsToFire > shotsFired)
                {
                    FireChargeGun nextState = new FireChargeGun();
                    nextState.shotsFired = shotsFired;
                    nextState.activatorSkillSlot = activatorSkillSlot;
                    if (fullBurst)
                        nextState.fullBurst = true;
                    nextState.firstShot = false;
                    nextState.shotsToFire = shotsToFire;
                    outer.SetNextState(nextState);
                    return;
                }

                if (fixedAge >= duration + extraDuration)
                {
                    outer.SetNextStateToMain();
                }
            }
        }

        private void Shoot()
        {
            //This is desynced, need to network this
            // Is it?
            bool isCrit = RollCrit();
            Util.PlayAttackSpeedSound("ExecutionerSecondary", gameObject, attackSpeedStat);

            if (isAuthority)
            {
                characterBody.SetAimTimer(2f);
                AddRecoil(-2f * recoil, -3f * recoil, -1f * recoil, 1f * recoil);
                characterBody.AddSpreadBloom(spreadBloomValue * 1.0f);
                Ray ray = GetAimRay();

                Vector3 vec = ray.direction;

                if (lamp)
                {
                    lamp.IncrementFire();
                    //SS2Log.Info("special lamp incrementing fire");
                }


                if (useAimAssist)
                {






                    /// TODO: BULLETATTACK PRECOLLECTED RAYCASTS!!!!!!!!!
                    /// 





                    // only aim assist if we hit nothing
                    bool shouldAimAssist = true;
                    if (Util.CharacterSpherecast(gameObject, ray, 1f, out RaycastHit hit, range, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore))
                    {
                        shouldAimAssist = false;
                    }
                    if(shouldAimAssist)
                    {
                        float angle = aimSnapAngle + aimSnapAnglePerShot * shotsFired;
                        BullseyeSearch search = new BullseyeSearch();
                        search.teamMaskFilter = TeamMask.GetEnemyTeams(GetTeam());
                        search.filterByLoS = true;
                        search.maxDistanceFilter = aimSnapRange;
                        search.minAngleFilter = 0;
                        search.maxAngleFilter = angle;
                        search.sortMode = BullseyeSearch.SortMode.Angle;
                        search.filterByDistinctEntity = true;
                        search.searchOrigin = ray.origin;
                        search.searchDirection = ray.direction;
                        search.RefreshCandidates();
                        foreach (HurtBox target in search.GetResults())
                        {
                            if (target && target.healthComponent && target.healthComponent.alive)
                            {
                                vec = target.transform.position - ray.origin;
                                break;
                            }
                        }
                    }
                    
                }

                DamageTypeCombo damageType = DamageType.Generic;
                damageType.damageSource = DamageSource.Secondary;
                bool inMasterySkin = skinNameToken.Equals("SS2_SKIN_EXECUTIONER2_MASTERY");
                var bulletAttack = new BulletAttack
                {
                    aimVector = vec,
                    origin = ray.origin,
                    damage = damageCoefficient * damageStat,
                    damageType = damageType,
                    damageColorIndex = DamageColorIndex.Default,
                    minSpread = 0f,
                    maxSpread = characterBody.spreadBloomAngle,
                    falloffModel = BulletAttack.FalloffModel.None,
                    maxDistance = range,
                    force = force,
                    isCrit = isCrit,
                    owner = gameObject,
                    muzzleName = muzzleString,
                    smartCollision = true,
                    procCoefficient = procCoefficient,
                    radius = 1f,
                    weapon = gameObject,
                    tracerEffectPrefab = inMasterySkin ? tracerPrefabMastery : tracerPrefab,
                    hitEffectPrefab = inMasterySkin ? hitPrefabMastery : hitPrefab,
                    spreadPitchScale = 0.2f,
                    spreadYawScale = 0.2f
                };

                bulletAttack.Fire();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}