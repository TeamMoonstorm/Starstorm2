using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.LampBoss
{
    public class SpawnBigLamp : BaseSkillState
    {
        private static BodyIndex bigLamp = BodyIndex.None;
        public static GameObject muzzleEffectPrefab;
        public static GameObject bodyPrefab;
        private static float duration = 30f;
        private static string enterSoundString = "Play_grandparent_attack3_sun_spawn";
        private static float walkSpeedCoefficient = 0.25f;

        private CharacterBody bigLampInstanceBody;
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
                bigLampInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject, "LanternL");
            }
            Util.PlaySound(enterSoundString, gameObject);
            CharacterBody.onBodyDestroyGlobal += OnBodyDestroyGlobal;
        }

        private void OnBodyDestroyGlobal(CharacterBody body)
        {
            if (isAuthority && body.bodyIndex == bigLamp && body.TryGetComponent(out NetworkedBodyAttachment attachment) && attachment.Network_attachedBodyObject == gameObject)
            {
                outer.SetNextState(new OwieMyLamp());
            }    
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
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

    public class OwieMyLamp : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();

            Chat.AddMessage("owwww ow ow owie owww");
            outer.SetNextStateToMain();
        }
    }
}
