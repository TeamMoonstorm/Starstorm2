using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS2.Items
{
    public sealed class PortableReactor : SS2Item, IContentPackModifier
    {
        private const string token = "SS2_ITEM_PORTABLEREACTOR_DESC";

        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;

        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Duration of invulnerability from Portable Reactor. (1 = 1 second)")]
        [FormatToken(token, 0)]
        public static float invulnTime = 80f;
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Stacking duration of invulnerability. (1 = 1 second)")]
        [FormatToken(token, 1)]
        public static float stackingInvuln = 40f;

        private BuffDef _buffPortableReactor; //SS2Assets.LoadAsset<BuffDef>("BuffReactor", SS2Bundle.Items);
        public static Material _overlay; // SS2Assets.LoadAsset<Material>("matReactorBuffOverlay", SS2Bundle.Items);

        public static GameObject bubbleEffectPrefab; //SS2Assets.LoadAsset<GameObject>("ReactorBubbleEffect", SS2Bundle.Items);
        public static GameObject endEffectPrefab; //SS2Assets.LoadAsset<GameObject>("ReactorBubbleEnd", SS2Bundle.Items);
        public static GameObject shieldEffectPrefab; //SS2Assets.LoadAsset<GameObject>("ReactorShieldEffect", SS2Bundle.Items);

        //★ ty nebby
        //To-Do: This thing spits out a few errors every time a stage is started. Doesn't really NEED fixed, but probably should be.
        //N: This tbh should be moved to a hook on stage start, to avoid having a resurrected player becoming invincible.
        public override void Initialize()
        {
            CharacterBody.onBodyStartGlobal += ImFuckingInvincible;

            BuffOverlays.AddBuffOverlay(_buffPortableReactor, _overlay);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "PortableReactor" - Items
             * BuffDef - "BuffReactor" - Items
             */
            yield break;
        }

        private void ImFuckingInvincible(CharacterBody obj)
        {
            if(obj.TryGetItemCount(_itemDef, out var count))
            {
                if (obj.isPlayerControlled)
                {
                    obj.AddTimedBuff(SS2Content.Buffs.BuffReactor, invulnTime + ((count - 1) * stackingInvuln));
                }
                else
                {
                    obj.AddTimedBuff(SS2Content.Buffs.BuffReactor, invulnTime / 4 + ((count - 1) * stackingInvuln));
                }
            }
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.buffDefs.AddSingle(_buffPortableReactor);
        }

        public sealed class Behavior : BaseBuffBehaviour, RoR2.IOnIncomingDamageServerReceiver, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffReactor;

            private GameObject bubbleEffect;
            private void OnEnable()
            {
                bubbleEffect = GameObject.Instantiate(bubbleEffectPrefab, CharacterBody.coreTransform);
                bubbleEffect.transform.localScale *= CharacterBody.radius;
            }
            private void OnDisable()
            {
                if (bubbleEffect) Destroy(bubbleEffect);

                EffectData effectData = new EffectData
                {
                    origin = this.CharacterBody.corePosition,
                    rotation = Quaternion.identity,
                    scale = this.CharacterBody.radius,
                };
                effectData.SetNetworkedObjectReference(this.CharacterBody.gameObject);
                EffectManager.SpawnEffect(endEffectPrefab, effectData, true);
            }
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.moveSpeedMultAdd += 1f;
            }

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (damageInfo.damageType == DamageType.VoidDeath) return;

                damageInfo.rejected = true;

                Vector3 direction = damageInfo.position - this.CharacterBody.corePosition;
                if (damageInfo.attacker)
                {
                    CharacterBody CharacterBody = damageInfo.attacker.GetComponent<CharacterBody>();
                    // from hit position to attacker's core position
                    direction = (CharacterBody ? CharacterBody.corePosition : damageInfo.attacker.transform.position) - damageInfo.position;
                }
                EffectData effectData = new EffectData
                {
                    origin = this.CharacterBody.corePosition,
                    rotation = Util.QuaternionSafeLookRotation(direction),
                    scale = this.CharacterBody.radius,
                };
                effectData.SetNetworkedObjectReference(this.CharacterBody.gameObject);
                EffectManager.SpawnEffect(shieldEffectPrefab, effectData, true);


            }
        }
    }
}