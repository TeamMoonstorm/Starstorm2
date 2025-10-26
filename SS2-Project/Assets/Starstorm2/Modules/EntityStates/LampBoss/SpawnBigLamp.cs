using RoR2;
using SS2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.LampBoss
{
    public class PrepSpawnBigLamp : BaseSkillState
    {
        public static GameObject muzzleEffectPrefab;
        private static string muzzleName = "LanternL";
        private static float baseDuration = 1.5f;
        private static string enterSoundString = "WayfarerRaiseBigLamp";
        private static float walkSpeedCoefficient = 0.25f;

        private float duration;
        private GameObject effectInstance;
        private Transform muzzle;
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;

            PlayAnimation("FullBody, Override", "HoldLamp");//, "Primary.playbackRate", duration);
            characterMotor.walkSpeedPenaltyCoefficient = walkSpeedCoefficient;

            muzzle = FindModelChild(muzzleName);
            if (muzzleEffectPrefab && muzzle)
            {
                effectInstance = GameObject.Instantiate(muzzleEffectPrefab, muzzle.transform.position, Quaternion.identity);
            }

            Util.PlaySound(enterSoundString, gameObject);
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextState(new SpawnBigLamp());
            }
        }
        public override void Update()
        {
            base.Update();
            if (effectInstance)
            {
                effectInstance.transform.position = muzzle.transform.position;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            characterMotor.walkSpeedPenaltyCoefficient = 1f;
        }
    }
    public class SpawnBigLamp : BaseSkillState
    {
        private static BodyIndex bigLamp = BodyIndex.None;
        public static GameObject muzzleEffectPrefab;
        public static GameObject bodyPrefab;
        private static float duration = 30f;
        private static string enterSoundString = "WayfarerStartBigLamp";
        private static string startLoopSoundString = "Play_WayfarerBigLampLoop"; // TODO: PUT THIS ON THE BIG LAMP DUMB FUCK!!!!!!!!!!!!!!!
        private static string stopLoopSoundString = "Stop_WayfarerBigLampLoop";
        private static float walkSpeedCoefficient = 0.25f;

        private CharacterBody bigLampInstanceBody;
        private bool bigLampResolved;
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }

        public override void OnEnter()
        {
            if (bigLamp == BodyIndex.None)
            {
                bigLamp = BodyCatalog.FindBodyIndex(bodyPrefab); // TODO: FREAKING!! BODY TYPE FIEL!!!!!!!!!!!!D
            }

            base.OnEnter();
            PlayAnimation("FullBody, Override", "HoldLamp", "Primary.playbackRate", duration);
            characterMotor.walkSpeedPenaltyCoefficient = walkSpeedCoefficient;

            if (muzzleEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, gameObject, "LanternL", false);
            }
            if (NetworkServer.active)
            {
                GameObject bigLampInstance = GameObject.Instantiate(bodyPrefab);
                bigLampInstanceBody = bigLampInstance.GetComponent<CharacterBody>();
                bigLampInstanceBody.teamComponent.teamIndex = teamComponent.teamIndex;
                bigLampResolved = true;
                bigLampInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject, "LanternL");
            }
            if (isAuthority)
            {
                CharacterBody.onBodyStartGlobal += ResolveLampInstance;
                CharacterBody.onBodyDestroyGlobal += OnBodyDestroyGlobal;
            }
            Util.PlaySound(enterSoundString, gameObject);
            
        }

        private void ResolveLampInstance(CharacterBody body)
        {
            if (bigLampResolved)
            {
                return;
            }
            if (body.bodyIndex == bigLamp && body.TryGetComponent(out NetworkedBodyAttachment attachment) && attachment.Network_attachedBodyObject == gameObject)
            {
                bigLampResolved = true;
                bigLampInstanceBody = body;
            }
        }

        private void OnBodyDestroyGlobal(CharacterBody body)
        {
            if (body.bodyIndex == bigLamp && body.TryGetComponent(out NetworkedBodyAttachment attachment) && attachment.Network_attachedBodyObject == gameObject)
            {
                outer.SetNextState(new OwieMyLamp());
            }    
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority)
            {
                FixedUpdateAuthority();
            }
        }
        private void FixedUpdateAuthority()
        {
            if (bigLampResolved)
            {
                if (!bigLampInstanceBody || !bigLampInstanceBody.healthComponent.alive)
                {
                    outer.SetNextState(new OwieMyLamp());
                    return;
                }
            }
            if (fixedAge >= duration)
            {
                outer.SetNextState(new LowerBigLamp());
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            Util.PlaySound(stopLoopSoundString, gameObject);
            CharacterBody.onBodyStartGlobal -= ResolveLampInstance;
            CharacterBody.onBodyDestroyGlobal -= OnBodyDestroyGlobal;
            
            characterMotor.walkSpeedPenaltyCoefficient = 1f;
            if (NetworkServer.active)
            {
                if (bigLampInstanceBody)
                {
                    bigLampInstanceBody.healthComponent.Suicide();
                }
            }
        }
    }

    public class LowerBigLamp : BaseState
    {
        private static string enterSoundString = "WayfarerEndBigLamp";
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(enterSoundString, gameObject);
            outer.SetNextStateToMain();
        }
    }

    public class OwieMyLamp : BaseState
    {
        private static float duration = 3f;
        private static string enterSoundString;// = "WayfarerBigLampDeath";
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(enterSoundString, gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}
