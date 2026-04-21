using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2
{
    public class BlitzRunBehavior : MonoBehaviour
    {
        private void OnEnable()
        {
           // On.RoR2.Run.HandlePlayerFirstEntryAnimation += OnHandlePlayerFirstEntryAnimation;
            Run.onRunStartGlobal += OnRunStart;
        }

        private void OnDisable()
        {
           // On.RoR2.Run.HandlePlayerFirstEntryAnimation -= OnHandlePlayerFirstEntryAnimation;
            Run.onRunStartGlobal -= OnRunStart;
        }

        // private void OnHandlePlayerFirstEntryAnimation(
        //     On.RoR2.Run.orig_HandlePlayerFirstEntryAnimation orig,
        //     Run self, CharacterBody body, Vector3 spawnPosition, Quaternion spawnRotation)
        // {
        //     if (self.gameObject == gameObject)
        //     {
        //         // No survivor pod — players teleport in directly
        //         body.SetBodyStateToPreferredInitialState();
        //         return;
        //     }
        //     orig(self, body, spawnPosition, spawnRotation);
        // }

        private void OnRunStart(Run run)
        {
            if (run.gameObject != gameObject)
                return;

            if (NetworkServer.active)
            {
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "SS2_GAMEMODE_BLITZ_START"
                });
            }
        }
    }
}