using RoR2;
using RoR2.Audio;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Equipments
{
    [DisabledContent]
    public sealed class WhiteFlag : EquipmentBase
    {
        private const string token = "SS2_EQUIP_GREATERWARBANNER_DESC";
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("WhiteFlag", SS2Bundle.Indev);
        public GameObject WarbannerObject { get; } = SS2Assets.LoadAsset<GameObject>("WhiteFlagWard", SS2Bundle.Indev);

        /*[ConfigurableField(SS2Config.IDItem, ConfigDesc = "Amount of Extra Regeneration. (1 = 100%)")]
        [TokenModifier(token, StatTypes.Percentage, 0)]
        public static float extraRegeneration = 0.5f;

        [ConfigurableField(SS2Config.IDItem, ConfigDesc = "Amount of Extra Crit Chance. (100 = 100%)")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float extraCrit = 20f;

        [ConfigurableField(SS2Config.IDItem, ConfigDesc = "Amount of Cooldown Reduction. (1 = 100%)")]
        [TokenModifier(token, StatTypes.Percentage, 2)]
        public static float cooldownReduction = 0.5f;*/

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<GreaterWarbannerBehavior>(stack);
        }

        public override bool FireAction(EquipmentSlot slot)
        {
            if (!slot.characterBody.characterMotor.isGrounded)
                return false;
            //To do: make better placement system
            Vector3 position = slot.inputBank.aimOrigin + slot.inputBank.aimDirection;
            GameObject gameObject = Object.Instantiate(WarbannerObject, position, Quaternion.identity);

            //gameObject.GetComponent<TeamFilter>().teamIndex = slot.teamComponent.teamIndex;
            var behavior = slot.gameObject.GetComponent<GreaterWarbannerBehavior>();
            //if (behavior.warBannerInstance)
            //NetworkServer.Destroy(behavior.warBannerInstance);
            behavior.warBannerInstance = gameObject;
            NetworkServer.Spawn(behavior.warBannerInstance);

            if (behavior.soundCooldown >= 5f)
            {
                var sound = NetworkSoundEventCatalog.FindNetworkSoundEventIndex("GreaterWarbanner");
                EffectManager.SimpleSoundEffect(sound, behavior.warBannerInstance.transform.position, true);
                behavior.soundCooldown = 0f;
            }
            return true;
        }

        public sealed class GreaterWarbannerBehavior : CharacterBody.ItemBehavior
        {
            public GameObject warBannerInstance;
            public float soundCooldown = 5f;

            private void FixedUpdate()
            {
                soundCooldown += Time.fixedDeltaTime;
            }

            private void OnDisable()
            {
                //if (warBannerInstance)
                //Destroy(warBannerInstance);
            }
        }
    }

}
