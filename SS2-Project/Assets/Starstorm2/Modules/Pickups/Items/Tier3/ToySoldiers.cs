using R2API;
using RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using System;
using MSU;
using System.Collections.Generic;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using UnityEngine.AddressableAssets;
using SS2.Components;
using UnityEngine.Networking;
using RoR2.Navigation;
using RoR2.Items;
using RoR2.CharacterAI;

namespace SS2.Items
{
    public sealed class ToySoldiers : SS2Item
    {
        private const string token = "SS2_ITEM_TOYSOLDIERS_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acToySoldiers", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Base damage of Toy Soldiers. (10 = 100%)")]
        [FormatToken(token, 0)]
        public static int baseDamage = 10;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Base health of Toy Soldiers. (10 = 100%)")]
        [FormatToken(token, 0)]
        public static int baseHealth = 10;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Damage per stack of Toy Soldiers. (10 = 100%)")]
        [FormatToken(token, 0)]
        public static int stackDamage = 5;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Health per stack of Toy Soldiers. (10 = 100%)")]
        [FormatToken(token, 0)]
        public static int stackHealth = 10;

        // also referenced by ToyHelper
        public static float soldierScale = 0.55f;

        private static GameObject soldierPodPrefab;
        private static GameObject commandoMasterPrefab;
        public override void Initialize()
        {
            commandoMasterPrefab = Addressables.LoadAssetAsync<GameObject>("f146e1c7699e35b43b70e119b46875e8").WaitForCompletion();
            soldierPodPrefab = Addressables.LoadAssetAsync<GameObject>("659066785bfffe94fbbd9183a5b12618").WaitForCompletion().InstantiateClone("ToySoldierPod");
            if (soldierPodPrefab != null)
            {
                soldierPodPrefab.transform.localScale *= soldierScale;

                // there is, a lot, to do here
                if (soldierPodPrefab.TryGetComponent(out SurvivorPodController spc))
                {
                    // this handles the camera and some other stuff we don't want at all
                    GameObject.Destroy(spc);
                }

                if (soldierPodPrefab.TryGetComponent(out Highlight hl))
                {
                    // no
                    GameObject.Destroy(hl);
                }

                if (soldierPodPrefab.TryGetComponent(out VehicleSeat vs))
                {
                    // nope
                    if (soldierPodPrefab.TryGetComponent(out ModelLocator ml) && ml.modelTransform != null && ml.modelTransform.TryGetComponent(out ChildLocator cl))
                    {
                        cl.AddChild("ExitPosition", vs.exitPosition);

                        Transform podMesh = ml.modelTransform.Find("EscapePodArmature/Base/EscapePodMesh");
                        if (podMesh != null && podMesh.TryGetComponent(out MeshRenderer mr))
                        {
                            List<Material> podMaterials = new List<Material>();
                            podMaterials.Add(mr.material);
                            podMaterials.Add(AssetCollection.FindAsset<Material>("matToyPodOverlay"));

                            mr.SetMaterials(podMaterials);

                            Transform podDoorMesh = ml.modelTransform.Find("EscapePodArmature/Base/Door/EscapePodDoorMesh");
                            if (podDoorMesh != null && podDoorMesh.TryGetComponent(out MeshRenderer mr2))
                            {
                                mr2.SetMaterials(podMaterials);
                            }

                            Transform podDoorPhysicsMesh = ml.modelTransform.Find("EscapePodArmature/Base/ReleaseExhaustFX/Door,Physics");
                            if (podDoorPhysicsMesh != null && podDoorPhysicsMesh.TryGetComponent(out MeshRenderer mr3))
                            {
                                mr3.SetMaterials(podMaterials);
                            }
                        }
                    }
                    // ok anyway bye
                    GameObject.Destroy(vs);
                }

                var instantiatePrefabBehaviors = soldierPodPrefab.GetComponents<InstantiatePrefabBehavior>();
                foreach (InstantiatePrefabBehavior ibp in instantiatePrefabBehaviors)
                {
                    // kill all
                    GameObject.Destroy(ibp);
                }

                if (soldierPodPrefab.TryGetComponent(out BuffPassengerWhileSeated bpws))
                {
                    GameObject.Destroy(bpws);
                }

                soldierPodPrefab.AddComponent<ToySoldierDropPodHandler>();
            }
            else
            {
                SS2Log.Error("ToySoldiers.Initialize : Failed to create pod prefab!");
            }

            On.EntityStates.SurvivorPod.SurvivorPodBaseState.OnEnter += SurvivorPodBaseState_OnEnter;
            On.EntityStates.SurvivorPod.Landed.OnEnter += Landed_OnEnter;
            On.EntityStates.SurvivorPod.Landed.FixedUpdate += Landed_FixedUpdate;
            On.EntityStates.SurvivorPod.Landed.OnExit += Landed_OnExit;
            On.EntityStates.SurvivorPod.Release.FixedUpdate += Release_FixedUpdate;
        }

        private void SurvivorPodBaseState_OnEnter(On.EntityStates.SurvivorPod.SurvivorPodBaseState.orig_OnEnter orig, EntityStates.SurvivorPod.SurvivorPodBaseState self)
        {
            if (self.gameObject.TryGetComponent(out ToySoldierDropPodHandler tsdph))
            {
                // .. nothing
            }
            else
            {
                orig(self);
            }
        }

