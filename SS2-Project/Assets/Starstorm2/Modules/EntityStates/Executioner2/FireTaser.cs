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

namespace EntityStates.Executioner2
{
    public class FireTaser : BaseSkillState
    {
        public static float damageCoefficient;
        public static float procCoefficient = 0.75f;
        public static float baseDuration = 0.4f;
        public static float recoil = 0f;
        public static float spreadBloom = 0.75f;
        public static float force = 200f;

        [HideInInspector]
        public static GameObject muzzleEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/Muzzleflash1.prefab").WaitForCompletion();
        [HideInInspector]
        public static GameObject tracerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/TracerCommandoDefault.prefab").WaitForCompletion();
        [HideInInspector] 
        public static GameObject hitPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/HitsparkCommando.prefab").WaitForCompletion();

        [SerializeField]
        public static GameObject TaserWhiffTarget;

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
                        ExecutionerTaserOrb taserOrb = new ExecutionerTaserOrb();
                        taserOrb.bouncedObjects = new List<HealthComponent>();
                        taserOrb.attacker = gameObject;
                        taserOrb.inflictor = gameObject;
                        taserOrb.teamIndex = GetTeam();
                        taserOrb.damageValue = dmg;
                        taserOrb.isCrit = isCrit;
                        taserOrb.origin = transform.position;
                        taserOrb.bouncesRemaining = 4;
                        taserOrb.procCoefficient = procCoefficient;
                        taserOrb.target = hurtBox;
                        taserOrb.damageColorIndex = DamageColorIndex.Default;
                        taserOrb.damageType = DamageType.Generic;
                        OrbManager.instance.AddOrb(taserOrb);
                    }
                    else
                    {
                        Ray aimRay = base.GetAimRay();
                        //aimRay.direction
                        Vector3 point = aimRay.GetPoint(28);

                        //Vector3 axis = Vector3.Cross(Vector3.up, point);
                        float x = UnityEngine.Random.Range(0, 25f);
                        float z = UnityEngine.Random.Range(-90f, 90);
                        Vector3 vector = Quaternion.Euler(0f, 0f, z) * (Quaternion.Euler(x, 0f, 0f) * aimRay.direction);
                        Debug.Log(aimRay.direction + " | vector: " + vector + " | " + x + " | " + z);

                        var ray = new Ray(gameObject.transform.position, aimRay.direction).GetPoint(28);

                        Debug.Log("ray eeee : " + ray);
                        Debug.Log("GRAHH !! : " + gameObject.transform.position);

                        ExecutionerTaserOrb taserOrb = new ExecutionerTaserOrb();
                        taserOrb.bouncedObjects = new List<HealthComponent>();
                        taserOrb.attacker = gameObject;
                        taserOrb.inflictor = gameObject;
                        taserOrb.teamIndex = GetTeam();
                        taserOrb.damageValue = dmg;
                        taserOrb.isCrit = isCrit;
                        taserOrb.origin = transform.position;
                        taserOrb.bouncesRemaining = 4;
                        taserOrb.procCoefficient = procCoefficient;
                        taserOrb.target = null;
                        taserOrb.damageColorIndex = DamageColorIndex.Default;
                        taserOrb.damageType = DamageType.Generic;
                        taserOrb.targetPosition = ray;
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