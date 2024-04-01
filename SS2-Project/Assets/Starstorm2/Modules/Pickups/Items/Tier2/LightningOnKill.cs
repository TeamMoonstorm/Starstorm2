using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using SS2.Components;
using System.Collections.Generic;
using RoR2.Orbs;
namespace SS2.Items
{
    public sealed class LightningOnKill : SS2Item
    {
        private const string token = "SS2_ITEM_LIGHTNINGONKILL_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("LightningOnKill", SS2Bundle.Items);

        public static GameObject orbEffect = SS2Assets.LoadAsset<GameObject>("JellyOrbEffect", SS2Bundle.Items);

        public static NetworkSoundEventDef soundEffect = SS2Assets.LoadAsset<NetworkSoundEventDef>("nsedProcLightningOnKill", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.ID_ITEM, ConfigDesc = "Total damage of Man O' War's lightning. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 0, "100")]
        public static float damageCoeff = 2f;         

        [RooConfigurableField(SS2Config.ID_ITEM, ConfigDesc = "Number of bounces.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static int bounceBase = 3;

        [RooConfigurableField(SS2Config.ID_ITEM, ConfigDesc = "Number of bounces per stack.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static int bounceStack = 2;

        [RooConfigurableField(SS2Config.ID_ITEM, ConfigDesc = "Radius of Man O' War's lightning, in meters.")]
        [TokenModifier(token, StatTypes.Default, 3)]
        public static float radiusBase = 20f;

        [RooConfigurableField(SS2Config.ID_ITEM, ConfigDesc = "Radius of Man O' War's lightning per stack, in meters.")]
        [TokenModifier(token, StatTypes.Default, 4)]
        public static float radiusPerStack = 4f;

        [RooConfigurableField(SS2Config.ID_ITEM, ConfigDesc = "Proc coefficient of damage dealt by Man o' War.")]
        public static float procCo = 1f;


        public override void Initialize()
        {
            GlobalEventManager.onCharacterDeathGlobal += ProcLightningOnKill;
        }

        private void ProcLightningOnKill(DamageReport damageReport)
        {
            CharacterBody body = damageReport.attackerBody;
            int stack = body && body.inventory ? body.inventory.GetItemCount(ItemDef) : 0;
            if (stack <= 0) return;

            

            if (!NetworkServer.active) return;

            int bouncesRemaining = bounceBase + bounceStack * (stack - 1) - 1;

            

            CustomLightningOrb orb = new CustomLightningOrb();
            orb.orbEffectPrefab = orbEffect;
            
            orb.duration = 0.033f;
            orb.bouncesRemaining = bouncesRemaining;
            orb.range = radiusBase + radiusPerStack * (stack - 1);
            orb.damageCoefficientPerBounce = 1f;
            orb.damageValue = body.damage * damageCoeff;
            orb.damageType = DamageType.Generic;
            orb.isCrit = body.RollCrit();
            orb.damageColorIndex = DamageColorIndex.Item;
            orb.procCoefficient = procCo;
            orb.origin = damageReport.victimBody.corePosition;
            orb.teamIndex = body.teamComponent.teamIndex;
            orb.attacker = body.gameObject;
            orb.procChainMask = default(ProcChainMask);
            orb.bouncedObjects = new List<HealthComponent>();
            HurtBox hurtbox = orb.PickNextTarget(damageReport.victimBody.corePosition, damageReport.victim);
            if (hurtbox)
            {
                if (body.inventory.GetItemCount(SS2Content.Items.ErraticGadget) > 0) // should probably just put this in CustomLightningOrb ?
                {
                    orb.canProcGadget = true;
                }

                EffectManager.SimpleSoundEffect(soundEffect.index, damageReport.victim.transform.position, true);
                orb.target = hurtbox;
                OrbManager.instance.AddOrb(orb);
            }
        }
    }
}
