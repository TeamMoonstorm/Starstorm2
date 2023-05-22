using Moonstorm;
using Moonstorm.Starstorm2;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace EntityStates.Executioner2
{
    public class FireChargeGun : BaseSkillState
    {
        [TokenModifier("SS2_EXECUTIONER_IONGUN_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static float damageCoefficient = 3.8f;
        public static float damageBurstCoefficient = 0.5f;
        public static float procCoefficient = 1.0f;
        public static float baseDuration = 0.085f;
        public static float recoil = 0.6f;
        [SerializeField]
        public static bool useAimAssist = false;
        public static float aimSnapAngle = 5.5f;
        public static float range = 200f;
        public static float force = 200f;
        public static float spreadBloomValue = 0.8f;
        //public static int minimumShotBurst = 5;
        public static string muzzleString = "Muzzle";
        public static GameObject ionEffectPrefab;
        public static GameObject muzzlePrefabMastery;
        public static GameObject tracerPrefabMastery;
        public static GameObject hitPrefabMastery;
        private string skinNameToken;
        public bool fullBurst = false;

        [HideInInspector]
        public static GameObject muzzlePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/MuzzleflashFMJ.prefab").WaitForCompletion();
        public static GameObject tracerPrefab;
        [HideInInspector]
        public static GameObject tracerExtraPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/TracerCommandoShotgun.prefab").WaitForCompletion();
        [HideInInspector]
        public static GameObject hitPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/HitsparkCommandoShotgun.prefab").WaitForCompletion();

        public int shotsFired = 0;

        private float duration;
        private List<HurtBox> targets;

        public override void OnEnter()
        {
            base.OnEnter();

            if (skillLocator.secondary.stock == 0)
                return;

            duration = baseDuration / attackSpeedStat;

            if (!fullBurst)
            {
                if (skillLocator.secondary.stock >= 10)
                {
                    //fullBurst = true;
                }
            }

            skinNameToken = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

            if (skinNameToken == "SS2_SKIN_EXECUTIONER2_MASTERY")
            {
                muzzlePrefab = muzzlePrefabMastery;
                PlayAnimation("Gesture, Override", "FireIonGun", "Secondary.playbackRate", duration);
                EffectManager.SimpleMuzzleFlash(muzzlePrefab, gameObject, muzzleString, false);
            }
            else
            {
                muzzlePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/MuzzleflashFMJ.prefab").WaitForCompletion();
                PlayAnimation("Gesture, Override", "FireIonGun", "Secondary.playbackRate", duration);
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
                if (skillLocator.secondary.stock > 0)
                {
                    FireChargeGun nextState = new FireChargeGun();
                    nextState.shotsFired = shotsFired;
                    nextState.activatorSkillSlot = activatorSkillSlot;
                    if (fullBurst)
                        nextState.fullBurst = true;
                    outer.SetNextState(nextState);
                    return;
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
                AddRecoil(-2f * recoil, -3f * recoil, -1f * recoil, 1f * recoil);
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
                    damageType = DamageType.Shock5s,
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
                    tracerEffectPrefab = tracerPrefab,
                    hitEffectPrefab = hitPrefab,
                    stopperMask = LayerIndex.world.mask,
                    spreadPitchScale = 0.2f,
                    spreadYawScale = 0.2f
                    //HitEffectNormal = ClayBruiser.Weapon.MinigunFire.bulletHitEffectNormal
                };

                if (skinNameToken == "SS2_SKIN_EXECUTIONER2_MASTERY")
                {
                    bulletAttack = new BulletAttack
                    {
                        aimVector = vec,
                        origin = ray.origin,
                        damage = damageCoefficient * damageStat,
                        damageType = DamageType.Shock5s,
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
                        tracerEffectPrefab = tracerPrefabMastery,
                        hitEffectPrefab = hitPrefabMastery,
                        stopperMask = LayerIndex.world.mask,
                        spreadPitchScale = 0.2f,
                        spreadYawScale = 0.2f
                        //HitEffectNormal = ClayBruiser.Weapon.MinigunFire.bulletHitEffectNormal
                    };
                }
                else
                {
                    bulletAttack = new BulletAttack
                    {
                        aimVector = vec,
                        origin = ray.origin,
                        damage = damageCoefficient * damageStat,
                        damageType = DamageType.Shock5s,
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
                        tracerEffectPrefab = tracerPrefab,
                        hitEffectPrefab = hitPrefab,
                        stopperMask = LayerIndex.world.mask,
                        spreadPitchScale = 0.2f,
                        spreadYawScale = 0.2f
                        //HitEffectNormal = ClayBruiser.Weapon.MinigunFire.bulletHitEffectNormal
                    };
                }

                bulletAttack.Fire();

                if (skillLocator.secondary.stock == 1)
                {
                    //characterBody.SetBuffCount(Moonstorm.Starstorm2.SS2Content.Buffs.bdExeMuteCharge.buffIndex, 0);

                    if (fullBurst)
                    {
                        var bulletAttack2 = new BulletAttack
                        {
                            aimVector = vec,
                            origin = ray.origin,
                            damage = damageBurstCoefficient * damageStat / 2,
                            damageType = DamageType.Shock5s,
                            damageColorIndex = DamageColorIndex.Default,
                            minSpread = 0.5f,
                            maxSpread = 16f,
                            falloffModel = BulletAttack.FalloffModel.None,
                            maxDistance = range,
                            bulletCount = 8,
                            force = force,
                            isCrit = isCrit,
                            owner = gameObject,
                            muzzleName = muzzleString,
                            smartCollision = true,
                            procCoefficient = procCoefficient * 0.5f,
                            radius = 0.5f,
                            weapon = gameObject,
                            tracerEffectPrefab = tracerExtraPrefab,
                            hitEffectPrefab = hitPrefab,
                            stopperMask = LayerIndex.world.mask,
                            spreadPitchScale = 1f,
                            spreadYawScale = 1f
                            //HitEffectNormal = ClayBruiser.Weapon.MinigunFire.bulletHitEffectNormal
                        };
                        if (skinNameToken == "SS2_SKIN_EXECUTIONER2_MASTERY")
                        {
                            bulletAttack2 = new BulletAttack
                            {
                                aimVector = vec,
                                origin = ray.origin,
                                damage = damageBurstCoefficient * damageStat / 2,
                                damageType = DamageType.Shock5s,
                                damageColorIndex = DamageColorIndex.Default,
                                minSpread = 0.5f,
                                maxSpread = 16f,
                                falloffModel = BulletAttack.FalloffModel.None,
                                maxDistance = range,
                                bulletCount = 8,
                                force = force,
                                isCrit = isCrit,
                                owner = gameObject,
                                muzzleName = muzzleString,
                                smartCollision = true,
                                procCoefficient = procCoefficient * 0.5f,
                                radius = 0.5f,
                                weapon = gameObject,
                                tracerEffectPrefab = tracerPrefabMastery,
                                hitEffectPrefab = hitPrefabMastery,
                                stopperMask = LayerIndex.world.mask,
                                spreadPitchScale = 1f,
                                spreadYawScale = 1f
                                //HitEffectNormal = ClayBruiser.Weapon.MinigunFire.bulletHitEffectNormal
                            };
                        }
                        else
                        {
                            bulletAttack2 = new BulletAttack
                            {
                                aimVector = vec,
                                origin = ray.origin,
                                damage = damageBurstCoefficient * damageStat / 2,
                                damageType = DamageType.Shock5s,
                                damageColorIndex = DamageColorIndex.Default,
                                minSpread = 0.5f,
                                maxSpread = 16f,
                                falloffModel = BulletAttack.FalloffModel.None,
                                maxDistance = range,
                                bulletCount = 8,
                                force = force,
                                isCrit = isCrit,
                                owner = gameObject,
                                muzzleName = muzzleString,
                                smartCollision = true,
                                procCoefficient = procCoefficient * 0.5f,
                                radius = 0.5f,
                                weapon = gameObject,
                                tracerEffectPrefab = tracerPrefabMastery,
                                hitEffectPrefab = hitPrefabMastery,
                                stopperMask = LayerIndex.world.mask,
                                spreadPitchScale = 1f,
                                spreadYawScale = 1f
                                //HitEffectNormal = ClayBruiser.Weapon.MinigunFire.bulletHitEffectNormal
                            };
                        }

                        bulletAttack2.Fire();
                    }
                    //Debug.Log("firing final bullet of full burst");
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}