using RoR2;
using SS2;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.CrystalPickup
{
    public class IdleUnactive : CrystalBaseState
    {
        public bool isActive;
        public static float checkDur;
        public static float radius;
        private float timer;
        private SkinCrystal scc;
        private SphereSearch search;
        private List<HurtBox> hits;
        protected override bool enableInteraction
        {
            get
            {
                return false;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            scc = GetComponent<SkinCrystal>();

            hits = new List<HurtBox>();
            search = new SphereSearch();
            search.mask = LayerIndex.entityPrecise.mask;
            search.radius = radius;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            timer += Time.fixedDeltaTime;
            if (timer >= checkDur)
            {
                timer -= checkDur;

                hits.Clear();
                search.ClearCandidates();
                search.origin = transform.position;
                search.RefreshCandidates();
                search.FilterCandidatesByDistinctHurtBoxEntities();
                search.GetHurtBoxes(hits);
                foreach (HurtBox h in hits)
                {
                    HealthComponent hp = h.healthComponent;
                    if (hp)
                    {
                        CharacterBody body = hp.body;
                        if (body && body.bodyIndex == scc.bodyIndex)
                        {
                            IdleActive nextState = new IdleActive();
                            outer.SetNextState(nextState);
                        }
                    }
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
