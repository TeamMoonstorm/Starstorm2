using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace Moonstorm.Starstorm2.Components
{
    public class CloneDroneCooldownComponent : NetworkBehaviour
    {
        [SerializeField]
        public static Material defaultMat = SS2Assets.LoadAsset<Material>("matCloneDroneLight", SS2Bundle.Interactables);
        [SerializeField]
        public static Material disableMat = SS2Assets.LoadAsset<Material>("matCloneDroneLightInvalid", SS2Bundle.Interactables);

        private ModelLocator modelLocator;
        private GameObject model;
        private ChildLocator childLocator;
        private GameObject glowMesh;
        private MeshRenderer glowMeshRenderer;

        private SkillLocator skillLoc;
        private bool hasSkill = true;
        private bool chargeMat;

        private float flickerDur = 4.0f;
        private float betweenFlicker = 0.25f;
        private float flickerStopwatch = 0f;
        private Coroutine flickerCoroutine;

        private AkEvent[] droneAkEvents;

        private void Awake()
        {
            modelLocator = GetComponent<ModelLocator>();
            model = modelLocator.modelTransform.gameObject;
            childLocator = model.GetComponent<ChildLocator>();
            glowMesh = childLocator.FindChild("GlowMesh").gameObject;
            glowMeshRenderer = glowMesh.GetComponent<MeshRenderer>();
            skillLoc = GetComponent<SkillLocator>();

            var droneBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/Drone1Body.prefab").WaitForCompletion();

            droneAkEvents = droneBody.GetComponents<AkEvent>();

            foreach (AkEvent akEvent in droneAkEvents)
            {
                var akEventType = akEvent.GetType();
                var newComponent = gameObject.AddComponent(akEventType);

                var fields = akEventType.GetFields();

                foreach (var field in fields)
                {
                    var value = field.GetValue(akEvent);
                    field.SetValue(newComponent, value);
                }
            }
        }

        private void FixedUpdate()
        {
            if (skillLoc.primary.stock < 1)
            {
                if (hasSkill)
                {
                    hasSkill = false;
                    glowMeshRenderer.material = disableMat;
                }

                if (skillLoc.primary.cooldownRemaining <= flickerDur && skillLoc.primary.cooldownRemaining > 0)
                {
                    flickerStopwatch += Time.fixedDeltaTime;
                    if (flickerStopwatch >= betweenFlicker)
                    {
                        flickerStopwatch = 0f;

                        if (glowMeshRenderer.material == disableMat)
                            glowMeshRenderer.material = defaultMat;
                        else
                            glowMeshRenderer.material = disableMat;
                    }
                }
            }
            else
            {
                if (!hasSkill)
                {
                    hasSkill = true;
                    glowMeshRenderer.material = defaultMat;
                }
            }

        }
    }
}
