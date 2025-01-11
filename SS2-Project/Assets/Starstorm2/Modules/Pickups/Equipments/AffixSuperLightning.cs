using MSU;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using RoR2.Orbs;
using System.Linq;
namespace SS2.Equipments
{
    public class AffixSuperLightning : SS2EliteEquipment
    {
        public override SS2AssetRequest<EliteAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<EliteAssetCollection>("acSuperLightning", SS2Bundle.Equipments);

        public static float orbBuffDuration = 13f;
        public override void Initialize()
        {
            BuffOverlays.AddBuffOverlay(SS2Assets.LoadAsset<BuffDef>("BuffSuperLightningOrb", SS2Bundle.Equipments), SS2Assets.LoadAsset<Material>("matSuperLightningBuffOverlay", SS2Bundle.Equipments));
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            if(sender.HasBuff(SS2Content.Buffs.BuffSuperLightningOrb))
            {
                args.moveSpeedMultAdd += 1f;
                args.attackSpeedMultAdd += 1f;
                args.cooldownReductionAdd += 99;
            }          
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override bool Execute(EquipmentSlot slot)
        {
            return false;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
            body.RemoveBuff(RoR2Content.Buffs.AffixBlue);
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
            body.AddBuff(RoR2Content.Buffs.AffixBlue);
        }

        public sealed class Behavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffAffixSuperLightning;
            private TemporaryOverlayInstance temporaryOverlayInstance;
            private static float orbCooldown = 4f;
            private static float orbSearchRadius = 80f;

            private float orbTimer;

            private void FixedUpdate()
            {
                orbTimer += Time.fixedDeltaTime;
                if(orbTimer >= orbCooldown)
                {
                    orbTimer -= orbCooldown;
                    HurtBox target = FindTarget();
                    OrbManager.instance.AddOrb(new SuperLightningOrb
                    {
                        target = target ? target : base.characterBody.mainHurtBox,
                        duration = 0.33f,
                        origin = base.characterBody.corePosition,
                    });
                    
                }    
            }

            private HurtBox FindTarget()
            {
                SphereSearch sphereSearch = new SphereSearch();
                sphereSearch.origin = base.transform.position;
                sphereSearch.radius = orbSearchRadius;
                sphereSearch.mask = LayerIndex.entityPrecise.mask;
                TeamMask mask = new TeamMask();
                mask.AddTeam(base.characterBody.teamComponent.teamIndex);
                HurtBox hurtBox = sphereSearch.RefreshCandidates()
                    .FilterCandidatesByHurtBoxTeam(mask)
                    .FilterCandidatesByDistinctHurtBoxEntities()
                    .OrderCandidatesByDistance()
                    .GetHurtBoxes()
                    .Where(hb => hb.healthComponent && hb.healthComponent != base.characterBody.healthComponent && !hb.healthComponent.body.HasBuff(SS2Content.Buffs.BuffSuperLightningOrb))
                    .FirstOrDefault();
                return hurtBox;
            }

            private void OnEnable()
            {
                if (characterBody.modelLocator && characterBody.modelLocator.modelTransform)
                {
                    temporaryOverlayInstance = TemporaryOverlayManager.AddOverlay(characterBody.modelLocator.modelTransform.gameObject);
                    temporaryOverlayInstance.animateShaderAlpha = false;
                    temporaryOverlayInstance.destroyComponentOnEnd = true;
                    temporaryOverlayInstance.originalMaterial = SS2Assets.LoadAsset<Material>("matSuperLightningOverlay", SS2Bundle.Equipments);
                    temporaryOverlayInstance.AddToCharacterModel(characterBody.modelLocator.modelTransform.GetComponent<CharacterModel>());
                }
            }
            private void OnDisable()
            {
                if (temporaryOverlayInstance != null)
                    temporaryOverlayInstance.RemoveFromCharacterModel();
            }
        }

        public sealed class OrbBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffSuperLightningOrb;

            private GameObject bubbleEffect;
            private void OnEnable()
            {
                bubbleEffect = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("SuperLightningBuffEffect", SS2Bundle.Equipments), characterBody.coreTransform.position, Quaternion.identity);
                bubbleEffect.transform.localScale *= characterBody.radius;
            }
            private void OnDisable()
            {
                if (bubbleEffect) Destroy(bubbleEffect);

                //EffectData effectData = new EffectData
                //{
                //    origin = this.characterBody.corePosition,
                //    rotation = Quaternion.identity,
                //    scale = this.characterBody.radius,
                //};
                //effectData.SetNetworkedObjectReference(this.characterBody.gameObject);
                //EffectManager.SpawnEffect(endEffectPrefab, effectData, true);
            }
            private void Update()
            {
                if (bubbleEffect)
                    bubbleEffect.transform.position = base.characterBody.corePosition;
            }
        }

        private class SuperLightningOrb : Orb
        {
            public override void Begin()
            {
                EffectData effectData = new EffectData
                {
                    origin = this.origin,
                    genericFloat = base.duration
                };
                effectData.SetHurtBoxReference(this.target);
                EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("SuperLightningOrbEffect", SS2Bundle.Equipments), effectData, true);
            }

            public override void OnArrival()
            {
                base.OnArrival();

                if (this.target && this.target.healthComponent && this.target.healthComponent.body)
                {
                    this.target.healthComponent.body.AddTimedBuff(SS2Content.Buffs.BuffSuperLightningOrb, orbBuffDuration);
                    EffectData effectData = new EffectData
                    {
                        origin = target.transform.position,
                        rotation = Quaternion.identity,
                        scale = target.healthComponent.body.radius,
                    };
                    EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("SuperLightningOrbImpact", SS2Bundle.Equipments), effectData, true);
                }
            }
        }
    }
}
