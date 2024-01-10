using EntityStates;
using UnityEngine;
using static Moonstorm.Starstorm2.Equipments.GreaterWarbanner;
using UnityEngine.Networking;
using RoR2;
using RoR2.Audio;
using Moonstorm.Starstorm2;
using RoR2.Skills;

namespace Assets.Starstorm2.Modules.EntityStates.Knight.BuffedSkills
{
    public class BannerSlam : BaseState
    {
        public static int duration;
        public static int swingTimeCoefficient;
        public static SkillDef originalSkillRef;

        public EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("GreaterWarbanner", SS2Bundle.Equipments);
        public GameObject WarbannerObject { get; set; } = SS2Assets.LoadAsset<GameObject>("GreaterWarbannerWard", SS2Bundle.Equipments);


        public override void OnEnter()
        {
            Debug.Log("DEBUGGER The buffed banner slam was entered!!");

            PlayCrossfade("Body", "SwingSpecial", "Special.playbackRate", duration * swingTimeCoefficient, 0.15f);

            var GBToken = characterBody.gameObject.GetComponent<GreaterBannerToken>();
            if (!GBToken)
            {
                characterBody.gameObject.AddComponent<GreaterBannerToken>();
                GBToken = characterBody.gameObject.GetComponent<GreaterBannerToken>();
            }

            Vector3 position = inputBank.aimOrigin - (inputBank.aimDirection);
            GameObject bannerObject = UnityEngine.Object.Instantiate(WarbannerObject, position, Quaternion.identity);

            bannerObject.GetComponent<TeamFilter>().teamIndex = teamComponent.teamIndex;
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
                //SS2Log.Debug("Removing oldest Warbanner");
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
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            Debug.Log("DEBUGGER The buffed banner was exited!!");
            outer.SetNextStateToMain();
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }

}