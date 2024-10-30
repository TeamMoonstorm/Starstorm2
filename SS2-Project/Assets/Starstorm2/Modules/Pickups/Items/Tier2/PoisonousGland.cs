using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using RoR2.Orbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SS2.Components;
namespace SS2.Items
{
    //check StickyOverloaderController for rest of behavior
    public sealed class PoisonousGland : SS2Item, IContentPackModifier
    {
        private const string token = "SS2_ITEM_POISONOUSGLAND_DESC";

        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acPoisonousGland", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Chance for monsters to miss their attacks, per debuff. (1 = 1%)")]
        [FormatToken(token, 0)]
        public static float missChance = 15f;

        private static GameObject effectPrefab;
        private static GameObject ball;
        private static GameObject proc;

        private static Material FUCK;
        public override void Initialize()
        {
            effectPrefab = SS2Assets.LoadAsset<GameObject>("PoisonousGlandEffect", SS2Bundle.Items);
            ball = SS2Assets.LoadAsset<GameObject>("PoisonBall", SS2Bundle.Items);
            proc = SS2Assets.LoadAsset<GameObject>("GlandProc", SS2Bundle.Items);
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;

            FUCK = SS2Assets.LoadAsset<Material>("tmpSquaresBold Material", SS2Bundle.Items);
            FUCK.shader = LegacyShaderAPI.Find("TextMeshPro/Distance Field");
        }
        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
        private void OnServerDamageDealt(DamageReport report)
        {
            if (report.damageInfo.procCoefficient <= 0) return;
            CharacterBody body = report.attackerBody;
            if (!body || !body.inventory) return;
            int stack = body.inventory.GetItemCount(SS2Content.Items.PoisonousGland);
            if (stack <= 0) return;        
            report.victimBody.AddTimedBuff(SS2Content.Buffs.BuffPoisonousGland, 2f * stack);      
        }

        public sealed class Behavior : BaseBuffBehaviour, IOnIncomingDamageOtherServerReciever, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffPoisonousGland;

            private GameObject effectInstance;
            private List<Transform> balls = new List<Transform>();
            private int debuffCount;
            private Collider bodyCollider;
            private ObjectScaleCurve scaler;
            private void OnEnable()
            {
                effectInstance = GameObject.Instantiate(effectPrefab, characterBody.coreTransform.position, Quaternion.identity);
                ////effectInstance.transform.localScale = Vector3.one * characterBody.radius;
                bodyCollider = base.GetComponent<Collider>();
                scaler = effectInstance.GetComponent<ObjectScaleCurve>();
            }
            private void OnDisable()
            {
                Destroy(effectInstance);
                balls = new List<Transform>();
                debuffCount = 0;
            }
            private void Start()
            {
                UpdateDebuffs(); // timing junk idk what basebuffbehavior even does
            }
            private void FixedUpdate()
            {
                if (!base.characterBody.healthComponent.alive)
                    Destroy(this.effectInstance);
            }

            // hover over head
            private void Update()
            {
                if(effectInstance)
                {
                    Vector3 a = base.transform.position;
                    if (this.bodyCollider)
                    {
                        a = this.bodyCollider.bounds.center + new Vector3(0f, this.bodyCollider.bounds.extents.y, 0f);
                    }
                    effectInstance.transform.position = a;
                }
            }

            private void SetNewDebuffCount(int stack)
            {
                if (scaler) scaler.time = 0;
                float rotation = 360f / stack;
                for(int i = balls.Count; i < stack; i++) // if stack > balls
                {
                    balls.Add(GameObject.Instantiate(ball, effectInstance.transform).transform);
                }
                for(int i = balls.Count - 1; i > stack; i--) // if balls > stack
                {
                    GameObject.Destroy(balls[i].gameObject);
                    balls.RemoveAt(i);
                }
                for(int i = 0; i < balls.Count; i++)
                {
                    balls[i].rotation = Quaternion.Euler(0, rotation * i, 0);
                }
            }

            public void OnIncomingDamageOther(HealthComponent victimHealthComponent, DamageInfo damageInfo)
            {              
                if (base.buffCount > 0)
                {
                    float miss = Util.ConvertAmplificationPercentageIntoReductionPercentage(missChance * buffCount);
                    if(Util.CheckRoll(miss, victimHealthComponent.body.master))
                    {
                        damageInfo.rejected = true;
                        EffectManager.SimpleEffect(proc, damageInfo.position, Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : UnityEngine.Random.onUnitSphere), true);
                    }
                }
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                UpdateDebuffs();              
            }
            private void UpdateDebuffs()
            {
                if (buffCount == 0) return;
                int debuff = 0;
                foreach (BuffIndex buffType in BuffCatalog.debuffBuffIndices)
                {
                    if (base.characterBody.HasBuff(buffType))
                    {
                        debuff++;
                    }
                }
                DotController dotController = DotController.FindDotController(base.gameObject);
                if (dotController)
                {
                    for (DotController.DotIndex dotIndex = DotController.DotIndex.Bleed; dotIndex < DotController.DotIndex.Count; dotIndex++)
                    {
                        if (dotController.HasDotActive(dotIndex))
                        {
                            debuff++;
                        }
                    }
                }
                if (debuff != debuffCount)
                {
                    SetNewDebuffCount(debuff);
                    debuffCount = debuff;
                }
            }
        }
    }
}
