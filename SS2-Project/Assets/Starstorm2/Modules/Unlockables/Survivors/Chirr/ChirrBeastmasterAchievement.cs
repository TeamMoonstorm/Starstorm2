using RoR2;
using RoR2.Achievements;
using UnityEngine.SceneManagement;
using UnityEngine;
using MSU;
namespace SS2.Unlocks.Chirr
{
    public sealed class ChirrBeastmasterAchievement : BaseAchievement
    {
        public override void OnInstall()
        {
            base.OnInstall();
            base.SetServerTracked(true);
        }
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("ChirrBody");
        }
        private class ChirrBeastmasterServerAchievement : BaseServerAchievement
        {
            private bool throwAllyComplete;
            private bool scrapItemsComplete;
            private bool dieComplete;
            //public override void OnInstall()
            //{
            //    base.OnInstall();
            //    GlobalEventManager.onCharacterDeathGlobal += CheckChirrDeath;
            //    On.RoR2.MapZone.TryZoneStart += CheckAlly;
            //}

            //private void CheckAlly(On.RoR2.MapZone.orig_TryZoneStart orig, MapZone self, Collider other)
            //{
            //    orig(self, other);
            //    if (!throwAllyComplete)
            //    {
            //        CharacterBody body = other.GetComponent<CharacterBody>();
            //    }
            //}

            //private void CheckChirrDeath(DamageReport damageReport)
            //{

            //}

            //public override void OnUninstall()
            //{
            //    base.OnUninstall();
            //    GlobalEventManager.onCharacterDeathGlobal -= CheckChirrDeath;
            //}
        }
    }   
}