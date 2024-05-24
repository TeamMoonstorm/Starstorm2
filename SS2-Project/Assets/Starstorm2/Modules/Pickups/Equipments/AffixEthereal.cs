using MonoMod.Cil;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2.Items;
using UnityEngine.Networking;
using Mono.Cecil.Cil;
using System;
using RoR2.Navigation;
using RoR2.Projectile;
using static MSU.BaseBuffBehaviour;

namespace SS2.Equipments
{
#if DEBUG
    public sealed class AffixEthereal : SS2EliteEquipment
    {
        public Material _matEtherealOverlay; //=> SS2Assets.LoadAsset<Material>("matEtherealOverlay", SS2Bundle.Equipments);

        public override SS2AssetRequest<EliteAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<EliteAssetCollection>("acAffixEthereal", SS2Bundle.Equipments);

        public override bool Execute(EquipmentSlot slot)
        {
            return false;
        }

        public override void Initialize()
        {
            _matEtherealOverlay = AssetCollection.FindAsset<Material>("matEtherealOverlay");

            // Used for suicide buff
            BuffDef buffHakai = AssetCollection.FindAsset<BuffDef>("bdHakai");
            Material matHakaiOverlay = AssetCollection.FindAsset<Material>("matHakaiOverlay");
            // Add relevant hooks
            IL.RoR2.HealthComponent.TakeDamage += EtherealDeathIL;

            // TODO: Make sure Eth overlay is handled

            // Used for suicide buff
            BuffOverlays.AddBuffOverlay(buffHakai, matHakaiOverlay);
        }

        private void EtherealDeathIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            bool ILFound = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(11),
                x => x.MatchCallOrCallvirt<GlobalEventManager>(nameof(GlobalEventManager.ServerDamageDealt)),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<HealthComponent>("get_alive")
            );

            c.Index += 3;

