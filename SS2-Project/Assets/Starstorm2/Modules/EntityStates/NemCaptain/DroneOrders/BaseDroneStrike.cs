using RoR2;
using SS2;
using SS2.Components;
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
        public float minDur = 0f;
        [SerializeField]
        public GameObject areaIndicator;
        [SerializeField]
        public float maxDistance = 256f;

        private NemCaptainController nemCaptainController;
        private GameObject areaIndicatorInstance;
        private OrderSkillDef activatorOrderSkillDef;

        public override void OnEnter()
        {
            base.OnEnter();

            characterBody.hideCrosshair = true;

            if (isAuthority)
            {
                areaIndicatorInstance = Object.Instantiate(areaIndicator);
                areaIndicatorInstance.transform.localScale = new Vector3(radius, radius, radius);
            }
            nemCaptainController = GetComponent<NemCaptainController>();
            if (activatorSkillSlot.skillDef is OrderSkillDef _activatorOrderSkillDef)
            {
                activatorOrderSkillDef = _activatorOrderSkillDef;
            }
        }

        public override void OnExit()
        {
            characterBody.hideCrosshair = false;

            if (areaIndicatorInstance != null)
                Destroy(areaIndicatorInstance.gameObject);
            base.OnExit();
        }

        public virtual void OnOrderEffect() { }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterBody.SetAimTimer(2f);

            if (isAuthority)
                FixedUpdateAuthority();
        }

        private void FixedUpdateAuthority()
        {
            if (!IsKeyDownAuthority() && fixedAge > minDur)
            {
                Util.PlaySound(Captain.Weapon.CallAirstrike1.fireAirstrikeSoundString, gameObject);
                nemCaptainController.AddStressAndCycleNextOrder(activatorOrderSkillDef.stressValue, activatorSkillSlot);
                OnOrderEffect();
                outer.SetNextState(new ForcedCooldown());
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
