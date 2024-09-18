using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
namespace SS2.Components
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
        public AnimateShaderAlpha thing;
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

                if (buffWard && buffWard.radius >= 0f)
                {
                    buffWard.radius -= buffWard.radius / 2f;
                }
                if (NetworkServer.active && timer2 > 1f)
                {
                    NetworkServer.Destroy(this.gameObject);
                }
            }

            if (NetworkServer.active && timer > 0.3f)
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

            bool foundOwner = body && body.HasBuff(buffWard.buffDef);

            //idk what this does and only hunter sigil "used" it so im commenting it out
            //bool foundOwner = false;

            //hits.Clear();
            //ownerSearch.ClearCandidates();
            //ownerSearch.origin = this.transform.position;
            //ownerSearch.RefreshCandidates();
            //ownerSearch.FilterCandidatesByDistinctHurtBoxEntities();
            //ownerSearch.FilterCandidatesByHurtBoxTeam(TeamMask.allButNeutral);
            //ownerSearch.GetHurtBoxes(hits);
            //foreach (HurtBox h in hits)
            //{
            //    CharacterBody charBody = h.healthComponent.body;

            //    if (buffToAmplify != null)
            //    {
            //        if (charBody.HasBuff(buffToAmplify))
            //        {
            //            charBody.SetBuffCount(amplifiedBuff.buffIndex, (int)buffCount);
            //        }
            //    }

            //    if (charBody == body)
            //        foundOwner = true;
            //}

            if (!foundOwner)
            {
                shouldDestroySoon = true;
                if(thing) thing.enabled = true;
            }
                
        }
    }
}