            if (ILFound)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<HealthComponent>>((hc) =>
                {
                    if (hc.health <= 0 && hc.body.HasBuff(SS2Content.Buffs.bdEthereal))
                    {
                        hc.health = 1;
                        if (!hc.body.HasBuff(SS2Content.Buffs.bdHakai))
                        {
                            hc.body.AddBuff(SS2Content.Buffs.bdHakai);
                            hc.body.AddBuff(RoR2Content.Buffs.Intangible);
                        }
                    }
                });
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }
        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        public sealed class BodyBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.EtherealItemAffix;

            public void Start()
            {
                if (NetworkServer.active)
                {
                    body.AddBuff(SS2Content.Buffs.bdEthereal);
                }
            }

            private void OnDestroy()
            {
                if (NetworkServer.active && body.enabled)
                {
                    body.RemoveBuff(SS2Content.Buffs.bdEthereal);
                }
            }

        }
        public sealed class HakaiBuffBehavior : BaseBuffBehaviour
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
                if (!HasAnyStacks)
                    return;

                if (CharacterBody.hullClassification == HullClassification.BeetleQueen)
                    timerDur = baseTimerDur * 3f;
                else if (CharacterBody.hullClassification == HullClassification.Golem)
                    timerDur = baseTimerDur * 2f;
                else
                    timerDur = baseTimerDur;

                projTimerDur = baseTimerDur / 3f;
                projTimer = timerDur * 0.95f;

                timer = 0;

                Transform modelTransform = CharacterBody.modelLocator.modelTransform;
                if (modelTransform)
                {
                    TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                    temporaryOverlay.duration = timerDur;

                    temporaryOverlay.animateShaderAlpha = true;
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 0.1f, 1f, 1f);
                    temporaryOverlay.originalMaterial = SS2Assets.LoadAsset<Material>("matHakaiOverlay", SS2Bundle.Equipments);
                    temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
                }

                GameObject charModel = CharacterBody.modelLocator.modelTransform.gameObject;
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
                                        GameObject effect = AddParticles(rendererInfos[i].renderer, CharacterBody.coreTransform, timerDur);
                                        if (effect != null)
                                            effectInstances.Add(effect);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private GameObject AddParticles(Renderer modelRenderer, Transform targetParentTransform, float duration)
            {
                if (HasAnyStacks && modelRenderer is MeshRenderer || modelRenderer is SkinnedMeshRenderer)
                {
                    GameObject effectPrefab = null;

                    if (CharacterBody.hullClassification == HullClassification.BeetleQueen)
                        effectPrefab = Instantiate(SS2Assets.LoadAsset<GameObject>("HakaiLightningBig", SS2Bundle.Equipments), targetParentTransform);
                    else
                        effectPrefab = Instantiate(SS2Assets.LoadAsset<GameObject>("HakaiLightning", SS2Bundle.Equipments), targetParentTransform);

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
                if (!NetworkServer.active || !HasAnyStacks)
                    return;

                timer += Time.fixedDeltaTime;
                if (timer >= timerDur && !expired)
                {
                    //Debug.Log("KILL");
                    Util.PlaySound("Play_UI_teleporter_event_complete", this.gameObject);
                    var charModel = CharacterBody.modelLocator.modelTransform.gameObject;
                    var mdl = charModel.GetComponent<CharacterModel>();
                    if (mdl != null)
                    {
                        mdl.invisibilityCount++;
                    }
                    CharacterBody.healthComponent.Suicide();
                    EffectManager.SimpleEffect(SS2Assets.LoadAsset<GameObject>("InitialBurst, Effect", SS2Bundle.Equipments), CharacterBody.corePosition, new Quaternion(0f, 0f, 0f, 0f), true);
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

                if (CharacterBody != null)
                {
                    if (CharacterBody.healthComponent.alive)
                    {
                        NodeGraph groundNodes = SceneInfo.instance.groundNodes;
                        List<NodeGraph.NodeIndex> nodeList = groundNodes.FindNodesInRange(CharacterBody.transform.position, 0f, 32f, HullMask.Human);
                        NodeGraph.NodeIndex randomNode = nodeList[random.Next(nodeList.Count)];
                        if (randomNode != null)
                        {
                            Vector3 position;
                            groundNodes.GetNodePosition(randomNode, out position);
                            ProjectileManager.instance.FireProjectile(projectilePrefab, position, new Quaternion(0, 0, 0, 0), CharacterBody.gameObject, 1f, 0f, CharacterBody.RollCrit(), DamageColorIndex.Default, null, 0);
                        }
                    }
                }
            }
        }

        public sealed class EtherealBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => SS2Content.Buffs.bdEthereal;
            public static GameObject projectilePrefab = SS2Assets.LoadAsset<GameObject>("EtherealCircle", SS2Bundle.Equipments);
            private static System.Random random = new System.Random();
            public static float baseTimerDur = 12.5f;

            private CharacterModel model;
            private GameObject etherealEffect;

            private float timerDur;
            private float timer;

            public void Start()
            {
                timerDur = baseTimerDur; // / body.attackSpeed;   >
                timer = timerDur * 0.75f;

                Util.PlaySound("Play_ui_teleporter_activate", this.gameObject);

                model = CharacterBody.modelLocator.modelTransform.GetComponent<CharacterModel>();
                if (model != null)
                {
                    this.etherealEffect = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("EtherealAffixEffect", SS2Bundle.Equipments), model.transform);
                }
            }

            private void FixedUpdate()
            {
                if (!NetworkServer.active)
                    return;

                // TODO: Will HasAnyStacks break this fixed update? I hope not!
                if (HasAnyStacks)
                {
                    timer += Time.fixedDeltaTime;
                    if (timer >= timerDur)
                    {
                        timer = 0;
                        if (CharacterBody.isChampion)
                            timer = timerDur / 2f;

                        //you never know.
                        if (projectilePrefab != null)
                        {
                            PlaceCircle();
                        }
                    }
                }
                
            }

            // Pre MSU 2.0 this was an OnDestroy method
            protected override void OnAllStacksLost()
            {
                // TODO From Nebby: Also, if the Effect is the one from the ExtendedEliteDef, the EliteBehaviour from MSU takes care of enabling and disabling the vfx as needed.
                if (etherealEffect)
                    Destroy(this.etherealEffect);
            }


            private void PlaceCircle()
            {
                Debug.Log("Firing projectile: " + projectilePrefab);

                // We dont need a HasAnyStacks check since we check in the FixedUpdate before it calls this method.
                if (CharacterBody != null)
                {
                    if (CharacterBody.healthComponent.alive)
                    {
                        NodeGraph groundNodes = SceneInfo.instance.groundNodes;
                        List<NodeGraph.NodeIndex> nodeList = groundNodes.FindNodesInRange(CharacterBody.transform.position, 0f, 32f, HullMask.Human);
                        NodeGraph.NodeIndex randomNode = nodeList[random.Next(nodeList.Count)];
                        if (randomNode != null)
                        {
                            Vector3 position;
                            groundNodes.GetNodePosition(randomNode, out position);
                            ProjectileManager.instance.FireProjectile(projectilePrefab, position, new Quaternion(0, 0, 0, 0), CharacterBody.gameObject, 1f, 0f, CharacterBody.RollCrit(), DamageColorIndex.Default, null, 0);
                        }
                    }
                }
            }
        }

    }
#endif
}
