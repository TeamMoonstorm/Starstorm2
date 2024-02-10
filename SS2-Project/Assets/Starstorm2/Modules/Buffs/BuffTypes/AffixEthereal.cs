using HG;
using Moonstorm.Components;
using R2API;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine.AddressableAssets;
using RoR2.Navigation;
using RoR2.Projectile;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class AffixEthereal : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdEthereal", SS2Bundle.Equipments);

        public override Material OverlayMaterial => SS2Assets.LoadAsset<Material>("matEtherealOverlay", SS2Bundle.Equipments);

        public sealed class Behavior : BaseBuffBodyBehavior
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
                timerDur = baseTimerDur / body.attackSpeed;
                timer = timerDur * 0.75f;

                model = body.modelLocator.modelTransform.GetComponent<CharacterModel>();
                if (model != null)
                {
                    this.etherealEffect = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("EtherealAffixEffect", SS2Bundle.Equipments), model.transform);
                }
            }

            private void FixedUpdate()
            {
                if (!NetworkServer.active)
                    return;

                timer += Time.fixedDeltaTime;
                if (timer >= timerDur)
                {
                    timer = 0;

                    //you never know.
                    if (projectilePrefab != null)
                    {
                        PlaceCircle();
                        if (body.isChampion)
                            PlaceCircle();
                    }
                }
            }

            public void OnDestroy()
            {
                if (etherealEffect)
                    Destroy(this.etherealEffect);
            }

            private void PlaceCircle()
            {
                Debug.Log("Firing projectile: " + projectilePrefab);

                //could be done better for players but idk how much it'd matter lol
                var masters = Run.instance.userMasters.Values.ToList();
                if (masters.Count > 0)
                {
                    CharacterMaster randomMaster = masters[random.Next(masters.Count)];
                    if (randomMaster != null)
                    {
                        CharacterBody body = randomMaster.GetBody();
                        if (body != null)
                        {
                            if (body.healthComponent.alive)
                            {
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
        }
    }
}
