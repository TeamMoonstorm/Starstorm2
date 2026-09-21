using MSU;
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections.Generic;
using RoR2.Items;
using UnityEngine.Networking;
using RoR2.CharacterAI;

namespace SS2.Items
{
    public sealed class ToyHelper : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acToyHelper", SS2Bundle.Items);
        public static Texture toyMandoSprite;
        private static float toyScale = 25f; // gets multiplied by .01 then subracted from 1 e.g. 25f -> 0

        public override void Initialize()
        {
            BuffOverlays.AddBuffOverlay(AssetCollection.FindAsset<BuffDef>("bdToy"), AssetCollection.FindAsset<Material>("matToySoldier"));
            toyMandoSprite = AssetCollection.FindAsset<Texture>("texToyCommandoIcon");
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class BodyBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.ToyHelper;

            private float oldAmount;
            private AISkillDriver[] skillDriverCache = null;
            private Transform modelTransform;
            private Texture oldIcon = null;

            // for death handling
            private HealthComponent healthComponent;
            private float timeUntilFreeze = 0.2f;
            private float deathTimer = 0f;
            private bool froze = false;

            public void Start()
            {
                // these should always be the same
                UpdateScale(toyScale);

                // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                // i really hate this override thats specifically for commando, but i dont want to procedurally green every icon while accounting for the blue outline that should remain blue
                // additionally, i dont want to make a new ToyCommandoBody prefab because that kind of would defeat the entire point of this helper item / the entire setup
                // ughhh
                if (BodyCatalog.FindBodyIndex(body) == BodyCatalog.FindBodyIndex("CommandoBody"))
                {
                    oldIcon = body.portraitIcon;
                    body.portraitIcon = ToyHelper.toyMandoSprite;
                }


                if (body && body.healthComponent)
                {
                    healthComponent = body.healthComponent;
                }

                // for some reason when players die they get like 7 overlays????????? and they shine bright like a small green sun. nothing like epilipsy tier but jarring to me as a player
                // thankfully im never playing as a toy, and the ai bodies dont seem to do this, but would be nice to know why and how to fix. thought checking for alive / already owning buff would
                if (NetworkServer.active && healthComponent && healthComponent.alive && body && !body.HasBuff(SS2Content.Buffs.bdToy))
                {
                    body.AddBuff(SS2Content.Buffs.bdToy);
                }
            }

            private void FixedUpdate()
            {
                if (!froze && healthComponent && !healthComponent.alive)
                {
                    deathTimer += Time.fixedDeltaTime;
                    if (deathTimer >= timeUntilFreeze)
                    {
                        if (body && body.modelLocator && body.modelLocator.modelTransform && body.modelLocator.modelTransform.TryGetComponent(out RagdollController rc))
                        {
                            // fall over in static pose e.g. https://youtu.be/TXvR6yxUVSw?t=77 
                            foreach (Transform bone in rc.bones)
                            {
                                if (bone && bone.gameObject && bone.gameObject.layer != LayerIndex.ragdoll.intVal)
                                {
                                    continue;
                                }

                                foreach (Joint joint in bone.GetComponents<Joint>())
                                {
                                    // its in the world and not attached to a body
                                    if (joint && !joint.connectedBody || joint is FixedJoint)
                                    {
                                        continue;
                                    }

                                    Rigidbody connectedBody = joint.connectedBody;
                                    Vector3 anchor = joint.anchor;
                                    bool enableCollision = joint.enableCollision;

                                    Destroy(joint);

                                    // make new fixedjoint with properties of old joint
                                    FixedJoint fixedJoint = bone.gameObject.AddComponent<FixedJoint>();
                                    fixedJoint.anchor = anchor;
                                    fixedJoint.autoConfigureConnectedAnchor = true;
                                    fixedJoint.connectedBody = connectedBody;
                                    fixedJoint.enableCollision = enableCollision;
                                    fixedJoint.breakForce = Mathf.Infinity;
                                    fixedJoint.breakTorque = Mathf.Infinity;
                                }
                            }
                        }

                        froze = true;
                    }
                }
            }

            private void UpdateScale(float amount)
            {
                float scale = 1 - amount * 0.01f;
                float oldScale = 1 - oldAmount * 0.01f;
                float deltaScale = scale / oldScale;
                oldAmount = amount;

                if (deltaScale == 1) return;

                if (!modelTransform && body.modelLocator)
                {
                    modelTransform = body.modelLocator.modelTransform;
                }
                if (modelTransform)
                {
                    body.radius *= deltaScale;
                    modelTransform.localScale *= deltaScale;

                    if (modelTransform.TryGetComponent(out RagdollController rc))
                    {
                        foreach (Transform bone in rc.bones)
                        {
                            if (bone && bone.gameObject && bone.gameObject.layer != LayerIndex.ragdoll.intVal)
                            {
                                continue;
                            }

                            foreach (Joint joint in bone.GetComponents<Joint>())
                            {
                                Vector3 worldAnchor = joint.transform.TransformPoint(joint.anchor);

                                joint.autoConfigureConnectedAnchor = false;
                                joint.connectedAnchor = joint.connectedBody.transform.InverseTransformPoint(worldAnchor);
                            }
                        }
                    }
                }

                if (NetworkServer.active && body &&body.master)
                {
                    ModifySkillDrivers(deltaScale);
                }
            }

            private void ModifySkillDrivers(float deltaScale)
            {
                if (!body || body.master.aiComponents.Length < 1)
                {
                    return;
                }

                if (skillDriverCache == null)
                {
                    skillDriverCache = body.master.aiComponents[0].skillDrivers;
                }

                foreach (AISkillDriver driver in skillDriverCache)
                {
                    driver.maxDistance *= deltaScale;
                    driver.minDistance *= deltaScale;
                }
            }

            private void OnDestroy()
            {
                if (healthComponent && healthComponent.alive)
                {
                    UpdateScale(0);

                    // ??????????
                    if (oldIcon)
                    {
                        body.portraitIcon = oldIcon;
                    }

                    if (NetworkServer.active && body.HasBuff(SS2Content.Buffs.bdToy))
                    {
                        body.SetBuffCount(SS2Content.Buffs.bdToy.buffIndex, 0);
                    }
                }
            }
        }
    }
}
