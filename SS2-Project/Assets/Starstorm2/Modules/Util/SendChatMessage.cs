using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    public class SendChatMessage : MonoBehaviour
    {
        public void SendMessage(string baskeToken, string message)
        {
            var asArray = new string[] { message };
            SendMessage(baskeToken, asArray);
        }

        public void SendMessage(string baseToken, string[] message)
        {
            var chatMessage = new Chat.SimpleChatMessage
            {
                baseToken = baseToken,
                paramTokens = message
            };
            Chat.SendBroadcastChat(chatMessage);
        }

    }
}