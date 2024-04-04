using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SS2.Items
{
    public sealed class Diary : SS2Item, IContentPackModifier
    {
        private const string token = "SS2_ITEM_DIARY_DESC";

        public override NullableRef<GameObject> ItemDisplayPrefab => null;

        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;
        private static ItemDef _consumedItemDef;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Number of levels gained when Empty Diary is consumed.")]
        [FormatToken(token)]
        public static int extraLevels = 3;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            ParallelAssetLoadCoroutineHelper helper = new ParallelAssetLoadCoroutineHelper();

            helper.AddAssetToLoad<ItemDef>("Diary", SS2Bundle.Items);
            helper.AddAssetToLoad<ItemDef>("DiaryConsumed", SS2Bundle.Items);

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            _itemDef = helper.GetLoadedAsset<ItemDef>("Diary");
            _consumedItemDef = helper.GetLoadedAsset<ItemDef>("DiaryConsumed");
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.itemDefs.AddSingle(_consumedItemDef);
        }

        //I have a hunch this could be done without a body behaviour, cant bother with it rn tho. -N
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
                for (int i = 0; i < uICameras.Count; i++)
                {
                    UICamera uICamera = uICameras[i];
                    CameraRigController cameraRigController = uICamera.cameraRigController;
                    if (cameraRigController && cameraRigController.viewer && cameraRigController.viewer.hasAuthority && cameraRigController.target == holderBodyobject)
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

            private void AddLevel(CharacterBody body)
            {
                ItemIndex diary = SS2Content.Items.Diary.itemIndex;
                ItemIndex consumed = SS2Content.Items.DiaryConsumed.itemIndex;
                if (body.inventory.GetItemCount(diary) > 0)
                {
                    // this uses xp orbs. wouldnt mind setting the level manually if we use our own vfx
                    ulong requiredExperience = TeamManager.GetExperienceForLevel(TeamManager.instance.GetTeamLevel(body.teamComponent.teamIndex) + (uint)extraLevels);
                    ulong experience = requiredExperience - TeamManager.instance.GetTeamCurrentLevelExperience(body.teamComponent.teamIndex);
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
