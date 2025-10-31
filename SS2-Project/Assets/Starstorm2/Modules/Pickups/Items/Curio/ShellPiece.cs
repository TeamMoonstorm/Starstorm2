using RoR2;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections.Generic;

namespace SS2.Items
{
    public sealed class ShellPiece : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acShellPiece", SS2Bundle.Items);
        public override bool IsAvailable(ContentPack contentPack) => SS2Config.enableBeta;
        public override void Initialize()
        {
            On.RoR2.CharacterMaster.OnServerStageBegin += OnServerStageBegin;
        }

        // restore consumed shells
        private void OnServerStageBegin(On.RoR2.CharacterMaster.orig_OnServerStageBegin orig, CharacterMaster self, Stage stage)
        {
            orig(self, stage);
            int itemCount = self.inventory.GetItemCount(SS2Content.Items.ShellPieceConsumed);
            if (itemCount > 0)
            {
                self.inventory.RemoveItem(SS2Content.Items.ShellPieceConsumed, itemCount);
                self.inventory.GiveItem(SS2Content.Items.ShellPiece, itemCount);
                CharacterMasterNotificationQueue.SendTransformNotification(self, SS2Content.Items.ShellPieceConsumed.itemIndex, SS2Content.Items.ShellPiece.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
            }
        }
    }
}
