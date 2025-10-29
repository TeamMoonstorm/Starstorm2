using RoR2;
using UnityEngine;
using RoR2.ContentManagement;
using R2API;
using RoR2.Items;
using RoR2.Orbs;
using System.Collections.Generic;
using MSU;
namespace SS2.Items
{
    public sealed class HitList : SS2Item // MAKE IT HIT LIST INSTEAD!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acHitList", SS2Bundle.Items);
        public override bool IsAvailable(ContentPack contentPack) => SS2Config.enableBeta;
        static GameObject orbEffect;
        static GameObject markEffect;
        public override void Initialize()
        {
            orbEffect = SS2Assets.LoadAsset<GameObject>("HitListOrbEffect", SS2Bundle.Items);
            markEffect = SS2Assets.LoadAsset<GameObject>("HitListIndicator", SS2Bundle.Items);
            BuffOverlays.AddBuffOverlay(SS2Assets.LoadAsset<BuffDef>("BuffHitListMark", SS2Bundle.Items), SS2Assets.LoadAsset<Material>("matHitListOverlay", SS2Bundle.Items));
            RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
        }

        private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.damageMultAdd += sender.inventory ? sender.inventory.GetItemCount(SS2Content.Items.StackHitList) * 0.01f : 0;
        }

        public class Behavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = false)]
            private static ItemDef GetItemDef() => SS2Content.Items.HitList;

            private static readonly Queue<GoldOrb> orbQueue = new Queue<GoldOrb>();
            private float markStopwatch;
            private void FixedUpdate()
            {
                markStopwatch += Time.fixedDeltaTime;
                float cooldown = 30f;
                for (int i = 0; i < stack-1; i++)
                    cooldown *= 0.8f;
                if (markStopwatch > cooldown && MarkRandomEnemy())
                {
                    markStopwatch = 0f;
                }
            }
            public bool MarkRandomEnemy()
            {
                List<CharacterBody> enemies = new List<CharacterBody>();
                foreach(CharacterBody enemy in CharacterBody.instancesList)
                {
                    if(enemy && enemy.healthComponent.alive && !enemy.HasBuff(SS2Content.Buffs.BuffHitListMark) && TeamMask.GetEnemyTeams(base.body.teamComponent.teamIndex).HasTeam(enemy.teamComponent.teamIndex) && enemy.teamComponent.teamIndex != TeamIndex.Neutral)
                    {
                        enemies.Add(enemy);
                    }
                }
                if(enemies.Count > 0)
                {
                    int i = UnityEngine.Random.Range(0, enemies.Count);
                    enemies[i].AddTimedBuff(SS2Content.Buffs.BuffHitListMark, 15f);
                    return true;
                }
                return false;
            }

            public void OnKilledOtherServer(DamageReport damageReport)
            {
                if(damageReport.victimBody && damageReport.victimBody.HasBuff(SS2Content.Buffs.BuffHitListMark))
                {
                    GrantItemOrb orb = new GrantItemOrb();
                    orb.origin = damageReport.victimBody.corePosition;
                    orb.target = Util.FindBodyMainHurtBox(damageReport.attackerBody);
                    orb.item = SS2Content.Items.StackHitList.itemIndex;
                    orb.count = 2;
                    orb.effectPrefab = orbEffect;
                    OrbManager.instance.AddOrb(orb);
                }
            }
        }

        public sealed class MarkBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation()]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffHitListMark;

            private TemporaryVisualEffect instance;
            private void FixedUpdate()
            {
                characterBody.UpdateSingleTemporaryVisualEffect(ref instance, markEffect, Mathf.Max(characterBody.radius, 1), characterBody.HasBuff(SS2Content.Buffs.BuffHitListMark));
            }
            private void OnEnable()
            {
                Util.PlaySound("RemunerationSpawn2", base.gameObject); //////////////////
                characterBody.UpdateSingleTemporaryVisualEffect(ref instance, markEffect, Mathf.Max(characterBody.radius, 1), characterBody.HasBuff(SS2Content.Buffs.BuffHitListMark));
            }
            private void OnDisable()
            {
                characterBody.UpdateSingleTemporaryVisualEffect(ref instance, markEffect, Mathf.Max(characterBody.radius, 1), characterBody.HasBuff(SS2Content.Buffs.BuffHitListMark));
            }
        }
    }
}
