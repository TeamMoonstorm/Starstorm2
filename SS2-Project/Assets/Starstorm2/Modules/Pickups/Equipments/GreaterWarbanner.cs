using RoR2;
using RoR2.Audio;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using MSU;
using RoR2.ContentManagement;
using MSU.Config;

namespace SS2.Equipments
{
    public class GreaterWarbanner : SS2Equipment
    {
        private const string token = "SS2_EQUIP_GREATERWARBANNER_DESC";

        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<EquipmentAssetCollection>("acGreaterWarbanner", SS2Bundle.Equipments);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of Extra Regeneration. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float extraRegeneration = 0.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of Extra Crit Chance. (100 = 100%)")]
        [FormatToken(token, 1)]
        public static float extraCrit = 20f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of Cooldown Reduction. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float cooldownReduction = 0.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Max active warbanners for each character.")]
        [FormatToken(token, 3)]
        public static int maxGreaterBanners = 1;

        private GameObject _warbannerObject;

        public override bool Execute(EquipmentSlot slot)
        {
            var GBToken = slot.characterBody.gameObject.GetComponent<GreaterBannerToken>();
            if (!GBToken)
            {
                slot.characterBody.gameObject.AddComponent<GreaterBannerToken>();
                GBToken = slot.characterBody.gameObject.GetComponent<GreaterBannerToken>();
            }
            //To do: make better placement system
            Vector3 position = slot.inputBank.aimOrigin - (slot.inputBank.aimDirection);
            GameObject bannerObject = UnityEngine.Object.Instantiate(_warbannerObject, position, Quaternion.identity);

            bannerObject.GetComponent<TeamFilter>().teamIndex = slot.teamComponent.teamIndex;
            NetworkServer.Spawn(bannerObject);

            if (GBToken.soundCooldown >= 5f)
            {
                var sound = NetworkSoundEventCatalog.FindNetworkSoundEventIndex("GreaterWarbanner");
                EffectManager.SimpleSoundEffect(sound, bannerObject.transform.position, true);
                GBToken.soundCooldown = 0f;
            }

            GBToken.ownedBanners.Add(bannerObject);

            if (GBToken.ownedBanners.Count > maxGreaterBanners)
            {
                var oldBanner = GBToken.ownedBanners[0];
                GBToken.ownedBanners.RemoveAt(0);
                EffectData effectData = new EffectData
                {
                    origin = oldBanner.transform.position
                };
                effectData.SetNetworkedObjectReference(oldBanner);
                EffectManager.SpawnEffect(HealthComponent.AssetReferences.executeEffectPrefab, effectData, transmit: true);

                UnityEngine.Object.Destroy(oldBanner);
                NetworkServer.Destroy(oldBanner);
            }

            return true;
        }

        public override void Initialize()
        {
            _warbannerObject = AssetCollection.FindAsset<GameObject>("GreaterWarbannerWard");
            RegisterTempVisualEffects();
            On.RoR2.GenericSkill.RunRecharge += FasterTickrateBannerHook;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        private void FasterTickrateBannerHook(On.RoR2.GenericSkill.orig_RunRecharge orig, GenericSkill self, float dt)
        {
            if (self)
            {
                if (self.characterBody)
                {
                    if (self.characterBody.HasBuff(SS2Content.Buffs.BuffGreaterBanner))
                    {
                        dt *= (1f + (2f * cooldownReduction));
                    }
                }
            }
            orig(self, dt);
        }

        private void RegisterTempVisualEffects()
        {
            // TODO: MSU 2.0
            /*var effectInstance = SS2Assets.LoadAsset<GameObject>("GreaterBannerBuffEffect", SS2Bundle.Equipments); 

            TempVisualEffectAPI.AddTemporaryVisualEffect(effectInstance.InstantiateClone("GreaterBannerBuffEffect", false), (CharacterBody body) => { return body.HasBuff(SS2Content.Buffs.BuffGreaterBanner); }, true, "MainHurtbox");*/
        }

        public class GreaterBannerToken : MonoBehaviour
        {
            //public GameObject[] ownedBanners = new GameObject[0];
            public List<GameObject> ownedBanners = new List<GameObject>(0);

            public float soundCooldown = 5f;

            private void FixedUpdate()
            {
                soundCooldown += Time.fixedDeltaTime;
            }
        }

        // TODO: Replace class with a single hook on RecalculateSTatsAPI.GetstatCoefficients. This way we replace the monobehaviour with just a method
        public sealed class GreatBannerBuffBehavior : BaseBuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffGreaterBanner;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (characterBody.HasBuff(SS2Content.Buffs.BuffGreaterBanner))
                {
                    args.critAdd += GreaterWarbanner.extraCrit;
                    args.regenMultAdd += GreaterWarbanner.extraRegeneration;
                }
            }
        }
    }
}
