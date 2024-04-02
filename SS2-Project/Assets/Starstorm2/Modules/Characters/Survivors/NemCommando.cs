using RoR2;
using UnityEngine;
using RoR2.Skills;
using System.Runtime.CompilerServices;
using RoR2.UI;
using MSU;
using System.Collections;
using R2API;
using System.Reflection;
using UnityEngine.Networking;

namespace SS2.Survivors
{
    public sealed class NemCommando : SS2Survivor
    {
        public override SurvivorDef SurvivorDef => _survivorDef;
        private SurvivorDef _survivorDef;
        public override NullableRef<GameObject> MasterPrefab => _monsterMaster;
        private GameObject _monsterMaster;
        public override GameObject CharacterPrefab => _prefab;
        private GameObject _prefab;

        private static float gougeDuration = 2;
        public static DamageAPI.ModdedDamageType GougeDamageType { get; private set; }
        public static DotController.DotIndex GougeDotIndex { get; private set; }
        private BuffDef _gougeBuffDef;

        public override void Initialize()
        {
            On.RoR2.CharacterSelectBarController.Awake += CharacterSelectBarController_Awake;

            GougeDamageType = DamageAPI.ReserveDamageType();
            GougeDotIndex = DotAPI.RegisterDotDef(0.25f, 0.25f, DamageColorIndex.SuperBleed, _gougeBuffDef);
            On.RoR2.HealthComponent.TakeDamage += TakeDamageGouge;
            GlobalEventManager.onServerDamageDealt += ApplyGouge;
        }

        private void TakeDamageGouge(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            bool triggerGougeProc = false;
            if (NetworkServer.active)
            {
                if (damageInfo.dotIndex == GougeDotIndex && damageInfo.procCoefficient == 0f && self.alive)
                {
                    if (damageInfo.attacker)
                    {
                        CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                        if (attackerBody)
                        {
                            damageInfo.crit = Util.CheckRoll(attackerBody.crit, attackerBody.master);
                        }
                    }
                    damageInfo.procCoefficient = 0.2f;
                    triggerGougeProc = true;
                }
            }

            orig(self, damageInfo);

            if (NetworkServer.active && !damageInfo.rejected && self.alive)
            {
                if (triggerGougeProc)
                {
                    GlobalEventManager.instance.OnHitEnemy(damageInfo, self.gameObject);
                }
            }
        }

        private void CharacterSelectBarController_Awake(On.RoR2.CharacterSelectBarController.orig_Awake orig, CharacterSelectBarController self)
        {
            //hide nemcommando from css proper
            SS2Content.Survivors.survivorNemMerc.hidden = !SurvivorCatalog.SurvivorIsUnlockedOnThisClient(SS2Content.Survivors.survivorNemMerc.survivorIndex); // hello nem comado
            SS2Content.Survivors.survivorNemCommando.hidden = !SurvivorCatalog.SurvivorIsUnlockedOnThisClient(SS2Content.Survivors.survivorNemCommando.survivorIndex);
            orig(self);
        }

        private void ApplyGouge(DamageReport report)
        {
            var victimBody = report.victimBody;
            var attackerBody = report.attackerBody;
            var damageInfo = report.damageInfo;
            if (DamageAPI.HasModdedDamageType(damageInfo, GougeDamageType))
            {
                var dotInfo = new InflictDotInfo()
                {
                    attackerObject = attackerBody.gameObject,
                    victimObject = victimBody.gameObject,
                    dotIndex = GougeDotIndex,
                    duration = gougeDuration,
                    damageMultiplier = 1,
                };
                DotController.InflictDot(ref dotInfo);
            }
        }

        public override bool IsAvailable()
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            ParallelAssetLoadCoroutineHelper helper = new ParallelAssetLoadCoroutineHelper();
            helper.AddAssetToLoad<GameObject>("NemCommandoBody", SS2Bundle.NemCommando);
            helper.AddAssetToLoad<GameObject>("NemCommandoMonsterMaster", SS2Bundle.NemCommando);
            helper.AddAssetToLoad<SurvivorDef>("survivorNemCommando", SS2Bundle.NemCommando);
            helper.AddAssetToLoad<BuffDef>("BuffGouge", SS2Bundle.NemCommando);

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            _survivorDef = helper.GetLoadedAsset<SurvivorDef>("survivorNemCommando");
            _monsterMaster = helper.GetLoadedAsset<GameObject>("NemCommandoMonsterMaster");
            _prefab = helper.GetLoadedAsset<GameObject>("NemCommandoBody");
            _gougeBuffDef = helper.GetLoadedAsset<BuffDef>("BuffGouge");
        }
    }

    public class NemmandoPistolToken : MonoBehaviour
    {
        public int secondaryStocks = 8;
    }
}
