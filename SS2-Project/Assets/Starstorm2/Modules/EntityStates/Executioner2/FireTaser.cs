using MSU;
using RoR2;
using RoR2.Orbs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using SS2.Orbs;
using static SS2.Orbs.ExecutionerTaserOrb;
using R2API;
using SS2.Components;

namespace EntityStates.Executioner2
{
    public class FireTaser : BaseSkillState
    {
        public static float damageCoefficient;
        public static float procCoefficient = 0.65f;
        public static float baseDuration = 0.4f;
        public static float recoil = 0f;
        public static float spreadBloom = 0.75f;
        public static float force = 200f;
        public static GameObject muzzleEffectPrefab;
        public static GameObject muzzleEffectPrefabMastery;

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

            PlayAnimation("Gesture, Override", "FireTaser");
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
                //AddRecoil(-0.4f * recoil, -0.8f * recoil, -0.3f * recoil, 0.3f * recoil);

                Vector3 origin = transform.position;
                var modelTransform = base.GetModelTransform();
                if (modelTransform){
                    ChildLocator locator = modelTransform.GetComponent<ChildLocator>();
                    if (locator){
                        Transform source = locator.FindChild("Muzzle");
                        if (source){
                            origin = source.position;
                            origin.y -= .85f;
                        }
                    }
                }

                //Transform source = component.FindChild("Muzzle");
                GameObject muzzle = GetComponent<ExecutionerController>() && GetComponent<ExecutionerController>().inMasterySkin ? muzzleEffectPrefabMastery : muzzleEffectPrefab;
                if (muzzle)
                    EffectManager.SimpleMuzzleFlash(muzzle, gameObject, muzzleString, false);

                if (NetworkServer.active)
                {
                    float dmg = damageCoefficient * damageStat;
                    Ray r = GetAimRay();
                    search = new BullseyeSearch();
                    search.searchOrigin = transform.position;
                    search.searchDirection = r.direction;
                    search.sortMode = BullseyeSearch.SortMode.Distance;
                    search.teamMaskFilter = TeamMask.all;
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
                        DamageTypeCombo damageType = DamageType.Generic;
                        damageType.damageSource = DamageSource.Primary;
                        ExecutionerTaserOrb taserOrb = new ExecutionerTaserOrb();
                        taserOrb.bouncedObjects = new List<HealthComponent>();
                        taserOrb.attacker = gameObject;
                        taserOrb.inflictor = gameObject;
                        taserOrb.teamIndex = GetTeam();
                        taserOrb.damageValue = dmg;
                        taserOrb.isCrit = isCrit;
                        taserOrb.origin = origin;
                        taserOrb.bouncesRemaining = 4;
                        taserOrb.procCoefficient = procCoefficient;
                        taserOrb.target = hurtBox;
                        taserOrb.damageColorIndex = DamageColorIndex.Default;
                        taserOrb.damageType = damageType;
                        taserOrb.skinNameToken = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken; 
                        OrbManager.instance.AddOrb(taserOrb);
                    }
                    else
                    {
                        Ray aimRay = base.GetAimRay();
                        var aimVector = aimRay.direction;
                        aimVector.y -= UnityEngine.Random.Range(-.125f, -.05f);
                        Vector3 axis = Vector3.Cross(Vector3.up, aimVector);

                        float x = UnityEngine.Random.Range(0, 5f); //maxspread
                        float z = UnityEngine.Random.Range(0f, 360f);
                        Vector3 vector = Quaternion.Euler(0f, 0f, z) * (Quaternion.Euler(x, 0f, 0f) * Vector3.forward);
                        float y = vector.y;
                        vector.y = 0f;

                        float angle = (Mathf.Atan2(vector.z, vector.x) * 57.29578f - 90f) * 5; //spreadYawScale
                        float angle2 = Mathf.Atan2(y, vector.magnitude) * 57.29578f;
                        var finalVec = (Quaternion.AngleAxis(angle, Vector3.up) * (Quaternion.AngleAxis(angle2, axis) * aimVector));

                        Vector3 ray = new Ray(gameObject.transform.position, finalVec).GetPoint(attackRange * .95f);
                        var casts = Physics.RaycastAll(new Ray(aimRay.origin, aimVector), attackRange * .95f, (LayerIndex.world.mask | LayerIndex.entityPrecise.mask));

                        //Debug.Log("aimRay.direction: " + aimRay.direction);
                        //bool groundHit = false;
                        if(casts.Length > 0)
                        {
                            ray = casts[0].point;

                        }

                        ExecutionerTaserOrb taserOrb = new ExecutionerTaserOrb();
                        taserOrb.bouncedObjects = new List<HealthComponent>();
                        taserOrb.attacker = gameObject;
                        taserOrb.inflictor = gameObject;
                        taserOrb.teamIndex = GetTeam();
                        taserOrb.damageValue = dmg;
                        taserOrb.isCrit = isCrit;
                        taserOrb.origin = origin;
                        taserOrb.bouncesRemaining = 4;
                        taserOrb.procCoefficient = procCoefficient;
                        taserOrb.target = null;
                        taserOrb.damageColorIndex = DamageColorIndex.Default;
                        taserOrb.damageType = DamageTypeCombo.GenericPrimary;
                        taserOrb.targetPosition = ray;
                        taserOrb.attackerAimVector = aimRay.direction;
                        taserOrb.skinNameToken = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

                        //taserOrb.groundHit = groundHit;
                        OrbManager.instance.AddOrb(taserOrb);
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