using R2API.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;
using RiskOfOptions.OptionConfigs;

using MSU;
using RoR2.ContentManagement;
using MSU.Config;
using UnityEngine;
using BepInEx;
using RoR2;

namespace SS2.Modules
{
    public sealed class Interactables : IContentPieceProvider<GameObject>
    {
        public ContentPack ContentPack => _contentPack;
        private ContentPack _contentPack;

        public static ConfiguredBool EnableInteractables = SS2Config.ConfigFactory.MakeConfiguredBool(true, (b) =>
        {
            b.Section = "Enable All Interactables";
            b.Key = "Enable All Interactables";
            b.Description = "Enables Starstorm 2's interactables. Set to false to disable interactables.";
            b.ConfigFile = SS2Config.ConfigMain;
            b.CheckBoxConfig = new CheckBoxConfig
            {
                restartRequired = true,
            };
        }).DoConfigure();

        private IEnumerable<IContentPiece<GameObject>> _unfilteredInteractables;

        public IContentPiece<GameObject>[] GetContents()
        {
            return _unfilteredInteractables.Where(PassesFilter).ToArray();
        }

        IContentPiece[] IContentPieceProvider.GetContents()
        {
            return _unfilteredInteractables.Where(PassesFilter).ToArray();
        }

        private bool PassesFilter(IContentPiece<GameObject> piece)
        {
            if (!EnableInteractables)
                return false;

            if(!(piece is SS2Interactable interactable))
            {
                return false;
            }

            if (!piece.IsAvailable(ContentPack))
                return false;

            return SS2Config.ConfigFactory.MakeConfiguredBool(true, (b) =>
            {
                b.Section = "Interactables";
                b.Key = MSUtil.NicifyString(interactable.InteractablePrefab.name);
                b.Description = "Enable/Disable this Interactable";
                b.ConfigFile = SS2Config.ConfigMain;
                b.CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !EnableInteractables,
                    restartRequired = true
                };
            }).DoConfigure();
        }

        internal Interactables(ContentPack contentPack, BaseUnityPlugin plugin)
        {
            _contentPack = contentPack;
            _unfilteredInteractables = ContentUtil.AnalyzeForGameObjectContentPieces<IInteractable>(plugin);
        }
    }
}