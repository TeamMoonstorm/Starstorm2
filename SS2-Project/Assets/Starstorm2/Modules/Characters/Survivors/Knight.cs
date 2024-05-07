using MSU;
using RoR2;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2.ContentManagement;
using R2API;
using static MSU.BaseBuffBehaviour;
using EntityStates;
using System;
#if DEBUG
namespace SS2.Survivors
{
    public sealed class Knight : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acKnight", SS2Bundle.Indev);

        public override void OnAssetCollectionLoaded(AssetCollection assetCollection)
        {
            _buffKnightCharged = assetCollection.FindAsset<BuffDef>("bdKnightCharged");
            _matChargedOverlay = assetCollection.FindAsset<Material>("matKnightSuperShield");
            _buffKnightSpecialPower = assetCollection.FindAsset<BuffDef>("bdKnightSpecialPowerBuff");
            _matSpecialPowerOverlay = assetCollection.FindAsset<Material>("matKnightBuffOverlay");
        }

        public BuffDef _buffKnightCharged;
        public Material _matChargedOverlay;

        public BuffDef _buffKnightSpecialPower; 
        public Material _matSpecialPowerOverlay; 

        public override void Initialize()
        {
            CharacterBody.onBodyStartGlobal += KnightBodyStart;
            ModifyPrefab();

            // Add the buff material overlays to buffoverlay dict
            BuffOverlays.AddBuffOverlay(_buffKnightCharged, _matChargedOverlay);
            BuffOverlays.AddBuffOverlay(_buffKnightSpecialPower, _matSpecialPowerOverlay);
        }


        public void ModifyPrefab()
        {
            var cb = CharacterPrefab.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/SimpleDotCrosshair.prefab").WaitForCompletion();
        }

        private void KnightBodyStart(CharacterBody body)
        {
            if (body.baseNameToken == "SS2_KNIGHT_BODY_NAME") // Every time one does an unecesary string comparasion, a developer dies -N
                body.SetBuffCount(SS2Content.Buffs.bdFortified.buffIndex, 3);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        // TODO: Load the actual buff 
        // The buff behavior for Knight's default passive
        public class KnightPassiveBuff : BaseBuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdKnightBuff;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.attackSpeedMultAdd += 0.4f;
                args.damageMultAdd += 0.4f;
            }
        }

        // TODO: Comment explaining the buff
        
        public class KnightChargedUpBuff : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdKnightCharged;
        }

        // TODO: Comment explaining the buff
        // TODO: Replace class with a single hook on RecalculateSTatsAPI.GetstatCoefficients. This way we replace the monobehaviour with just a method
        public class KnightShieldBuff : BaseBuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdShield;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.armorAdd += 100f;
                args.moveSpeedReductionMultAdd += 0.6f;
            }
        }

        // TODO: Replace class with a single hook on RecalculateSTatsAPI.GetstatCoefficients. This way we replace the monobehaviour with just a method
        public class KnightSpecialEmpowerBuff : BaseBuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdKnightSpecialPowerBuff;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseJumpPowerAdd += 0.7f;
                args.baseMoveSpeedAdd += 0.4f;
            }
        }

        // TODO: Replace class with a single hook on RecalculateSTatsAPI.GetstatCoefficients. This way we replace the monobehaviour with just a method
        public class KnightSpecialSlowEnemiesBuff : BaseBuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdKnightSpecialSlowBuff;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.attackSpeedReductionMultAdd += 2;
                args.moveSpeedReductionMultAdd += 2;
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
#endif
