using MSU;
using RoR2;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2.ContentManagement;
using R2API;
using EntityStates;
using System;
namespace SS2.Survivors
{
    public sealed class Knight : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acKnight", SS2Bundle.Indev);
        
        public SS2AssetRequest<AssetCollection> ExtraKnightAssets => SS2Assets.LoadAssetAsync<AssetCollection>("acKnightExtra", SS2Bundle.Indev);

        public AssetCollection ExtraAssetCollection { get; set; }

        public static float reducedGravity = 0.10f;

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
            MasterPrefab = AssetCollection.masterPrefab;
            SurvivorDef = AssetCollection.survivorDef;


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
            cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/SimpleDotCrosshair.prefab").WaitForCompletion();
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
                args.attackSpeedMultAdd += 0.4f;
                args.damageMultAdd += 0.4f;
            }

            if (sender.HasBuff(SS2Content.Buffs.bdShield))
            {
                args.armorAdd += 100f;
                args.moveSpeedReductionMultAdd += 0.6f;
            }

            if (sender.HasBuff(SS2Content.Buffs.bdKnightSpecialPowerBuff))
            {
                args.baseJumpPowerAdd += 1f;
                args.baseMoveSpeedAdd += 0.3f;
                args.jumpPowerMultAdd += 0.5f;
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
                    setStateOnHurt.SetStun(3f);
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
                    if (!HasAnyStacks || !CharacterBody.characterMotor || !CharacterBody)
                        return;

                    if (CharacterBody.characterMotor.isGrounded)
                    {
                        return;
                    }

                    // TODO: No clue if this will work
                    CharacterBody.characterMotor.velocity.y -= Time.fixedDeltaTime * Physics.gravity.y * reducedGravity;
                }
            }



        public class KnightParryBuff : BaseBuffBehaviour, IOnIncomingDamageServerReceiver
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdParry;

            // Called when the body with this buff takes damage
            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (HasAnyStacks && damageInfo.attacker != CharacterBody)
                {
                    // TODO: Use body index
                    // We want to ensure that Knight is the one taking damage
                    if (CharacterBody.baseNameToken != "SS2_KNIGHT_BODY_NAME")
                        return;

                    damageInfo.rejected = true;

                    SetStateOnHurt ssoh = damageInfo.attacker.GetComponent<SetStateOnHurt>();
                    if (ssoh)
                    {
                        // Stun the enemy
                        Type state = ssoh.targetStateMachine.state.GetType();
                        if (state != typeof(StunState) && state != typeof(ShockState) && state != typeof(FrozenState))
                        {
                            ssoh.SetStun(3f);
                        }
                    }

                    // TODO: Should we have a custom sound for this?
                    Util.PlaySound("NemmandoDecisiveStrikeReady", gameObject);

                    EntityStateMachine weaponEsm = EntityStateMachine.FindByCustomName(gameObject, "Weapon");
                    if (weaponEsm != null)
                    {
                        weaponEsm.SetNextState(new EntityStates.Knight.Parry());
                    }

                    Destroy(this);
                }
            }
        }
    }
}
