using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Components
{
    public class CloneDroneCooldownComponent : NetworkBehaviour
    {
        [SerializeField]
        public static Material defaultMat = SS2Assets.LoadAsset<Material>("matCloneDroneLight", SS2Bundle.Indev);
        [SerializeField]
        public static Material disableMat = SS2Assets.LoadAsset<Material>("matCloneDroneLightInvalid", SS2Bundle.Indev);

        private ModelLocator modelLocator;
        private GameObject model;
        private ChildLocator childLocator;
        private GameObject glowMesh;
        private MeshRenderer glowMeshRenderer;

        private SkillLocator skillLoc;
        private bool consoleSpam = false;

        private void Awake()
        {
            modelLocator = GetComponent<ModelLocator>();
            model = modelLocator.modelTransform.gameObject;
            childLocator = model.GetComponent<ChildLocator>();
            glowMesh = childLocator.FindChild("GlowMesh").gameObject;
            glowMeshRenderer = glowMesh.GetComponent<MeshRenderer>();
            skillLoc = GetComponent<SkillLocator>();
        }

        private void FixedUpdate()
        {
            if (skillLoc.primary.stock < 1)
            {
                if (!consoleSpam)
                {
                    consoleSpam = true;
                    //Debug.Log("CONSOLE SPAM LOL");
                }
                if ((skillLoc.primary.cooldownRemaining <= 3 && skillLoc.primary.cooldownRemaining > 2.5) || (skillLoc.primary.cooldownRemaining <= 2 && skillLoc.primary.cooldownRemaining > 1.5) || (skillLoc.primary.cooldownRemaining <= 1 && skillLoc.primary.cooldownRemaining > 0.5) || skillLoc.primary.cooldownRemaining == 0)
                {
                    //Debug.Log("LESS THAN SIX SECONDS CONSOLE SPAM LOL");
                    if (glowMeshRenderer.material != defaultMat)
                        glowMeshRenderer.material = defaultMat;
                }
                if ((skillLoc.primary.cooldownRemaining <= 2.5 && skillLoc.primary.cooldownRemaining > 2) || (skillLoc.primary.cooldownRemaining <= 1.5 && skillLoc.primary.cooldownRemaining > 1) || (skillLoc.primary.cooldownRemaining <= 0.5 && skillLoc.primary.cooldownRemaining > 0))
                {
                    //Debug.Log("ESS THAN FIVE SECONDS CONSOLE SPAM LOL");
                    if (glowMeshRenderer.material != disableMat)
                        glowMeshRenderer.material = disableMat;
                }
            }
        }
    }
}
