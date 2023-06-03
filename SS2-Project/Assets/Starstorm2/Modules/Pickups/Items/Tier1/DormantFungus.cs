using Moonstorm.Starstorm2.Components;
using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    [DisabledContent]
    public sealed class DormantFungus : ItemBase
    {
        private const string token = "SS2_ITEM_DORMANTFUNGUS_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("DormantFungus", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Base amount of healing. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 0, "100")]
        public static float baseHealPercentage = 0.01f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Amount of healing per stack. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 1, "100")]
        public static float stackHealPercentage = 0.01f;
        public override void Initialize()
        {
            base.Initialize();
            //On.RoR2.FootstepHandler.Footstep_string_GameObject += ModifiedFootstepHandlerFootstep;
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.DormantFungus;

            private float timer;
            private float sprintTimer;

            private ModifiedFootstepHandler footstepHandler;
            private Mesh[] mesh = new Mesh[] { };
            private bool hasFootsteps = false;

            public void Start()
            {
                if (body.modelLocator.modelTransform.GetComponent<FootstepHandler>())
                {
                    footstepHandler = body.modelLocator.modelTransform.gameObject.AddComponent<ModifiedFootstepHandler>();
                    footstepHandler.footstepEffect = SS2Assets.LoadAsset<GameObject>("DungusTrailEffect", SS2Bundle.Items);
                    footstepHandler.enableFootstepDust = false;
                    hasFootsteps = true;
                }
            }

            public void FixedUpdate()
            {
                if (body.isSprinting)
                {
                    if (footstepHandler && sprintTimer > 1f)
                        footstepHandler.enableFootstepDust = true;
                    timer += Time.fixedDeltaTime;
                    sprintTimer += Time.fixedDeltaTime;
                    if (timer >= 1f)
                    {
                        if (NetworkServer.active)
                            body.healthComponent.HealFraction(baseHealPercentage + (stackHealPercentage * (stack - 1)), default);
                        timer = 0;
                        if (hasFootsteps)
                        {
                            if (Run.instance.runRNG.nextBool)
                                footstepHandler.footstepEffect = SS2Assets.LoadAsset<GameObject>("DungusTrailEffect", SS2Bundle.Items);
                            else
                                footstepHandler.footstepEffect = SS2Assets.LoadAsset<GameObject>("DungusTrailEffectAlt", SS2Bundle.Items);
                        }
                    }
                }
                else
                {
                    if (hasFootsteps)
                        footstepHandler.enableFootstepDust = false;
                    timer = 0;
                    sprintTimer = 0;
                }
            }

            public void OnDestroy()
            {
                Destroy(footstepHandler);
            }
        }

    }
}
