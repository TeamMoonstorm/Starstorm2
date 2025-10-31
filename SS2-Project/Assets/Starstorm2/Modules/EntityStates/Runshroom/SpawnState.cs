using System;
using RoR2;
using UnityEngine;

namespace EntityStates.Runshroom
{
    public class SpawnState : BaseState
    {
        public static GameObject burrowPrefab;
        private static float duration = 2f;
        private static string spawnSoundString = "Play_runshroom_spawn";

        public override void OnEnter()
        {
            base.OnEnter();

            Util.PlaySound(spawnSoundString, gameObject);
            PlayAnimation("Body", "Spawn", "Spawn.playbackRate", duration);
            EffectManager.SimpleMuzzleFlash(burrowPrefab, gameObject, "BurrowCenter", false);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority && fixedAge >= duration)
            {
                this.outer.SetNextStateToMain();
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
        
    }
}