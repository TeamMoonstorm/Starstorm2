using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
namespace EntityStates.Pickups.Remuneration
{
    public class Wait : BaseState
    {
        public static float duration = 1.75f;

        public override void OnEnter()
        {
            base.OnEnter();

            base.transform.Find("RemunerationPortal").gameObject.SetActive(false);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= duration)
            {
                this.outer.SetNextState(new OfferCoin());
            }
        }
    }
}
