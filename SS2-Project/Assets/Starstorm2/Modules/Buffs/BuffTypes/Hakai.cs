using Moonstorm.Components;
using R2API;
using RoR2;
using RoR2.Navigation;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class Hakai : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdHakai", SS2Bundle.Equipments);

        //public override Material OverlayMaterial => SS2Assets.LoadAsset<Material>("matHakaiOverlay", SS2Bundle.Equipments);

        public sealed class Behavior : BaseBuffBodyBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdHakai;
            public static GameObject projectilePrefab = SS2Assets.LoadAsset<GameObject>("EtherealCircle", SS2Bundle.Equipments);
            private static System.Random random = new System.Random();
            public static float baseProjTimerDur = 12.5f;
            public static float baseTimerDur = 4f;
            private float timer;
            private float timerDur;
            private bool expired = false;
            private float projTimerDur;
            private float projTimer;

            private List<GameObject> effectInstances;

            public void Start()
            {
                if (body.hullClassification == HullClassification.BeetleQueen)
                    timerDur = baseTimerDur * 3f;
                else if (body.hullClassification == HullClassification.Golem)
                    timerDur = baseTimerDur * 2f;
                else
                    timerDur = baseTimerDur;

                projTimerDur = baseTimerDur / 3f; // / body.attackSpeed;   >
                projTimer = timerDur * 0.95f;

                timer = 0;

                Transform modelTransform = body.modelLocator.modelTransform;
                if (modelTransform)
                {
                    TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                    temporaryOverlay.duration = timerDur;

                    temporaryOverlay.animateShaderAlpha = true;
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 0.1f, 1f, 1f);
                    temporaryOverlay.originalMaterial = SS2Assets.LoadAsset<Material>("matHakaiOverlay", SS2Bundle.Equipments);
                    temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());

                    GameObject charModel = modelTransform.gameObject;
                    if (charModel != null)
                    {
                        CharacterModel cm = charModel.GetComponent<CharacterModel>();
                        if (cm != null)
                        {
                            CharacterModel.RendererInfo[] rendererInfos = cm.baseRendererInfos;
                            if (rendererInfos != null)
                            {
                                for (int i = 0; i < rendererInfos.Length; i++)
                                {
                                    if (rendererInfos[i].renderer != null)
                                    {
                                        if (!rendererInfos[i].ignoreOverlays)
                                        {
                                            GameObject effect = AddParticles(rendererInfos[i].renderer, body.coreTransform, timerDur);
                                            if (effect != null)
                                                effectInstances.Add(effect);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private GameObject AddParticles(Renderer modelRenderer, Transform targetParentTransform, float duration)
            {
                if (modelRenderer is MeshRenderer || modelRenderer is SkinnedMeshRenderer)
                {
                    GameObject effectPrefab = Instantiate(SS2Assets.LoadAsset<GameObject>("HakaiLightning", SS2Bundle.Equipments), targetParentTransform);
                    ParticleSystem ps = effectPrefab.GetComponent<ParticleSystem>();
                    ParticleSystem.ShapeModule shape = ps.shape;
                    if (modelRenderer != null)
                    {
                        if (modelRenderer is MeshRenderer)
                        {
                            shape.shapeType = ParticleSystemShapeType.MeshRenderer;
                            shape.meshRenderer = (MeshRenderer)modelRenderer;
                        }
                        else if (modelRenderer is SkinnedMeshRenderer)
                        {
                            shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
                            shape.skinnedMeshRenderer = (SkinnedMeshRenderer)modelRenderer;
                        }
                    }
                    ParticleSystem.MainModule main = ps.main;
                    main.duration = duration;
                    ps.gameObject.SetActive(true);
                    BoneParticleController bpc = effectPrefab.GetComponent<BoneParticleController>();
                    if (bpc != null && modelRenderer is SkinnedMeshRenderer)
                    {
                        bpc.skinnedMeshRenderer = (SkinnedMeshRenderer)modelRenderer;
                    }
                    return effectPrefab;
                }
                return null;
            }

            public void FixedUpdate()
            {
                if (!NetworkServer.active)
                    return;

                timer += Time.fixedDeltaTime;
                if (timer >= timerDur && !expired)
                {
                    Debug.Log("KILL");
                    body.healthComponent.Suicide();
                    expired = true;
                }

                projTimer += Time.fixedDeltaTime;
                if (projTimer >= projTimerDur)
                {
                    projTimer = 0;

                    //you never know.
                    if (projectilePrefab != null)
                    {
                        PlaceCircle();
                    }
                }
            }

            private void PlaceCircle()
            {
                Debug.Log("Firing projectile: " + projectilePrefab);

                if (body != null)
                {
                    if (body.healthComponent.alive)
                    {
                        NodeGraph groundNodes = SceneInfo.instance.groundNodes;
                        List<NodeGraph.NodeIndex> nodeList = groundNodes.FindNodesInRange(body.transform.position, 0f, 32f, HullMask.Human);
                        NodeGraph.NodeIndex randomNode = nodeList[random.Next(nodeList.Count)];
                        if (randomNode != null)
                        {
                            Vector3 position;
                            groundNodes.GetNodePosition(randomNode, out position);
                            ProjectileManager.instance.FireProjectile(projectilePrefab, position, new Quaternion(0, 0, 0, 0), body.gameObject, 1f, 0f, body.RollCrit(), DamageColorIndex.Default, null, 0);
                        }
                    }
                }
            }
        }
    }
}
