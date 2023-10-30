using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;
namespace EntityStates.Pickups.Remuneration
{
    public class OfferCoin : BaseState
    {
        public static float duration = 1.93f;

        public override void OnEnter()
        {
            base.OnEnter();

            Transform t = base.transform.Find("RemunerationPortal");
            t.gameObject.SetActive(true);
            t.GetComponent<ScaleParticleSystemDuration>().newDuration = duration;
            t.Find("Portal/Sphere").GetComponent<ObjectScaleCurve>().timeMax = duration; // cringe idc
            Util.PlaySound("RemunerationSpawn2", base.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.fixedAge >= duration)
            {
                this.outer.SetNextState(new Reward());
            }
        }
    }
}
