﻿using MSU;
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

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Enabled Elite types, separated by commas. WARNING: MAY CAUSE GAME TO FAIL TO LOAD IF MIS-CONFIGURED.      Default: \"EliteFireEquipment,EliteIceEquipment,EliteLightningEquipment,EliteHauntedEquipment,ElitePoisonEquipment,EliteEarthEquipment\"")]
        public static string eliteDefEnabledStrings = "EliteFireEquipment,EliteIceEquipment,EliteLightningEquipment,EliteHauntedEquipment,ElitePoisonEquipment,EliteEarthEquipment";
        public static void AddEliteToWhitelist(EliteDef eliteDef) => whitelistedEliteDefs.Add(eliteDef);

        public override void Initialize()
        {
            //CreateWhitelist();
            //CreatePillarDecal();
            RoR2Application.onLoad += CreateWhitelist;
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

        private static void CreateWhitelist()
        {
            SS2Log.Debug("empy whitelist being made");

            string[] splitString = eliteDefEnabledStrings.Split(',');

            SS2Log.Debug("empy split strings made");

            whitelistedEliteDefStrings = new List<string>(splitString);

            SS2Log.Debug("empy split strings in list");

            foreach (string name in whitelistedEliteDefStrings)
            {
                SS2Log.Debug("looking for elite");
                EliteDef ed = GetEliteDefFromString(name);
                SS2Log.Debug("got ed from name");
                if (ed != null)
                {
                    whitelistedEliteDefs.Add(ed);
                    SS2Log.Debug("added " + ed.name + " to empy whitelist");
                }
            }
        }

        public static EliteDef GetEliteDefFromString(String defString)
        {
            SS2Log.Debug("elite def from string");
            foreach (EliteDef ed in EliteCatalog.eliteDefs)
            {
                if (ed.eliteEquipmentDef.name == defString)
                {
                    SS2Log.Debug("elite def from string found");
                    return ed;
                }
                else
                {
                    SS2Log.Debug("elite def from string failed, should be retrying..");
                }
            }

            SS2Log.Debug("Failed to find elite from string: " + defString);
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

        private void Start()
        {
            this.setStateOnHurt = base.GetComponent<SetStateOnHurt>();           
            ogSubtitle = CharacterBody.subtitleNameToken;
            model = CharacterBody.modelLocator.modelTransform.GetComponent<CharacterModel>();           
        }

        private void AddEliteBuffs()
        {

        }

        protected override void OnFirstStackGained()
        {
            base.OnFirstStackGained();

            foreach (EliteDef ed in EliteCatalog.eliteDefs)
            {
                //shitty hardcoded case for blighted; add actual cross compat later!
                if (ed.IsAvailable() && AffixEmpyrean.whitelistedEliteDefs.Contains(ed) && !CharacterBody.HasBuff(ed.eliteEquipmentDef?.passiveBuffDef))
                    CharacterBody.AddBuff(ed.eliteEquipmentDef.passiveBuffDef);
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
            CharacterBody.subtitleNameToken = "SS2_ELITE_EMPYREAN_SUBTITLE";
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
                if (CharacterBody.HasBuff(ed.eliteEquipmentDef.passiveBuffDef))
                    CharacterBody.RemoveBuff(ed.eliteEquipmentDef.passiveBuffDef);
            }

            CharacterBody.subtitleNameToken = ogSubtitle;
        }

        // item rewards are temporary
        public void OnKilledServer(DamageReport damageReport)
        {
            if (!HasAnyStacks) return; // this feels weird but /shrug

            if (!damageReport.attackerBody) return;

            if (CharacterBody.teamComponent.teamIndex != TeamIndex.Player)
            {
                var cedInstance = Components.CustomEliteDirector.instance;
                if (cedInstance.empyreanActive)
                    cedInstance.empyreanActive = false;
            }


            int numItems = this.CharacterBody.isChampion ? 4 : 2;
            float spreadAngle = 360f / numItems;
            float startingAngle = -(spreadAngle / 2) * (numItems - 1);
            for (int i = 0; i < numItems; i++)
            {
                float angle = startingAngle + i * spreadAngle;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * damageReport.victimBody.coreTransform.forward;
                Vector3 velocity = Vector3.up * 20f + direction * 10f;

                PickupIndex pickupIndex = RoR2.Artifacts.SacrificeArtifactManager.dropTable.GenerateDrop(RoR2.Artifacts.SacrificeArtifactManager.treasureRng);
                if (pickupIndex != PickupIndex.none)
                {
                    PickupDropletController.CreatePickupDroplet(pickupIndex, damageReport.victimBody.corePosition, velocity);
                }
            }

            if (Util.CheckRoll(20f))
            {
                // only pull an elite the empyrean has.
                EliteDef[] eliteDefs = EliteCatalog.eliteDefs.Where(x => x.eliteEquipmentDef && CharacterBody.HasBuff(x.eliteEquipmentDef.passiveBuffDef)).ToArray();
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
