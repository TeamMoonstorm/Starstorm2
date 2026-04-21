using RoR2;
using UnityEngine.Networking;

namespace SS2
{
    public class BlitzRun : Run
    {
        public override void Start()
        {
            base.Start();

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
