using RoR2;
using UnityEngine;

namespace EntityStates.ClayMonger
{
    public class SpawnState : BaseState
    {
        public static float duration;
        public static string spawnSoundString;

        [Header("Anim Params")]
        public static string animLayerName;
        public static string spawnAnimName;
        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation(animLayerName, spawnAnimName);
            Util.PlaySound(spawnSoundString, gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(fixedAge > duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}