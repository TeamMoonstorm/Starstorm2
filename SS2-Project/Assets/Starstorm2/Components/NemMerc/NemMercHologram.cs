using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using EntityStates;
using EntityStates.NemMerc;

namespace Moonstorm.Starstorm2.Components
{
    public class NemMercHologram : MonoBehaviour
    {
        [NonSerialized]
        public float timeUntilReveal;
        [NonSerialized]
        public float lifetime;
        [NonSerialized]
        public GameObject owner;
        
        public SphereCollider collider;
        public GameObject model;

        private float stopwatch;
        private bool isRevealed;

        private void FixedUpdate()
        {
            this.stopwatch += Time.fixedDeltaTime;
            if (this.stopwatch >= timeUntilReveal && !isRevealed)
            {
                isRevealed = true;
                if (this.model) this.model.SetActive(true);
                if (this.collider) this.collider.enabled = true;
            }
            if (stopwatch >= lifetime)
            {
                Destroy(base.gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject != this.owner) return;

            GameObject bodyObject = other.gameObject;
            HurtBox hurtBox = other.GetComponent<HurtBox>();
            if(hurtBox && hurtBox.healthComponent)
            {
                bodyObject = hurtBox.healthComponent.gameObject;
            }

            EntityStateMachine body = EntityStateMachine.FindByCustomName(bodyObject, "Body");
            if (body && body.SetInterruptState(new NemAssaulter(), InterruptPriority.Pain))
            {
                Destroy(base.gameObject);
            }
                

            
        }




    }
}
