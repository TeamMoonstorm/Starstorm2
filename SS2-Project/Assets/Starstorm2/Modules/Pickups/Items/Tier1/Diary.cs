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
    public sealed class Diary : ItemBase
    {
        private const string token = "SS2_ITEM_DIARY_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("Diary", SS2Bundle.Items);

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
            private void Awake()
            {
                Stage.onServerStageBegin += PrepareAddLevel;
            }
            private void OnDestroy()
            {
                Stage.onServerStageBegin -= PrepareAddLevel;
            }
            private void PrepareAddLevel(Stage stage)
            {
                if (stage.sceneDef)
                    base.GetComponent<CharacterMaster>().onBodyStart += AddLevel;
            }

            [Server]
            private void AddLevel(CharacterBody body)
            {
                ItemIndex diary = SS2Content.Items.Diary.itemIndex;
                ItemIndex consumed = SS2Content.Items.DiaryConsumed.itemIndex;
                if (body.inventory.GetItemCount(diary) > 0)
                {
                    // this uses xp orbs. wouldnt mind setting the level manually if we use our own vfx
                    ulong experience = TeamManager.instance.GetTeamNextLevelExperience(body.teamComponent.teamIndex) - TeamManager.instance.GetTeamCurrentLevelExperience(body.teamComponent.teamIndex);
                    ExperienceManager.instance.AwardExperience(body.transform.position, body, experience);

                    body.inventory.RemoveItem(diary);
                    body.inventory.GiveItem(consumed);
                    CharacterMasterNotificationQueue.SendTransformNotification(body.master, diary, consumed, CharacterMasterNotificationQueue.TransformationType.Default);
                    PlayDiarySFXLocal(body.gameObject);   // vfx would be nice             
                }

                body.master.onBodyStart -= AddLevel;
            }

        }
    }
}
