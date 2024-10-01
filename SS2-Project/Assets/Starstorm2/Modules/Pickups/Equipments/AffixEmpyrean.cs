using MSU;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Linq;
using MSU.Config;

namespace SS2.Equipments
{
    public sealed class AffixEmpyrean : SS2EliteEquipment
    {
        public override SS2AssetRequest<EliteAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<EliteAssetCollection>("acAffixEmpyrean", SS2Bundle.Equipments);

        public static List<EliteDef> whitelistedEliteDefs = new List<EliteDef>();

        public static List<String> whitelistedEliteDefStrings = new List<String>();

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Enabled Elite types, separated by commas. Default: \"EliteFireEquipment,EliteIceEquipment,EliteLightningEquipment,ElitePoisonEquipment,EliteEarthEquipment\"")]
        public static string eliteDefEnabledStrings = "EliteFireEquipment,EliteIceEquipment,EliteLightningEquipment,EliteEarthEquipment";
        public static void AddEliteToWhitelist(EliteDef eliteDef) => whitelistedEliteDefs.Add(eliteDef);

        public override void Initialize()
        {
            Run.onRunStartGlobal += CreateWhitelist;
            IL.RoR2.CharacterBody.RecalculateStats += RecalculateStatsEmpyreanIL;
        }
        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        #region Hooks
        private void RecalculateStatsEmpyreanIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            bool ILFound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld(typeof(RoR2Content.Buffs), nameof(RoR2Content.Buffs.AffixBlue)),
                x => x.MatchCallOrCallvirt<CharacterBody>(nameof(CharacterBody.HasBuff)),
                x => x.MatchBrfalse(out _),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<CharacterBody>("get_maxHealth"),
                x => x.MatchLdcR4(0.5f)
                );
            if (ILFound)
            {

                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, CharacterBody, float>>((defaultPercentage, body) =>
                {
                    if (body.HasBuff(SS2Content.Buffs.bdEmpyrean))
                    {
                        return 0.1f;
                    }
                    return defaultPercentage;
                });
            }
            else
            {
                SS2Log.Fatal("Failed to find IL match for Empyrean hook 1!");
            }

            bool ILFound2 = c.TryGotoNext(MoveType.After,
                x => x.MatchSub(),
                x => x.MatchCallOrCallvirt<CharacterBody>("set_maxHealth"),
                x => x.MatchLdloc(out _),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<CharacterBody>("get_maxHealth")
                );
            if (ILFound2)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, CharacterBody, float>>((defaultAmount, body) =>
                {
                    if (body.HasBuff(SS2Content.Buffs.bdEmpyrean))
                    {
                        return defaultAmount * 0.111f;
                    }
                    return defaultAmount;
                });
            }
            else
            {
                SS2Log.Fatal("Failed to find IL match for Empyrean hook 2!");
            }
        }
        #endregion

        private static void CreateWhitelist(Run run)
        {
            whitelistedEliteDefs = new List<EliteDef>();
            try
            {
                string[] splitString = eliteDefEnabledStrings.Split(',');
                whitelistedEliteDefStrings = new List<string>(splitString);

                foreach (string name in whitelistedEliteDefStrings)
                {
                    EliteDef ed = GetEliteDefFromString(name);
                    if (ed != null && (!ed.eliteEquipmentDef.requiredExpansion || Run.instance.IsExpansionEnabled(ed.eliteEquipmentDef.requiredExpansion)))
                    {
                        whitelistedEliteDefs.Add(ed);
                    }
                }
            }
            catch (Exception e) // lol. lmao
            {
                SS2Log.Fatal("AffixEmpyrean.CreateWhitelist(): Failed to create whitelist. Using default.");
                SS2Log.Fatal(e);
                whitelistedEliteDefs = new List<EliteDef> { RoR2Content.Elites.Fire, RoR2Content.Elites.Ice, RoR2Content.Elites.Lightning, DLC1Content.Elites.Earth };
            }
        }

        public static EliteDef GetEliteDefFromString(string defString)
        {
            foreach (EliteDef ed in EliteCatalog.eliteDefs)
            {
                if (ed && ed.eliteEquipmentDef && ed.eliteEquipmentDef.name == defString)
                {
                    return ed;
                }
            }
            SS2Log.Error("Empyrean Whitelist: Could not find eliteEquipmentDef " + defString);
            return null;
        }
 

        public override bool Execute(EquipmentSlot slot)
        {
            return false;
        }
        
        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }
    }

    public sealed class AffixEmpyreanBehavior : BaseBuffBehaviour, IOnKilledServerReceiver
    {
        [BuffDefAssociation]
        private static BuffDef GetBuffDef() => SS2Content.Buffs.bdEmpyrean;
        private string ogSubtitle;
        private CharacterModel model;
        private SetStateOnHurt setStateOnHurt;
        private bool wasStun;
        private bool wasHitStun;
        private bool wasFrozen;

        protected override void Awake()
        {
            base.Awake();
            this.setStateOnHurt = base.GetComponent<SetStateOnHurt>();           
            ogSubtitle = characterBody.subtitleNameToken;
            model = characterBody.modelLocator.modelTransform.GetComponent<CharacterModel>();           
        }

        protected override void OnFirstStackGained()
        {
            base.OnFirstStackGained();

            foreach (EliteDef ed in EliteCatalog.eliteDefs)
            {
                //shitty hardcoded case for blighted; add actual cross compat later!
                if (ed.IsAvailable() && AffixEmpyrean.whitelistedEliteDefs.Contains(ed) && !characterBody.HasBuff(ed.eliteEquipmentDef?.passiveBuffDef))
                    characterBody.AddBuff(ed.eliteEquipmentDef.passiveBuffDef);
            }
            if (setStateOnHurt)
            {
                wasStun = setStateOnHurt.canBeStunned;
                setStateOnHurt.canBeStunned = false;
                wasHitStun = setStateOnHurt.canBeHitStunned;
                setStateOnHurt.canBeHitStunned = false;
                wasFrozen = setStateOnHurt.canBeFrozen;
                setStateOnHurt.canBeFrozen = false;
            }
            characterBody.subtitleNameToken = "SS2_ELITE_EMPYREAN_SUBTITLE";
        }
        protected override void OnAllStacksLost()
        {
            base.OnAllStacksLost();
            if (setStateOnHurt)
            {
                setStateOnHurt.canBeStunned = wasStun;
                setStateOnHurt.canBeHitStunned = wasHitStun;
                setStateOnHurt.canBeFrozen = wasFrozen;
            }
            foreach (EliteDef ed in EliteCatalog.eliteDefs)
            {
                if (characterBody.HasBuff(ed.eliteEquipmentDef.passiveBuffDef))
                    characterBody.RemoveBuff(ed.eliteEquipmentDef.passiveBuffDef);
            }

            characterBody.subtitleNameToken = ogSubtitle;
        }

        // item rewards are temporary
        public void OnKilledServer(DamageReport damageReport)
        {
            if (!hasAnyStacks) return; // this feels weird but /shrug

            if (!damageReport.attackerBody) return;

            if (characterBody.teamComponent.teamIndex == TeamIndex.Player) return;

            int numItems = this.characterBody.isChampion ? 2 : 1;
            float spreadAngle = 360f / numItems;
            float startingAngle = -(spreadAngle / 2) * (numItems - 1);
            for (int i = 0; i < numItems; i++)
            {
                float angle = startingAngle + i * spreadAngle;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * damageReport.victimBody.coreTransform.forward;
                if (numItems == 1) direction = Vector3.zero;
                Vector3 velocity = Vector3.up * 20f + direction * 10f;

                PickupIndex pickupIndex = RoR2.Artifacts.SacrificeArtifactManager.dropTable.GenerateDrop(RoR2.Artifacts.SacrificeArtifactManager.treasureRng);
                if (pickupIndex != PickupIndex.none)
                {
                    PickupDropletController.CreatePickupDroplet(pickupIndex, damageReport.victimBody.corePosition, velocity);
                }
            }

            if (Util.CheckRoll(8f))
            {
                // only pull an elite the empyrean has.
                EliteDef[] eliteDefs = EliteCatalog.eliteDefs.Where(x => x.eliteEquipmentDef && x.eliteEquipmentDef != SS2Content.Equipments.AffixEmpyrean && characterBody.HasBuff(x.eliteEquipmentDef.passiveBuffDef)).ToArray();
                int eliteIndex = Mathf.FloorToInt(UnityEngine.Random.Range(0, eliteDefs.Length));

                EquipmentIndex equipmentIndex = eliteDefs[eliteIndex].eliteEquipmentDef.equipmentIndex;
                PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(equipmentIndex);

                if (pickupIndex != PickupIndex.none)
                {
                    PickupDropletController.CreatePickupDroplet(pickupIndex, damageReport.victimBody.corePosition, Vector3.up * 20f);
                }


            }
        }
    }

}
