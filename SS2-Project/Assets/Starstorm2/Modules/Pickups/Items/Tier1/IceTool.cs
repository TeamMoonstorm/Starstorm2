using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using UnityEngine;
using R2API;
using UnityEngine.AddressableAssets;
using Path = System.IO.Path;
using System.Collections.Generic;
namespace SS2.Items
{
    public class IceTool : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acIceTool", SS2Bundle.Items);
        public static GameObject _effect;
        private static string RuntimePath = Path.Combine(Addressables.RuntimePath, "StandaloneWindows64");
        private static string AnimationPath = Path.Combine(Path.GetDirectoryName(SS2Main.Instance.Info.Location), "assetbundles", "ss2vanilla");
        public override void Initialize()
        {
            _effect = AssetCollection.FindAsset<GameObject>("WallJumpEffect");
            //string loader = Path.Combine(RuntimePath, "ror2-base-loader_text_assets_all.bundle");
            //var runtimeLoader = Addressables.LoadAssetAsync<RuntimeAnimatorController>("RoR2/Base/Loader/animLoader.controller").WaitForCompletion();
            //AnimatorModifications loaderMods = new AnimatorModifications(SS2Main.Instance.Info.Metadata);

            //var loaderJumpF = CreateDefaultJumpState();
            //loaderJumpF.Name = "SS2-JumpIceToolF";
            //loaderJumpF.Clip = SS2Assets.LoadAsset<AnimationClip>("LoaderArmature_IceToolFront", SS2Bundle.Vanilla); // idk if you can get assets right from fbx

            //var loaderJumpB = CreateDefaultJumpState();
            //loaderJumpB.Name = "SS2-JumpIceToolB";
            //loaderJumpB.Clip = SS2Assets.LoadAsset<AnimationClip>("LoaderArmature_IceToolBack", SS2Bundle.Vanilla);

            //var loaderJumpL = CreateDefaultJumpState();
            //loaderJumpL.Name = "SS2-JumpIceToolL";
            //loaderJumpL.Clip = SS2Assets.LoadAsset<AnimationClip>("LoaderArmature_IceToolLeft", SS2Bundle.Vanilla);

            //var loaderJumpR = CreateDefaultJumpState();
            //loaderJumpR.Name = "SS2-JumpIceToolR";
            //loaderJumpR.Clip = SS2Assets.LoadAsset<AnimationClip>("LoaderArmature_IceToolRight", SS2Bundle.Vanilla);

            //loaderMods.NewStates.Add("Body", new List<State>{ loaderJumpF, loaderJumpB, loaderJumpL, loaderJumpR });
            //AnimationsAPI.AddModifications(loader, runtimeLoader, loaderMods);
            //var animLoader = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/LoaderBody.prefab").WaitForCompletion().GetComponentInChildren<Animator>();
            //AnimationsAPI.AddAnimatorController(animLoader, runtimeLoader);
        }
        //public State CreateDefaultJumpState()
        //{
        //    State jumpState = new State
        //    {
        //        ClipBundlePath = AnimationPath,
        //        Speed = 1,
        //        WriteDefaultValues = true
        //    };
        //    var transition = new Transition
        //    {
        //        DestinationStateName = "AscendDescend",
        //        TransitionDuration = 1f,
        //        ExitTime = 0.337f,
        //        HasExitTime = true
        //    };
        //    jumpState.Transitions.Add(transition);
        //    return jumpState;
        //}

        public override bool IsAvailable(ContentPack contentPack)
        {
            return SS2Config.enableBeta && base.IsAvailable(contentPack);
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
                        Vector3 closestPoint = body.footPosition;
                        float smallestDistance = Mathf.Infinity;
                        Collider[] hitColliderList;
                        Color effectColor = Color.gray;
                        hitColliderList = Physics.OverlapSphere(body.footPosition, body.radius * 1.5f, LayerIndex.world.mask);
                        if (hitColliderList.Length > 0)
                        {
                            foreach (Collider hc in hitColliderList)
                            {
                                Vector3 point = hc.ClosestPoint(body.footPosition);
                                float distance = (point - body.footPosition).sqrMagnitude;
                                if (distance < smallestDistance)
                                {
                                    smallestDistance = distance;
                                    closestPoint = point;
                                    SurfaceDef hcsd = SurfaceDefProvider.GetObjectSurfaceDef(hc, new Vector3(69, 420, 1337));
                                    if (hcsd != null)
                                    {
                                        effectColor = hcsd.approximateColor;
                                        //could make this sort for closest surfacedef but probably excessive..
                                    }
                                }                            
                            }
                        }
                        var debug = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        Destroy(debug.GetComponent<Collider>());
                        if (this.hasModelAnimator)
                        {
                            int layerIndex = this.modelAnimator.GetLayerIndex("Body");
                            if (layerIndex >= 0)
                            {
                                float transitionTime = 0.05f;
                                BodyAnimatorSmoothingParameters smoothingParameters = this.GetComponent<BodyAnimatorSmoothingParameters>();
                                if (smoothingParameters) transitionTime = smoothingParameters.smoothingParameters.intoJumpTransitionTime;

                                Vector3 moveVector = closestPoint;
                                Vector3 aimDirection = body.inputBank.aimDirection;
                                Vector3 normalized = new Vector3(aimDirection.x, 0f, aimDirection.z).normalized;
                                Vector3 up = transform.up;
                                Vector3 normalized2 = Vector3.Cross(up, normalized).normalized;
                                Vector3 input = new Vector2(Vector3.Dot(moveVector, normalized), Vector3.Dot(moveVector, normalized2));
                                SS2Log.Info("input = " + input);
                                if (input.y < -0.5f) // moving left
                                    this.modelAnimator.CrossFadeInFixedTime("SS2-JumpIceToolR", transitionTime, layerIndex);
                                else if (input.y > 0.5f) // moving right
                                    this.modelAnimator.CrossFadeInFixedTime("SS2-JumpIceToolL", transitionTime, layerIndex);
                                else if (input.x < -0.5f) // back
                                    this.modelAnimator.CrossFadeInFixedTime("SS2-JumpIceToolF", transitionTime, layerIndex);
                                else if (input.x > 0.5f) // forward
                                    this.modelAnimator.CrossFadeInFixedTime("SS2-JumpIceToolB", transitionTime, layerIndex);
                                else
                                    this.modelAnimator.CrossFadeInFixedTime("SS2-JumpIceToolF", transitionTime, layerIndex);
                            }
                        }
                       

                        

                        Util.PlaySound("Play_char_land", gameObject);

                        EffectManager.SpawnEffect(_effect, new EffectData
                        {
                            origin = closestPoint,
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
