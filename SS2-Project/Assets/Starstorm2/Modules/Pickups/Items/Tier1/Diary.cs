using R2API.Networking.Interfaces;
using R2API.Networking;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    [DisabledContent]

    public sealed class Diary : ItemBase
    {
        private const string token = "SS2_ITEM_DIARY_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("Diary", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Experience bonus from each diary. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 0, "100")]
        public static float experienceBonus = 1;
        public override void Initialize()
        {
            base.Initialize();
            NetworkingAPI.RegisterMessageType<Behavior.DiarySFXMessage>();
        }
        public sealed class Behavior : BaseItemMasterBehavior
        {
            //plays the diary sfx only if players can see the experience bar being affected
            public static void PlayDiarySFXLocal(GameObject holderBodyobject)
            {
                if (!holderBodyobject)
                {
                    return;
                }
                List<UICamera> uICameras = UICamera.instancesList;
                for(int i = 0; i < uICameras.Count; i++)
                {
                    UICamera uICamera = uICameras[i];
                    CameraRigController cameraRigController = uICamera.cameraRigController;
                    if(cameraRigController && cameraRigController.viewer && cameraRigController.viewer.hasAuthority && cameraRigController.target == holderBodyobject)
                    {
                        Util.PlaySound("DiaryLevelUp", holderBodyobject);
                        return;
                    }
                }
            }
            [ItemDefAssociation(useOnClient = false, useOnServer = true)]
            private static ItemDef GetItemDef() => SS2Content.Items.Diary;
            private void OnEnable()
            {
                Stage.onStageStartGlobal += AddLevel;
            }

            private void AddLevel(Stage stage)
            {
                
            }

            private void OnDisable()
            {

            }
            private struct TimedExpOffset
            {
                public float awardTime;
                public long offset;
            }
            public class DiarySFXMessage : INetMessage
            {
                private NetworkInstanceId BodyObjectID;

                public DiarySFXMessage()
                {
                }

                public DiarySFXMessage(NetworkInstanceId bodyObjectID)
                {
                    BodyObjectID = bodyObjectID;
                }

                public void Serialize(NetworkWriter writer)
                {
                    writer.Write(BodyObjectID);
                }

                public void Deserialize(NetworkReader reader)
                {
                    BodyObjectID = reader.ReadNetworkId();
                }

                public void OnReceived()
                {
                    if (NetworkServer.active)
                    {
                        return;
                    }
                    GameObject gameObject = Util.FindNetworkObject(BodyObjectID);
                    if (gameObject)
                    {
                        PlayDiarySFXLocal(gameObject);
                    }
                }
            }
        }
    }
}
