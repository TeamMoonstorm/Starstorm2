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
                        Vector3 point = aimRay.GetPoint(28);

                        //Vector3 axis = Vector3.Cross(Vector3.up, point);
                        float x = UnityEngine.Random.Range(0f, 25f);
                        float z = UnityEngine.Random.Range(0f, 360f);
                        Vector3 vector = Quaternion.Euler(0f, 0f, z) * (Quaternion.Euler(x, 0f, 0f) * Vector3.forward);
                        Debug.Log("vector: " + vector);
                        //float y = vector.y;
                        //vector.y = 0f;
                        //float angle = (Mathf.Atan2(vector.z, vector.x) * 57.29578f - 90f) * 1;
                        //float angle2 = Mathf.Atan2(y, vector.magnitude) * 57.29578f * 1;
                        //Vector3 final = Quaternion.AngleAxis(angle, Vector3.up) * (Quaternion.AngleAxis(angle2, axis) * point);

                        var ray = new Ray(gameObject.transform.position, vector).GetPoint(28);

                        //GameObject whiffTarget = new GameObject();
                        //var boxc = whiffTarget.AddComponent<BoxCollider>();
                        //boxc.isTrigger = true;
                        //var box = whiffTarget.AddComponent<HurtBox>();
                        ////var timer = whiffTarget.AddComponent<DestroyOnTimer>();
                        ////timer.duration = 1f;
                        //whiffTarget.AddComponent<NetworkIdentity>();
                        //var whiffTargetInstance = PrefabAPI.InstantiateClone(TaserWhiffTarget, "taserWhiffInstance");


                        //whiffTargetInstance.AddComponent<TaserWhiffComponent>();
                        //wiffTarget.AddComponent<NetwokIdentity>();
                        //var distance = 28;
                        //var tolerance = 7;
                        //var offset = gameObject.transform.forward * distance;
                        //var position = offset + new Vector3(UnityEngine.Random.Range(-tolerance, tolerance), UnityEngine.Random.Range(0f, 2.0f), UnityEngine.Random.Range(-tolerance, tolerance));
                        //whiffTargetInstance.transform.position = ray;
                        //NetworkServer.Spawn(whiffTargetInstance);

                        

                        Debug.Log("ray eeee : " + ray);
                        //Debug.Log("spawend objewct : " + whiffTargetInstance.transform.position);
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