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

        public override void Initialize()
        {
            BuffDef buffKnightCharged = AssetCollection.FindAsset<BuffDef>("bdKnightCharged");
            Material matChargedOverlay = AssetCollection.FindAsset<Material>("matKnightSuperShield");
            BuffDef buffKnightSpecialPower = AssetCollection.FindAsset<BuffDef>("bdKnightSpecialPowerBuff");
            Material matSpecialPowerOverlay = AssetCollection.FindAsset<Material>("matKnightBuffOverlay");

            CharacterBody.onBodyStartGlobal += KnightBodyStart;
            ModifyPrefab();
            R2API.RecalculateStatsAPI.GetStatCoefficients += ModifyStats;

            // Add the buff material overlays to buffoverlay dict
            BuffOverlays.AddBuffOverlay(buffKnightCharged, matChargedOverlay);
            BuffOverlays.AddBuffOverlay(buffKnightSpecialPower, matSpecialPowerOverlay);
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
                args.baseJumpPowerAdd += 0.5f;
                args.baseMoveSpeedAdd += 1.0f;
                args.jumpPowerMultAdd += 1.2f;
            }

            if (sender.HasBuff(SS2Content.Buffs.bdKnightSpecialSlowBuff))
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
