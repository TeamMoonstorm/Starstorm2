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
using RoR2.ContentManagement;

namespace SS2.Survivors
{
    public sealed class NemCommando : SS2Survivor, IContentPackModifier
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acNemCommando", SS2Bundle.NemCommando);

        private static float gougeDuration = 2;
        public static DamageAPI.ModdedDamageType GougeDamageType { get; private set; }
        public static DotController.DotIndex GougeDotIndex { get; private set; }
        private BuffDef _gougeBuffDef;
        private GameObject distantGashProjectile;
        private GameObject distantGashProjectileBlue;
        private GameObject distantGashProjectileYellow;
        private GameObject grenadeProjectile;

        public override void Initialize()
        {
            _gougeBuffDef = AssetCollection.FindAsset<BuffDef>("BuffGouge");
            distantGashProjectile = AssetCollection.FindAsset<GameObject>("NemCommandoSwordBeamProjectile");
            distantGashProjectileBlue = AssetCollection.FindAsset<GameObject>("NemCommandoSwordBeamProjectileBlue");
            distantGashProjectileYellow = AssetCollection.FindAsset<GameObject>("NemCommandoSwordBeamProjectileYellow");

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

        //N: I should make this an utility idk
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

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        

        private void ModifyProjectiles()
        {
            var damageAPIComponent = distantGashProjectile.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            damageAPIComponent.Add(GougeDamageType);
            damageAPIComponent = distantGashProjectileBlue.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            damageAPIComponent.Add(GougeDamageType);
            damageAPIComponent = distantGashProjectileYellow.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            damageAPIComponent.Add(GougeDamageType);

            var pie = grenadeProjectile.GetComponent<RoR2.Projectile.ProjectileImpactExplosion>();
            pie.impactEffect = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/OmniExplosionVFXCommandoGrenade.prefab").WaitForCompletion();
        }
    }
}
