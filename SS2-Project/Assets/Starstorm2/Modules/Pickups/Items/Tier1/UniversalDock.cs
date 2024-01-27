using Moonstorm.Starstorm2.Buffs;
using RoR2;
using RoR2.Items;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class UniversalDock : ItemBase
    {
        private const string token = "SS2_ITEM_UNIVERSALDOCK_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("UniversalDock", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Time it takes for Universal Dock to recharge, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float baseCooldown = 15f;

        [RooConfigurableField(SS2Config.IDItem, ConfigName = "How much faster Universal Dock recharges, per stack. (1 = 100%)")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float cooldownReductionPerStack = 20f; // percent

        public static GameObject overlayPanel = SS2Assets.LoadAsset<GameObject>("RefreshPanel", SS2Bundle.Items);


        public override void Initialize()
        {
            On.RoR2.UI.HUD.Awake += AddPanels;
        }

        private void AddPanels(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);

            Transform scaler = self.transform.Find("MainContainer/MainUIArea/SpringCanvas/BottomRightCluster/Scaler");
            Transform skill1 = scaler.Find("Skill1Root");
            if (skill1) GameObject.Instantiate(overlayPanel, skill1);
            Transform skill2 = scaler.Find("Skill2Root");
            if (skill2) GameObject.Instantiate(overlayPanel, skill2);
            Transform skill3 = scaler.Find("Skill3Root");
            if (skill3) GameObject.Instantiate(overlayPanel, skill3);
            Transform skill4 = scaler.Find("Skill4Root");
            if (true) GameObject.Instantiate(overlayPanel, skill4);
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.UniversalDock;

            private float cooldownTimer;
            private bool refreshReady;

            private void Start()
            {
                body.onSkillActivatedAuthority += TryRefresh;
            }

            private void TryRefresh(GenericSkill genericSkill)
            {
                if(this.cooldownTimer <= 0)
                {
                    genericSkill.ApplyAmmoPack();
                    float cooldownReduction = cooldownReductionPerStack * (stack - 1);
                    this.cooldownTimer = baseCooldown - Util.ConvertAmplificationPercentageIntoReductionPercentage(cooldownReduction);
                }
            }

            private void FixedUpdate()
            {
                this.cooldownTimer -= Time.fixedDeltaTime;
            }


        }
    }
}
