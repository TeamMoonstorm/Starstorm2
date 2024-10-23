using MSU;
using RoR2;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2.ContentManagement;
using R2API;
using EntityStates;
using System;
using UnityEngine.Networking;

namespace SS2.Survivors
{
    public sealed class Knight : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acKnight", SS2Bundle.Indev);
        
        public SS2AssetRequest<AssetCollection> ExtraKnightAssets => SS2Assets.LoadAssetAsync<AssetCollection>("acKnightExtra", SS2Bundle.Indev);

        public static GameObject KnightImpactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2SmokeBomb.prefab").WaitForCompletion();
        public static GameObject KnightCrosshair = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/SimpleDotCrosshair.prefab").WaitForCompletion();
        public static GameObject KnightDroppod = Addressables.LoadAssetAsync<GameObject>("Prefabs/NetworkedObjects/SurvivorPod").WaitForCompletion();
        public static GameObject KnightPassiveWard;
        public static GameObject KnightHitEffect;
        public static GameObject KnightSpinEffect;

        public static SerializableEntityStateType ShieldStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Knight.Shield));

        public static float dodgeFOV = EntityStates.Commando.DodgeState.dodgeFOV;

        public static Vector3 chargeCameraPos = new Vector3(1.2f, -0.25f, -6.1f);
        public static Vector3 altCameraPos = new Vector3(-1.2f, -0.25f, -6.1f);

        
        public static CharacterCameraParamsData chargeCameraParams = new CharacterCameraParamsData
        {
            maxPitch = 85f,
            minPitch = -85f,
            pivotVerticalOffset = 1f,
            idealLocalCameraPos = chargeCameraPos,
            wallCushion = 0.1f,
        };

        public static CharacterCameraParamsData altCameraParams = new CharacterCameraParamsData
        {
            maxPitch = 85f,
            minPitch = -85f,
            pivotVerticalOffset = 1f,
            idealLocalCameraPos = altCameraPos,
            wallCushion = 0.1f,
        };

        public AssetCollection ExtraAssetCollection { get; set; }

        public static float reducedGravity = 0.07f;

        public static DamageAPI.ModdedDamageType ExtendedStunDamageType { get; set; }

        private static float stunDebuffDuration = 3f;

        public override IEnumerator LoadContentAsync()
        {
            SS2AssetRequest<SurvivorAssetCollection> request = AssetRequest;
            SS2AssetRequest<AssetCollection> extraRequest = ExtraKnightAssets;

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            AssetCollection = request.Asset;

            CharacterPrefab = AssetCollection.bodyPrefab;
            masterPrefab = AssetCollection.masterPrefab;
            survivorDef = AssetCollection.survivorDef;


            extraRequest.StartLoad();
            while (!extraRequest.IsComplete)
                yield return null;

            ExtraAssetCollection = extraRequest.Asset;

        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(ExtraAssetCollection);
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }

        public override void Initialize()
        {
            BuffDef buffKnightSpecialPower = AssetCollection.FindAsset<BuffDef>("bdKnightSpecialPowerBuff");
            Material matSpecialPowerOverlay = AssetCollection.FindAsset<Material>("matKnightBuffOverlay");
            KnightHitEffect = AssetCollection.FindAsset<GameObject>("KnightImpactSlashEffect");
            KnightSpinEffect = AssetCollection.FindAsset<GameObject>("KnightSpin");
            KnightPassiveWard = AssetCollection.FindAsset<GameObject>("KnightPassiveBuffWard");

            RegisterKnightDamageTypes();
            ModifyPrefab();
            R2API.RecalculateStatsAPI.GetStatCoefficients += ModifyStats;

            // Add the buff material overlays to buffoverlay dict
            BuffOverlays.AddBuffOverlay(buffKnightSpecialPower, matSpecialPowerOverlay);
        }

        private void RegisterKnightDamageTypes()
        {
            ExtendedStunDamageType = R2API.DamageAPI.ReserveDamageType();
            GlobalEventManager.onServerDamageDealt += ApplyExtendedStun;
        }
        private void ApplyExtendedStun(DamageReport obj)
        {
            var victimBody = obj.victimBody;
            var damageInfo = obj.damageInfo;

            if (DamageAPI.HasModdedDamageType(damageInfo, ExtendedStunDamageType))
            {
                victimBody.AddTimedBuffAuthority(SS2Content.Buffs.bdKnightStunAttack.buffIndex, stunDebuffDuration);
            }
        }


        public void ModifyPrefab()
        {
            var cb = CharacterPrefab.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = KnightCrosshair;
            cb.preferredPodPrefab = KnightDroppod;

            EntityStateMachine bodyState = EntityStateMachine.FindByCustomName(cb.gameObject, "Body");
            bodyState.mainStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Knight.MainState));
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        private void ModifyStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            // The buff behavior for Knight's default passive
            if (sender.HasBuff(SS2Content.Buffs.bdKnightBuff))
            {
                args.attackSpeedMultAdd += 0.2f;
                args.damageMultAdd += 0.2f;
            }

            if (sender.HasBuff(SS2Content.Buffs.bdKnightShield))
            {
                args.armorAdd += 200f;
                args.moveSpeedReductionMultAdd += 0.6f;
            }

            if (sender.HasBuff(SS2Content.Buffs.bdKnightSpecialPowerBuff))
            {
                args.baseJumpPowerAdd += 1f;
                args.baseMoveSpeedAdd += 0.1f;
                args.jumpPowerMultAdd += 0.5f;
                args.damageMultAdd += 0.2f;
            }

            if (sender.HasBuff(SS2Content.Buffs.bdKnightSpecialSlowBuff))
            {
                args.attackSpeedReductionMultAdd += 2;
                args.moveSpeedReductionMultAdd += 2;
            }

            if (sender.HasBuff(SS2Content.Buffs.bdKnightStunAttack))
            {
                SetStateOnHurt setStateOnHurt = sender.GetComponent<SetStateOnHurt>();
                
                if (setStateOnHurt)
                {
                    setStateOnHurt.SetStun(4f);
                    sender.RemoveOldestTimedBuff(SS2Content.Buffs.bdKnightStunAttack);
                }
            }
        }

            public class KnightSpecialPowerBuff : BaseBuffBehaviour
            {
                [BuffDefAssociation]
                private static BuffDef GetBuffDef() => SS2Content.Buffs.bdKnightSpecialPowerBuff;

                private void FixedUpdate()
                {
                    if (!hasAnyStacks || !characterBody.characterMotor || !characterBody)
                        return;

                    if (characterBody.characterMotor.isGrounded)
                    {
                        return;
                    }

                    // TODO: No clue if this will work
                    characterBody.characterMotor.velocity.y -= Time.fixedDeltaTime * Physics.gravity.y * reducedGravity;
                }
            }
    }
}
