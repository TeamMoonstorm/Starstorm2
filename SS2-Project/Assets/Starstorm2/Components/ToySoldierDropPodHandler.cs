using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;

namespace SS2.Components
{
    public class ToySoldierDropPodHandler : NetworkBehaviour
    {
        public GameObject owner;
        public int stacks;
        public List<CharacterBody> summonedBodies = new List<CharacterBody>();

        public void SetOwner(GameObject newOwner)
        {
            owner = newOwner;
        }

        public void AddSummonedBody(GameObject body)
        {
            if (body.TryGetComponent(out CharacterBody b))
            {
                summonedBodies.Add(b);
            }
        }

        public void Start()
        {
            RoR2.TeleporterInteraction.onTeleporterChargedGlobal += TeleporterInteraction_onTeleporterChargedGlobal;
        }

        private void TeleporterInteraction_onTeleporterChargedGlobal(TeleporterInteraction obj)
        {
            if (NetworkServer.active)
            {
                foreach (CharacterBody body in summonedBodies)
                {
                    if (body.TryGetComponent(out HealthComponent hc))
                    {
                        hc.Suicide();
                    }
                }
            }
        }

        public void OnDestroy()
        {
            RoR2.TeleporterInteraction.onTeleporterChargedGlobal -= TeleporterInteraction_onTeleporterChargedGlobal;
        }
    }
}
