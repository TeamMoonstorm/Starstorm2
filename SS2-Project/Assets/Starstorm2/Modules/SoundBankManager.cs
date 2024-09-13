using RoR2;
using UnityEngine;
using Path = System.IO.Path;
using MonoMod.RuntimeDetour;
using System;
using R2API.Utils;
namespace SS2
{
    public static class SoundBankManager
    {
        public static string soundBankDirectory
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(SS2Main.Instance.Info.Location), "soundbanks");
            }
        }

        public static void Init()
        {
            var hook = new Hook(
            typeof(AkSoundEngineInitialization).GetMethodCached(nameof(AkSoundEngineInitialization.InitializeSoundEngine)),
            typeof(SoundBankManager).GetMethodCached(nameof(AddBanks)));

            
        }

        private static bool AddBanks(Func<AkSoundEngineInitialization, bool> orig, AkSoundEngineInitialization self)
        {
            var res = orig(self);

            LoadBanks();

            return res;
        }

        private static void LoadBanks()
        {
            //LogCore.LogE(AkSoundEngine.ClearBanks().ToString());
            AkSoundEngine.AddBasePath(soundBankDirectory);
            AkSoundEngine.LoadFilePackage("Starstorm2.pck", out var packageID/*, -1*/);
            AkSoundEngine.LoadBank("Starstorm2", /*-1,*/ out var bank);
            AkSoundEngine.LoadBank("SS2Init", /*-1,*/ out var bitch);
        }

        [SystemInitializer(dependencies: typeof(MusicTrackCatalog))]
        public static void MusicInit()
        {
            AkSoundEngine.LoadBank("SS2Music", /*-1,*/ out var bank);
            GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("SS2MusicInitializer", SS2Bundle.Base));
        }
    }
}
