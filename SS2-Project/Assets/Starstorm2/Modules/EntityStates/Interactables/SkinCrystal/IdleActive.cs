using RoR2;
using SS2;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.CrystalPickup
{
    public class IdleActive : CrystalBaseState
    {
        public bool isActive;
        public static float checkDur = 0.3f;
        public static float radius = 18f;
        private float timer;
        private SkinCrystal scc;
        private SphereSearch search;
        private List<HurtBox> hits;
        protected override bool enableInteraction
        {
            get
            {
                return true;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            scc = GetComponent<SkinCrystal>();
            scc.model.SetActive(true);
            Debug.Log("crystal active");

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
                            return;
                        }
                    }
                }

                IdleUnactive nextState = new IdleUnactive();
                outer.SetNextState(nextState);
            }
        }


        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
