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
        private float timerfuck;
        [SyncVar]
        public bool shouldDestroySoon = false;
        public CharacterBody body;
        private BuffWard buffWard;
        public int buffStacks = 1;
        public BuffIndex stackBuff; //goofy ahh code. if you use this you need to remove the buff manually in a CharacterBody.RemoveBuff hook. check HuntersSigil.cs
        public AnimateShaderAlpha thing;
        private void Start()
        {
            buffWard = GetComponent<BuffWard>();      
        }
        
        private void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;

            if (shouldDestroySoon == true)
            {
                if (thing) thing.enabled = true;

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

            if(NetworkServer.active)
            {
                timerfuck -= Time.fixedDeltaTime;
                if(timerfuck <= 0 && buffStacks > 0)
                {
                    timerfuck = buffWard.interval;
                    this.SetBuffStacks(TeamComponent.GetTeamMembers(buffWard.teamFilter.teamIndex), buffWard.radius*buffWard.radius, base.transform.position);
                }
                    
            }
        }

        // buffward copypaste fml
        private void SetBuffStacks(IEnumerable<TeamComponent> recipients, float radiusSqr, Vector3 currentPosition)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            if (!buffWard?.buffDef)
            {
                return;
            }
            foreach (TeamComponent teamComponent in recipients)
            {
                Vector3 vector = teamComponent.transform.position - currentPosition;
                if (buffWard.shape == BuffWard.BuffWardShape.VerticalTube)
                {
                    vector.y = 0f;
                }
                CharacterBody component = teamComponent.GetComponent<CharacterBody>();
                if (vector.sqrMagnitude <= radiusSqr)
                {               
                    if (component && component.HasBuff(buffWard.buffDef) && (!buffWard.requireGrounded || !component.characterMotor || component.characterMotor.isGrounded))
                    {
                        int buffCount = component.GetBuffCount(stackBuff);
                        for(int i = buffCount; i < buffStacks; i++)
                        {
                            component.AddBuff(stackBuff);// fug
                        }
                    }
                }
            }
        }

        private void CheckBodyInRadius()
        {
            bool foundOwner = body && body.HasBuff(buffWard.buffDef);

            if (!foundOwner)
            {
                shouldDestroySoon = true;           
            }
                
        }
    }
}
