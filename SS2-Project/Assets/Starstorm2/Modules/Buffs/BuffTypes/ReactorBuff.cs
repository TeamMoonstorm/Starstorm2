using R2API;
using RoR2;
using UnityEngine;

using MSU;
namespace SS2.Buffs
{
    //[DisabledContent]
    public sealed class ReactorBuff : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffReactor", SS2Bundle.Items);
        public override Material OverlayMaterial => SS2Assets.LoadAsset<Material>("matReactorBuffOverlay", SS2Bundle.Items);

        public static GameObject bubbleEffectPrefab => SS2Assets.LoadAsset<GameObject>("ReactorBubbleEffect", SS2Bundle.Items);

        public static GameObject endEffectPrefab => SS2Assets.LoadAsset<GameObject>("ReactorBubbleEnd", SS2Bundle.Items);
        public static GameObject shieldEffectPrefab => SS2Assets.LoadAsset<GameObject>("ReactorShieldEffect", SS2Bundle.Items);
        //To-Do: Maybe better invincibility implementation. Projectile deflection for cool points?
        public sealed class Behavior : BaseBuffBodyBehavior, RoR2.IOnIncomingDamageServerReceiver, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffReactor;

            private GameObject bubbleEffect;
            private void OnEnable()
            {
                bubbleEffect = GameObject.Instantiate(bubbleEffectPrefab, body.coreTransform);
                bubbleEffect.transform.localScale *= body.radius;
            }
            private void OnDisable()
            {
                if (bubbleEffect) Destroy(bubbleEffect);

                EffectData effectData = new EffectData
                {
                    origin = this.body.corePosition,
                    rotation = Quaternion.identity,
                    scale = this.body.radius,
                };
                effectData.SetNetworkedObjectReference(this.body.gameObject);
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

                Vector3 direction = damageInfo.position - this.body.corePosition;
                if(damageInfo.attacker)
                {
                    CharacterBody body = damageInfo.attacker.GetComponent<CharacterBody>();
                    // from hit position to attacker's core position
                    direction = (body ? body.corePosition : damageInfo.attacker.transform.position) - damageInfo.position;
                }
                EffectData effectData = new EffectData
                {
                    origin = this.body.corePosition,
                    rotation = Util.QuaternionSafeLookRotation(direction),
                    scale = this.body.radius,
                };
                effectData.SetNetworkedObjectReference(this.body.gameObject);
                EffectManager.SpawnEffect(shieldEffectPrefab, effectData, true);

                
            }


        }
    }
}
