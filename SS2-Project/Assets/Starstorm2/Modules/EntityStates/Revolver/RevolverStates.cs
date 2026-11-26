using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using SS2;
using SS2.Items;
namespace EntityStates.Revolver
{
    public class SearchForTargets : BaseState
    {
        private static float minDuration = 0.67f; // longer spin with more shots would be swag but we need a looping sfx
        private static float maxDuration = 0.67f;

        private static int minShots = 1;
        private static int maxShots = 6;

        private static string enterSoundString = "Play_bandit2_R_load";
        private static string startLoopSoundString = "";
        private static string endLoopSoundString = "";

        private int shotCount;
        private float duration;
        public override void OnEnter()
        {
            base.OnEnter();

            Util.PlaySound(enterSoundString, gameObject);
            Util.PlaySound(startLoopSoundString, gameObject);

            Transform fx = FindModelChild("ReloadFX");
            if (fx)
            {
                fx.gameObject.SetActive(true);
            }
            Transform mesh = FindModelChild("Mesh");
            if (mesh)
            {
                mesh.gameObject.SetActive(false);
            }

            if (isAuthority)
            {
                shotCount = GetComponent<RevolverTurretController>().shotCount;
                duration = Util.Remap(shotCount, minShots, maxShots, minDuration, maxDuration);
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration)
            {
                outer.SetNextState(new FireTurret { shotsRemaining = shotCount });
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            Util.PlaySound(endLoopSoundString, gameObject);

            Transform fx = FindModelChild("ReloadFX");
            if (fx)
            {
                fx.gameObject.SetActive(true);
            }
            Transform mesh = FindModelChild("Mesh");
            if (mesh)
            {
                mesh.gameObject.SetActive(false);
            }
        }
    }
    public class FireTurret : BaseState
    {
        private static float baseDuration = 0.125f;
        private static float force = 150f;
        private static float bulletRadius = .1f;
        private static float bulletDistance = 999f;
        private static float bulletTargetSpread = 5f;
        private static float bulletFreeSpread = 25f;

        private static float scanDistance = 128f;
        private static float scanAngle = 90f;

        public static GameObject explosionEffectPrefab;
        public static GameObject tracerPrefab;
        public static GameObject hitEffectPrefab;
        public static GameObject muzzleFlashPrefab;
        private static string fireSoundString = "Play_bandit2_R_fire"; // NEEWD REVOLVER SOUND D
        private static float soundPitchFuck = 1.8f;
        private static string muzzleString = "Muzzle";

        public int shotsRemaining;

