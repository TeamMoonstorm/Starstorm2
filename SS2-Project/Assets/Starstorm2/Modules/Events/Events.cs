using SS2.ScriptableObjects;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using RiskOfOptions.OptionConfigs;
using Object = UnityEngine.Object;
using MSU.Config;
using MSU;
using System.Collections;
using System.Collections.Generic;
using RoR2;
using RoR2.ContentManagement;
using System.Linq;
namespace SS2
{
    public sealed class Events : IContentPieceProvider<GameObject>
    {
        public static ConfiguredBool EnableEvents = SS2Config.ConfigFactory.MakeConfiguredBool(true, b =>
        {
            b.section = "Storms";
            b.key = "Enable Storms";
            b.description = "Enables Starstorm 2's storms. Set to false to disable storms.";
            b.configFile = SS2Config.ConfigEvent;
            b.checkBoxConfig = new CheckBoxConfig
            {
                restartRequired = true
            };
        }).DoConfigure();

        public ContentPack contentPack => _contentPack;
        private ContentPack _contentPack;

        private IEnumerable<IContentPiece<GameObject>> _unfilteredCharacters;

        public IContentPiece<GameObject>[] GetContents()
        {
            return _unfilteredCharacters.Where(PassesFilter).ToArray();
        }

        IContentPiece[] IContentPieceProvider.GetContents()
        {
            return _unfilteredCharacters.Where(PassesFilter).ToArray();
        }

        private bool PassesFilter(IContentPiece<GameObject> _contentPiece)
        {
            if (!(_contentPiece is IGameObjectContentPiece<GameplayEvent> item))
            {
                return false;
            }

            if (!item.IsAvailable(contentPack))
                return false;

            return true;
        }

        public static IEnumerator Init()
        {
            SS2Config.ConfigFactory.MakeConfiguredBool(true, b =>
            {
                b.section = "Visuals";
                b.key = "Custom Main Menu";
                b.description = "Setting this to false returns the main menu to the original, bright one.";
                b.configFile = SS2Config.ConfigMisc;
                b.onConfigChanged += b1 =>
                {
                    if (b1)
                    {
                        SceneManager.sceneLoaded -= StormOnMenu;
                        SceneManager.sceneLoaded += StormOnMenu;
                    }
                    else
                    {
                        SceneManager.sceneLoaded -= StormOnMenu;
                    }
                };
            }).DoConfigure();

            yield return null;
        }

        private static void StormOnMenu(Scene scene, LoadSceneMode mode)
        {
            if (scene.name.Equals("title"))
            {
                DateTime today = DateTime.Today;
                if ((today.Month == 12) && ((today.Day == 27) || (today.Day == 26) || (today.Day == 25) || (today.Day == 24)|| (today.Day == 23)))
                {
                    Object.Instantiate(SS2Assets.LoadAsset<GameObject>("ChristmasMenuEffect", SS2Bundle.Events), Vector3.zero, Quaternion.identity);
                    Debug.Log("Merry Christmas from TeamMoonstorm!! :)");
                }
                else
                {
                    Object.Instantiate(SS2Assets.LoadAsset<GameObject>("StormMainMenuEffect", SS2Bundle.Events), Vector3.zero, Quaternion.identity);
                }
            }   
        }

        
    }
}
