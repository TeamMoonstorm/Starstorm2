using EntityStates;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using UnityEngine;

namespace SS2.Items
{
    public class IceTool : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acIceTool", SS2Bundle.Items);

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public class IceToolBodyBehavior : BaseItemBodyBehavior
        {
            private void Update()
            {
                if (this.body && this.body.hasAuthority && this.body.inputBank && this.body.inputBank.jump.justPressed)
                {
                    if (this.canWallJump)
                    {
                        this.Activate();
                    }
                }
            }

            private bool canWallJump
            {
                get
                {
                    return Physics.CheckSphere(this.transform.position + (Vector3.up * 0.5f), 2.1f, LayerIndex.world.mask, QueryTriggerInteraction.Collide);
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

            private void Activate()
            {
                if (this.hasCharacterMotor)
                {
                    if (this.body.characterMotor.jumpCount >= this.body.maxJumpCount) // only activate when out of jumps- prevents fighting with hopoo feathers or other possible jump mods. feel free to change
                    {
                        float horizontalBonus = 1f;
                        float verticalBonus = 1f;
                        GenericCharacterMain.ApplyJumpVelocity(this.body.characterMotor, this.body, horizontalBonus, verticalBonus, false);

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

                        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/FeatherEffect"), new EffectData
                        {
                            origin = this.body.footPosition
                        }, true);
                    }
                }
            }
        }
    }
}
