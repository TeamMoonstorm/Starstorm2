using RoR2;
using UnityEngine;
using Path = System.IO.Path;
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
