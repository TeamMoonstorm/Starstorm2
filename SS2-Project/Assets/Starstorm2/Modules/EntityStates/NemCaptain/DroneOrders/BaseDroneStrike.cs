using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.NemCaptain.Weapon
{
    public class BaseDroneStrike : BaseSkillState
    {
        [SerializeField]
        public float dmgCoefficient;
        [SerializeField]
        public float procCoefficient;
        [SerializeField]
        public float radius;
        [SerializeField]
        public GameObject explosionPrefab;
        [SerializeField]
        public GameObject dronePrefab;
        [SerializeField]
        public float minDur = 0.2f;
        [SerializeField]
        public GameObject areaIndicator;
        [SerializeField]
        public float maxDistance = 256f;

        [Header("gross and hacky")]
        //not proud of this
        [SerializeField]
        public bool isFrost;
        [SerializeField]
        public bool isShock;
        private GameObject areaIndicatorInstance;
        public override void OnEnter()
        {
            base.OnEnter();

            characterBody.hideCrosshair = true;

            if (isAuthority)
            {
                areaIndicatorInstance = Object.Instantiate(areaIndicator);
                areaIndicatorInstance.transform.localScale = new Vector3(radius, radius, radius);
            }
        }
        public override void OnExit()
        {
            Util.PlaySound(Captain.Weapon.CallAirstrike1.fireAirstrikeSoundString, gameObject);

            if (explosionPrefab != null)
                Explode();

            if (dronePrefab != null && isAuthority)
                PlaceDrone();

            characterBody.hideCrosshair = false;

            if (skillLocator.primary.stock < 1)
                skillLocator.primary.UnsetSkillOverride(gameObject, activatorSkillSlot.skillDef, GenericSkill.SkillOverridePriority.Replacement);

            if (areaIndicatorInstance != null)
                Destroy(areaIndicatorInstance.gameObject);
            base.OnExit();
        }

        public void PlaceDrone()
        {
            Ray aimRay = GetAimRay();
            RaycastHit raycastHit;
            Physics.Raycast(aimRay, out raycastHit, maxDistance, LayerIndex.CommonMasks.bullet);

            GameObject droneObject = Object.Instantiate(dronePrefab, raycastHit.point, Quaternion.identity);
            droneObject.GetComponent<TeamFilter>().teamIndex = teamComponent.teamIndex;
            droneObject.GetComponent<GenericOwnership>().ownerObject = gameObject;

            NetworkServer.Spawn(gameObject);
        }

        public void Explode()
        {
            Debug.Log("exploding");
            Ray aimRay = GetAimRay();
            RaycastHit raycastHit;
            Physics.Raycast(aimRay, out raycastHit, maxDistance, LayerIndex.CommonMasks.bullet);

            bool crit = RollCrit();
            DamageType damageType = DamageType.Stun1s;
            if (isFrost)
                damageType = DamageType.Freeze2s;
            if (isShock)
                damageType = DamageType.Shock5s;

            BlastAttack blast = new BlastAttack()
            {
                radius = radius,
                procCoefficient = procCoefficient,
                position = raycastHit.point,
                attacker = gameObject,
                teamIndex = teamComponent.teamIndex,
                crit = crit,
                baseDamage = characterBody.damage * dmgCoefficient,
                damageColorIndex = DamageColorIndex.Default,
                falloffModel = BlastAttack.FalloffModel.None,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                damageType = damageType
            };
            blast.Fire();

            EffectManager.SimpleEffect(explosionPrefab, raycastHit.point, Quaternion.identity, true);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterBody.SetAimTimer(4f);

            if (isAuthority)
                FixedUpdateAuthority();
        }

        private void FixedUpdateAuthority()
        {
            if (!inputBank.skill1.down && fixedAge > minDur)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void Update()
        {
            base.Update();
            UpdateAreaIndicator();
        }

        private void UpdateAreaIndicator()
        {
            if (areaIndicatorInstance)
            {
                float maxDistance = 256f;

                Ray aimRay = GetAimRay();
                RaycastHit raycastHit;
                if (Physics.Raycast(aimRay, out raycastHit, maxDistance, LayerIndex.CommonMasks.bullet))
                {
                    areaIndicatorInstance.transform.position = raycastHit.point;
                    areaIndicatorInstance.transform.up = raycastHit.normal;
                }
                else
                {
                    areaIndicatorInstance.transform.position = aimRay.GetPoint(maxDistance);
                    areaIndicatorInstance.transform.up = -aimRay.direction;
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