        private void Landed_OnExit(On.EntityStates.SurvivorPod.Landed.orig_OnExit orig, EntityStates.SurvivorPod.Landed self)
        {
            if (self.gameObject.TryGetComponent(out ToySoldierDropPodHandler tsdph))
            {
                Util.PlaySound("Stop_UI_podSteamLoop", self.gameObject);
            }
            else
            {
                orig(self);
            }
        }

        private void Landed_OnEnter(On.EntityStates.SurvivorPod.Landed.orig_OnEnter orig, EntityStates.SurvivorPod.Landed self)
        {
            if (self.gameObject.TryGetComponent(out ToySoldierDropPodHandler tsdph))
            {
                self.PlayAnimation("Base", Animator.StringToHash("Idle"));
                Util.PlaySound("Play_UI_podSteamLoop", self.gameObject);
            }
            else
            {
                orig(self);
            }
        }

        private void Landed_FixedUpdate(On.EntityStates.SurvivorPod.Landed.orig_FixedUpdate orig, EntityStates.SurvivorPod.Landed self)
        {
            if (self.gameObject.TryGetComponent(out ToySoldierDropPodHandler tsdph))
            {
                self.outer.SetNextState(new EntityStates.SurvivorPod.PreRelease());
                return;
            }

            orig(self);
        }


        private void Release_FixedUpdate(On.EntityStates.SurvivorPod.Release.orig_FixedUpdate orig, EntityStates.SurvivorPod.Release self)
        {
            orig(self);

            if (NetworkServer.active && self.gameObject.TryGetComponent(out ToySoldierDropPodHandler tsdph))
            {
                Transform modelTransform = self.GetModelTransform();
                if (modelTransform != null && modelTransform.TryGetComponent(out ChildLocator cl) && cl.TryFindChild("ExitPosition", out Transform doorTransform))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var toySummon = new MasterSummon();
                        toySummon.position = new Vector3(doorTransform.position.x + (-1.5f + i), doorTransform.position.y, doorTransform.position.z);
                        toySummon.masterPrefab = commandoMasterPrefab;
                        toySummon.summonerBodyObject = tsdph.owner;

                        var toyMaster = toySummon.Perform();
                        if (toyMaster != null)
                        {
                            CharacterBody toyBody = toyMaster.GetBody();

                            tsdph.AddSummonedBody(toyBody.gameObject);

                            if (toyBody.TryGetComponent(out NetworkStateMachine nsm))
                            {
                                nsm.stateMachines[0].SetNextStateToMain();
                            }

                            Inventory toyInventory = toyMaster.inventory;
                            toyInventory.GiveItemPermanent(SS2Content.Items.ToyHelper);
                            toyInventory.GiveItemPermanent(SS2Content.Items.SpiderverseHelper);

                            toyInventory.GiveItemPermanent(RoR2Content.Items.BoostDamage, (stackDamage + Mathf.Max(baseDamage - 10, 0)) * tsdph.stacks);
                            toyInventory.GiveItemPermanent(RoR2Content.Items.BoostHp, (stackHealth + Mathf.Max(baseHealth - 10, 0)) * tsdph.stacks);

                            if (toyMaster.TryGetComponent(out BaseAI ai))
                            {
                                ai.copyLeaderTarget = true;
                                ai.UpdateTargets();
                            }
                        }
                    }
                }

                self.outer.SetNextState(new EntityStates.SurvivorPod.ReleaseFinished());
            }
        }
        public sealed class BodyBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.ToySoldiers;

            public void OnEnable()
            {
                if (NetworkServer.active) RoR2.TeleporterInteraction.onTeleporterBeginChargingGlobal += TeleporterInteraction_onTeleporterBeginChargingGlobal;
            }
            private void TeleporterInteraction_onTeleporterBeginChargingGlobal(TeleporterInteraction obj)
            {
                if (NetworkServer.active)
                {
                    NodeGraph groundNodes = SceneInfo.instance.groundNodes;
                    List<NodeGraph.NodeIndex> nodes = groundNodes.FindNodesInRange(obj.transform.position, 5, 35, HullMask.Human);

                    if (nodes.Count > 0)
                    {
                        NodeGraph.NodeIndex node = nodes[UnityEngine.Random.Range(0, nodes.Count)];
                        groundNodes.GetNodePosition(node, out Vector3 pos);

                        GameObject podInstance = GameObject.Instantiate(soldierPodPrefab);
                        podInstance.transform.position = new Vector3(pos.x, pos.y, pos.z);

                        if (podInstance.TryGetComponent(out ToySoldierDropPodHandler tsdph))
                        {
                            tsdph.owner = body.gameObject;
                            tsdph.stacks = stack;
                        }

                        NetworkServer.Spawn(podInstance);
                    }
                }
            }

            public void OnDisable()
            {
                if (NetworkServer.active) RoR2.TeleporterInteraction.onTeleporterBeginChargingGlobal -= TeleporterInteraction_onTeleporterBeginChargingGlobal;
            }
        }
    }
}