        private GameObject attacker;
        private float damageValue;
        private float procCoefficient;
        private bool crit;
        private ProcChainMask procChainMask;
        private HurtBox target;
        public override void OnSerialize(NetworkWriter writer)
        {
            writer.Write(HurtBoxReference.FromHurtBox(target));
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            target = reader.ReadHurtBoxReference().ResolveHurtBox();
        }
        public override void OnEnter()
        {
            base.OnEnter();

            Transform mesh = FindModelChild("Mesh");
            if (mesh)
            {
                mesh.gameObject.SetActive(true);
            }
            Transform fx = FindModelChild("ReloadFX");
            if (fx)
            {
                fx.gameObject.SetActive(false);
            }

            if (isAuthority)
            {
                var controller = GetComponent<RevolverTurretController>();
                attacker = controller.owner;
                damageValue = controller.damageValue;
                procCoefficient = controller.procCoefficient;
                procChainMask = controller.procChainMask;
                crit = controller.crit;

                target = SearchForTarget();
            }

            FireAt(target);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority && fixedAge >= baseDuration)
            {
                if (shotsRemaining > 0)
                {
                    outer.SetNextState(new FireTurret { shotsRemaining = shotsRemaining - 1 });
                }
                else
                {
                    outer.SetNextState(new FadeOut());
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();

        }

        private HurtBox SearchForTarget()
        {
            TeamMask filter = TeamMask.allButNeutral;
            filter.RemoveTeam(TeamComponent.GetObjectTeam(attacker));
            BullseyeSearch search = new BullseyeSearch
            {
                teamMaskFilter = filter,
                filterByLoS = true,
                searchOrigin = transform.position,
                searchDirection = transform.forward,
                sortMode = BullseyeSearch.SortMode.DistanceAndAngle,
                maxDistanceFilter = scanDistance,
                maxAngleFilter = scanAngle,
            };
            search.RefreshCandidates();
            search.FilterOutGameObject(base.gameObject);

            List<HurtBox> targets = new List<HurtBox>();
            search.GetResults(targets);

            if (targets.Count > 0)
            {
                return targets[UnityEngine.Random.Range(0, targets.Count)];
            }
            return null;
        }

        private void AimAt(Vector3 direction)
        {
            transform.rotation = Util.QuaternionSafeLookRotation(direction);
        }

        private void FireAt(HurtBox target)
        {
            PlayAnimation("Base", "Fire");
            EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, gameObject, muzzleString, false);
            Util.PlayAttackSpeedSound(fireSoundString, gameObject, soundPitchFuck);

            Vector3 origin = transform.position;
            Transform muzzleTransform = FindModelChild(muzzleString);
            if (muzzleTransform)
            {
                origin = muzzleTransform.position;
            }

            Vector3 direction = transform.forward;
            direction = Util.ApplySpread(direction, 0f, bulletFreeSpread, 1f, 0.67f);
            if (target)
            {
                direction = target.transform.position - transform.position;
                direction = Util.ApplySpread(direction, 0f, bulletTargetSpread, 1f, 1f);
            }
            AimAt(direction);

            if (isAuthority)
            {
                float maxSpread = bulletFreeSpread;
                if (target)
                {
                    maxSpread = bulletTargetSpread;
                }
                BulletAttack bulletAttack = new BulletAttack
                {
                    aimVector = direction,
                    origin = origin,
                    owner = attacker,
                    weapon = gameObject,
                    damage = damageValue,
                    damageColorIndex = DamageColorIndex.Item,
                    damageType = DamageType.Generic,
                    procChainMask = procChainMask,
                    falloffModel = BulletAttack.FalloffModel.None,
                    force = force,
                    procCoefficient = procCoefficient,
                    maxDistance = bulletDistance,
                    radius = bulletRadius,
                    isCrit = crit,
                    muzzleName = muzzleString,
                    minSpread = 0,
                    maxSpread = maxSpread,
                    hitEffectPrefab = hitEffectPrefab,
                    smartCollision = true,
                    tracerEffectPrefab = tracerPrefab,
                    stopperMask = LayerIndex.entityPrecise.mask, // pierce walls but not enemies
                };
                bulletAttack.Fire();


                if (shotsRemaining == 1)
                {
                    outer.SetNextState(new FadeOut());
                }
            }
        }
    }

    public class FadeOut : BaseState
    {
        public static float duration = 0.67f;

        private static float printTime = 0.67f;
        private static float printStartHeight = -0.5f;
        private static float printMaxHeight = -2.1f;

        private static float zuh = 0.7f;
        public override void OnEnter()
        {
            base.OnEnter();

            Transform modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                var printController = modelTransform.GetComponent<PrintController>();
                if (printController)
                {
                    printController.enabled = false;
                    printController.printTime = printTime;
                    printController.startingPrintHeight = printStartHeight;
                    printController.maxPrintHeight = printMaxHeight;
                    printController.enabled = true;
                }
                var animateShaderAlpha = modelTransform.GetComponent<AnimateShaderAlpha>();
                if (animateShaderAlpha)
                {
                    animateShaderAlpha.timeMax = printTime * zuh;
                    animateShaderAlpha.enabled = true;
                }
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (NetworkServer.active && fixedAge >= duration)
            {
                Destroy(base.gameObject);
            }
        }
    }
}
