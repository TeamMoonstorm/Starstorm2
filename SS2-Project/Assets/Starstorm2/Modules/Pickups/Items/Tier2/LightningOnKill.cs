using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using SS2.Components;
using System.Collections.Generic;
using RoR2.Orbs;
using MSU;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace SS2.Items
{
    public sealed class LightningOnKill : SS2Item
    {
        private const string token = "SS2_ITEM_LIGHTNINGONKILL_DESC";

        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;

        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Total damage of Man O' War's lightning. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float damageCoeff = 2f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Number of bounces.")]
        [FormatToken(token, 1)]
        public static int bounceBase = 3;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Number of bounces per stack.")]
        [FormatToken(token, 2)]
        public static int bounceStack = 2;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Radius of Man O' War's lightning, in meters.")]
        [FormatToken(token, 3)]
        public static float radiusBase = 20f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Radius of Man O' War's lightning per stack, in meters.")]
        [FormatToken(token, 4)]
        public static float radiusPerStack = 4f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Proc coefficient of damage dealt by Man o' War.")]
        public static float procCo = 1f;

        private static GameObject _orbEffect;
        private static NetworkSoundEventDef _soundEffect;

        public override void Initialize()
        {
            GlobalEventManager.onCharacterDeathGlobal += ProcLightningOnKill;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "LightningOnKill" - Items
             * GameObject - "JellyOrbEffect" - Items
             * NetworkSoundEventDef - "nsedProcLightningOnKill" - Items
             */
            yield break;
        }

        private void ProcLightningOnKill(DamageReport damageReport)
        {
            CharacterBody body = damageReport.attackerBody;
            int stack = body && body.inventory ? body.inventory.GetItemCount(ItemDef) : 0;
            if (stack <= 0) return;



            if (!NetworkServer.active) return;

            int bouncesRemaining = bounceBase + bounceStack * (stack - 1) - 1;



            CustomLightningOrb orb = new CustomLightningOrb();
            orb.orbEffectPrefab = _orbEffect;

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

                EffectManager.SimpleSoundEffect(_soundEffect.index, damageReport.victim.transform.position, true);
                orb.target = hurtbox;
                OrbManager.instance.AddOrb(orb);
            }
        }
    }
}
