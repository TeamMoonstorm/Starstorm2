using RoR2;
using UnityEngine.Networking;

namespace EntityStates.Generic
{
    public class NemesisSpawnState : BaseState
    {
        public static float duration = 3f;
        protected string portalMuzzle = "Chest";

        private CameraRigController cameraController;
        private bool initCamera;

        public override void OnEnter()
        {
            base.OnEnter();
            initCamera = false;
            PlayAnimation("FullBody, Override", "Spawn");
            Util.PlaySound(EntityStates.NullifierMonster.SpawnState.spawnSoundString, gameObject);

            if (NetworkServer.active) characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            SpawnEffect();
        }

        public virtual void SpawnEffect()
        {
            if (EntityStates.NullifierMonster.SpawnState.spawnEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(EntityStates.NullifierMonster.SpawnState.spawnEffectPrefab, gameObject, portalMuzzle, false);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            // i don't know if all this null checking is necessary but i'd rather play it safe than spend time testing
            if (!cameraController)
            {
                if (characterBody && characterBody.master)
                {
                    if (characterBody.master.playerCharacterMasterController)
                    {
                        if (characterBody.master.playerCharacterMasterController.networkUser)
                        {
                            cameraController = characterBody.master.playerCharacterMasterController.networkUser.cameraRigController;
                        }
                    }
                }
            }
            else
            {
                if (!initCamera)
                {
                    initCamera = true;
                    //SetPitchYawFromLookVector was removed, idk why.
                    //cameraController.SetPitchYawFromLookVector(-characterDirection.forward);
                }
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (NetworkServer.active) characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}