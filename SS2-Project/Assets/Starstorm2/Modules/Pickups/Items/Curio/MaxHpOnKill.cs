using RoR2;
using UnityEngine;
using RoR2.ContentManagement;
using RoR2.Orbs;
using R2API;
using UnityEngine.Networking;
namespace SS2.Items
{
    // scrap from chests
    // potential from chest
    // elites drop items
    // champions drop items

    // hardened shell
    // bleedout
    // shield gating/shield on kill
    // max hp on kill

    // stack crit on shrine use. convert crit chance to crit dmg
    // dmg to elites ?
    // debuffs deal % health dmg
    // more money from mobs. dmg from money?


    //storm:
    //scrap
    //shieldgate
    //elite dmg

    //fire:
    //champions
    //bleedout
    //money

    //ice:
    //elites
    //shell
    //debuffs

    //lightning:
    //potentials
    //hp
    //crit

    //earth:
    //champion
    //shell
    //elite dmg

    //poison:
    //scrap
    //hp
    //debuffs

    //gold:
    //scrap
    //shell
    //money

    //void:
    //potential
    //shield
    //crit
    public sealed class MaxHpOnKill : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acMaxHpOnKill", SS2Bundle.Items);
        public override bool IsAvailable(ContentPack contentPack) => SS2Config.enableBeta;
        public static GameObject orbEffect;
        public override void Initialize()
        {
            orbEffect = SS2Assets.LoadAsset<GameObject>("HpOnKillOrbEffect", SS2Bundle.Items);
            RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
        }

        private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (!sender.inventory) return;
            args.baseHealthAdd += sender.inventory.GetItemCount(SS2Content.Items.StackMaxHpOnKill);
        }

        private void OnCharacterDeathGlobal(DamageReport damageReport)
        {
            if (!NetworkServer.active) return;
            Inventory inventory = damageReport.attackerMaster ? damageReport.attackerMaster.inventory : null;
            if (inventory)
            {
                int hp = inventory.GetItemCount(SS2Content.Items.MaxHpOnKill);
                Vector3 origin = damageReport.victimBody ? damageReport.victimBody.corePosition : Vector3.zero;
                if (hp > 0)
                {
                    GrantItemOrb orb = new GrantItemOrb();
                    orb.origin = origin;
                    orb.target = Util.FindBodyMainHurtBox(damageReport.attackerBody);
                    orb.item = SS2Content.Items.StackMaxHpOnKill.itemIndex;
                    orb.count = 1 + hp;
                    orb.effectPrefab = orbEffect;
                    OrbManager.instance.AddOrb(orb);
                }
            }
        }
    }
    public class GrantItemOrb : Orb
    {
        public override void Begin()
        {
            base.duration = base.distanceToTarget / speed;
            EffectData effectData = new EffectData
            {
                origin = this.origin,
                genericFloat = base.duration
            };
            effectData.SetHurtBoxReference(this.target);
            EffectManager.SpawnEffect(effectPrefab, effectData, true);
            HurtBox component = this.target.GetComponent<HurtBox>();
            CharacterBody characterBody = (component != null) ? component.healthComponent.GetComponent<CharacterBody>() : null;
            if (characterBody)
            {
                this.targetInventory = characterBody.inventory;
            }
        }
        public override void OnArrival()
        {
            if (this.targetInventory)
            {
                this.targetInventory.GiveItem(item, count);
            }
        }

        public float speed = 30f;

        public int count;
        public ItemIndex item;
        public GameObject effectPrefab;
        private Inventory targetInventory;
    }
}
