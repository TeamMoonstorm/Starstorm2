using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;

namespace Moonstorm.Starstorm2.Components
{
    public class WardUtils : NetworkBehaviour
    {
        private float timer;
        private float timer2;
        private bool shouldDestroySoon = false;
        public CharacterBody body;
        public float radius;

        private BuffWard buffWard;
        private TeamIndex teamIndex;
        private SphereSearch ownerSearch;
        private List<HurtBox> hits;
        private BuffDef buffToAmplify;
        public BuffDef amplifiedBuff;
        public float buffCount;

        private void Start()
        {
            if (body == null)
            {
                shouldDestroySoon = true;
                return;
            }

            teamIndex = body.teamComponent.teamIndex;

            buffWard = GetComponent<BuffWard>();

            if (buffWard != null)
            {
                buffToAmplify = buffWard.buffDef;
            }

            hits = new List<HurtBox>();
            ownerSearch = new SphereSearch();
            ownerSearch.mask = LayerIndex.entityPrecise.mask;
            ownerSearch.radius = radius;
        }
        
        private void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;

            if (shouldDestroySoon == true)
            {
                timer2 += Time.fixedDeltaTime;
                if (buffWard.radius >= 0.25f)
                {
                    buffWard.radius -= 0.25f;
                }
                if (timer2 > 0.2f)
                {
                    NetworkServer.Destroy(this.gameObject);
                }
            }

            if (timer > 0.3f)
            {
                if (body == null)
                {
                    shouldDestroySoon = true;
                    return;
                }
                CheckBodyInRadius();
            }
        }

        private void CheckBodyInRadius()
        {
            if (!NetworkServer.active)
                return;

            bool foundOwner = false;

            hits.Clear();
            ownerSearch.ClearCandidates();
            ownerSearch.origin = this.transform.position;
            ownerSearch.RefreshCandidates();
            ownerSearch.FilterCandidatesByDistinctHurtBoxEntities();
            ownerSearch.FilterCandidatesByHurtBoxTeam(TeamMask.allButNeutral);
            ownerSearch.GetHurtBoxes(hits);
            foreach (HurtBox h in hits)
            {
                CharacterBody charBody = h.healthComponent.body;

                if (buffToAmplify != null)
                {
                    if (charBody.HasBuff(buffToAmplify))
                    {
                        /*if (charBody.GetBuffCount(buffToAmplify) < buffCount)
                            charBody.SetBuffCount(buffToAmplify.buffIndex, (int)buffCount);*/
                        charBody.SetBuffCount(amplifiedBuff.buffIndex, (int)buffCount);
                    }
                }

                if (charBody == body)
                    foundOwner = true;
            }

            if (!foundOwner)
                shouldDestroySoon = true;
        }
    }
}
