using Moonstorm;
using Moonstorm.Starstorm2;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Executioner
{
    public class FireIonSummon : BaseSkillState
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

        //public static GameObject muzzlePrefab = Resources.Load<GameObject>("Prefabs/Effects/Muzzleflashes/MuzzleflashHuntressFlurry");
        //public static GameObject tracerPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerCommandoDefault");
        //public static GameObject hitPrefab = Resources.Load<GameObject>("Prefabs/Effects/HitsparkCommando");

        public int shotsFired = 0;

        private float duration;
        private List<HurtBox> targets;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;

            PlayAnimation("Gesture, Override", "FireIonGun", "Secondary.playbackRate", duration);
            //EffectManager.SimpleMuzzleFlash(muzzlePrefab, gameObject, muzzleString, false);

            Shoot();
            if (!HasBuff(SS2Content.Buffs.BuffExecutionerSuperCharged))
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
                    bool shotMinimum = shotsFired >= minimumShotBurst || characterBody.HasBuff(SS2Content.Buffs.BuffExecutionerSuperCharged);
                    if (!shotMinimum || keyDown)
                    {
                        FireIonSummon nextState = new FireIonSummon();
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
            GameObject masterPrefab = SS2Assets.LoadAsset<GameObject>("DroidDroneMaster");

            if (isAuthority)
            {
                /*characterBody.SetAimTimer(2f);
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
                }*/

                /*var bulletAttack = new BulletAttack
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
                bulletAttack.Fire();*/
                var droneSummon = new MasterSummon();
                float x;
                float y;
                float z;
                Vector3 pos;
                x = (characterBody.corePosition.x + UnityEngine.Random.Range(-8, 8));
                y = characterBody.corePosition.y;
                z = (characterBody.corePosition.z + UnityEngine.Random.Range(-8, 8));
                pos = new Vector3(x, y, z);
                droneSummon.position = pos;
                droneSummon.masterPrefab = masterPrefab;
                droneSummon.summonerBodyObject = characterBody.gameObject;

                var droneMaster = droneSummon.Perform();
                if (droneMaster)
                {
                    droneMaster.gameObject.AddComponent<MasterSuicideOnTimer>().lifeTimer = 15;

                    CharacterBody droidBody = droneMaster.GetBody();
                    droidBody.baseDamage *= (0.5f);

                    Inventory droidInventory = droneMaster.inventory;
                    //droidInventory.SetEquipmentIndex(victimEquipment);

                    Util.PlaySound("DroidHead", characterBody.gameObject);
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}