using EntityStates;
using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Items
{
    public class IceTool : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acIceTool", SS2Bundle.Indev);
        public static GameObject _effect;

        public override void Initialize()
        {
            _effect = AssetCollection.FindAsset<GameObject>("WallJumpEffect");
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public class IceToolBodyBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = true)]
            public static ItemDef GetItemDef() => SS2Content.Items.IceTool;
            private float timesJumped;
            private bool check;

            private void Start()
            {
                if (body && body.hasAuthority && body.characterMotor && body.inputBank)
                    check = true;
                //felt ugly checking all of this in update every frame; these things should never change if they're true once

                body.characterMotor.onMovementHit += OnMovementHit;
            }

            private void FixedUpdate()
            {
                if (check && !body.characterMotor.isGrounded && body.inputBank.jump.justPressed)
                {
                    if (this.canWallJump)
                    {
                        WallJump();
                    }
                }
            }

            private bool canWallJump
            {
                get
                {
                    return Physics.CheckSphere(this.transform.position + (Vector3.up * 0.5f), body.radius * 1.5f, LayerIndex.world.mask, QueryTriggerInteraction.Collide);
                }
            }

            private bool hasCharacterMotor
            {
                get
                {
                    return (this.body && this.body.characterMotor);
                }
            }

            private bool hasModelAnimator
            {
                get
                {
                    return (this.body && this.body.modelLocator && this.body.modelLocator.modelTransform && this.body.modelLocator.modelTransform.GetComponent<Animator>());
                }
            }

            private Animator modelAnimator
            {
                get
                {
                    return this.body.modelLocator.modelTransform.GetComponent<Animator>();
                }
            }
            private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
            {
                if (body.characterMotor.isGrounded)
                {
                    timesJumped = 0f;
                }
            }

            private void WallJump()
            {
                if (this.hasCharacterMotor)
                {
                    if (this.body.characterMotor.jumpCount >= this.body.maxJumpCount && timesJumped < stack) // only activate when out of jumps- prevents fighting with hopoo feathers or other possible jump mods. feel free to change
                    {                                                                                        //★ i think itd be nice if you could use these before double jumps but unsure how to make it take priority.
                        float horizontalBonus = 1f;
                        float verticalBonus = 1.3f;
                        EntityStates.GenericCharacterMain.ApplyJumpVelocity(this.body.characterMotor, this.body, horizontalBonus, verticalBonus, false);
                        timesJumped++;

                        if (this.hasModelAnimator)
                        {
                            int layerIndex = this.modelAnimator.GetLayerIndex("Body");
                            if (layerIndex >= 0)
                            {
                                float transitionTime = 0.05f;
                                BodyAnimatorSmoothingParameters smoothingParameters = this.GetComponent<BodyAnimatorSmoothingParameters>();
                                if (smoothingParameters) transitionTime = smoothingParameters.smoothingParameters.intoJumpTransitionTime;
                                this.modelAnimator.CrossFadeInFixedTime("Jump", transitionTime, layerIndex);
                            }
                        }

                        Color effectColor = Color.gray;

                        Collider[] hitColliderList;
                        hitColliderList = Physics.OverlapSphere(transform.position + (Vector3.up * 0.5f), body.radius * 1.5f, LayerIndex.world.mask);
                        if (hitColliderList.Length > 0)
                        {
                            foreach (Collider hc in hitColliderList)
                            {
                                SurfaceDef hcsd = SurfaceDefProvider.GetObjectSurfaceDef(hc, hc.ClosestPoint(transform.position));
                                if (hcsd != null)
                                {
                                    effectColor = hcsd.approximateColor;
                                    //could make this sort for closest surfacedef but probably excessive..
                                }
                            }
                        }

                        Util.PlaySound("Play_char_land", gameObject);

                        EffectManager.SpawnEffect(_effect, new EffectData
                        {
                            origin = body.footPosition,
                            scale = body.radius,
                            color = effectColor
                        }, true);

                        /*EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/FeatherEffect"), new EffectData
                        {
                            origin = this.body.footPosition
                        }, true);*/
                    }
                }
            }

            private void OnDestroy()
            {
                if (body.characterMotor)
                    body.characterMotor.onMovementHit -= OnMovementHit;
            }
        }
    }
}
