//using RoR2;
//using RoR2.Achievements;
//using UnityEngine.SceneManagement;
//using UnityEngine;
//namespace Moonstorm.Starstorm2.Unlocks.Chirr
//{

//    public sealed class ChirrBeastmasterUnlockable : UnlockableBase
//    {
//        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.survivor.beastmaster", SS2Bundle.Chirr);

//        public sealed class ChirrBeastmasterAchievement : BaseAchievement
//        {

//            public override void OnInstall()
//            {
//                base.OnInstall();
//                base.SetServerTracked(true);
//            }
//            public override BodyIndex LookUpRequiredBodyIndex()
//            {
//                return BodyCatalog.FindBodyIndex("ChirrBody");
//            }
//            private class ChirrBeastmasterServerAchievement : BaseServerAchievement
//            {
//                private bool throwAllyComplete;
//                private bool scrapItemsComplete;
//                private bool dieComplete;
//                public override void OnInstall()
//                {
//                    base.OnInstall();
//                    GlobalEventManager.onCharacterDeathGlobal += CheckChirrDeath;
//                    On.RoR2.MapZone.TryZoneStart += CheckAlly;
//                }

//                private void CheckAlly(On.RoR2.MapZone.orig_TryZoneStart orig, MapZone self, Collider other)
//                {
//                    orig(self, other);
//                    if(!throwAllyComplete)
//                    {
//                        CharacterBody body = other.GetComponent<CharacterBody>();
//                    }
//                }

//                private void CheckChirrDeath(DamageReport damageReport)
//                {
//                    if (damageReport.attackerTeamIndex == TeamIndex.Player && damageReport.victimIsElite && damageReport.victimBody.HasBuff(SS2Content.Buffs.bdEmpyrean))
//                    {
//                        this.Grant();
//                    }
//                }

//                public override void OnUninstall()
//                {
//                    base.OnUninstall();
//                    GlobalEventManager.onCharacterDeathGlobal -= CheckChirrDeath;
//                }
//            }
//        }

//    }
//}