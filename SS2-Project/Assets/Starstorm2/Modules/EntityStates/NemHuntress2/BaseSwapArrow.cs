using RoR2;
using RoR2.Skills;
using RoR2.Projectile;
using UnityEngine;
using Moonstorm.Starstorm2.Components;

namespace EntityStates.NemHuntress2
{
    public class BaseSwapArrow : BaseState
    {
        [SerializeField]
        public float baseDuration = 0.2f;
        /*[SerializeField]
        public SkillDef skillDef;*/
        [SerializeField]
        public GameObject arrowPrefab;

        private float duration;
        private int primaryStockCount;

        private NemHuntressController nhc;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;

            //primaryStockCount = skillLocator.primary.stock;
            //skillLocator.primary.stock = 1;
            //Debug.Log("primary : " + skillLocator.primary.stock);
            //skillLocator.primary.SetSkillOverride(skillLocator.primary, skillDef, GenericSkill.SkillOverridePriority.Contextual);
            //skillLocator.primary.stock = primaryStockCount;
            //Debug.Log("primary : " + skillLocator.primary.stock);

            nhc = characterBody.GetComponent<NemHuntressController>();
            if (nhc != null)
            {
                nhc.currentArrow = arrowPrefab;
            }

            //skillLocator.special.cooldownScale = 0;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
