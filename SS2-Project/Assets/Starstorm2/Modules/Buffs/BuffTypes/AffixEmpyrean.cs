using HG;
using Moonstorm.Components;
using R2API;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine.AddressableAssets;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class AffixEmpyrean : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdEmpyrean", SS2Bundle.Equipments);

        public static List<EliteDef> blacklistedEliteDefs = new List<EliteDef>();

        public override Material OverlayMaterial => SS2Assets.LoadAsset<Material>("matRainbowOverlay", SS2Bundle.Equipments);
        public static void AddEliteToBlacklist(EliteDef eliteDef) => blacklistedEliteDefs.Add(eliteDef);


        public override void Initialize()
        {
            base.Initialize();
            On.RoR2.Util.GetBestBodyName += MakeEmpyreanName;
            RoR2Application.onLoad += CreateBlacklist;
            IL.RoR2.CharacterBody.RecalculateStats += RecalculateStatsEmpyreanIL;
        }

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
                //Debug.Log("Failed to find IL match for Empyrean hook 1!");
            }

            //Debug.Log(il + " :: IL DEBUGLOG");

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
                //Debug.Log("Failed to find IL match for Empyrean hook 2!");
            }
        }

        private static string MakeEmpyreanName(On.RoR2.Util.orig_GetBestBodyName orig, GameObject bodyObject)
        {
            var text = orig(bodyObject);
            var empyreanIndex = SS2Content.Buffs.bdEmpyrean.buffIndex;
            if (!bodyObject)
                return text;

            if (!bodyObject.TryGetComponent<CharacterBody>(out var body))
                return text;

            if (!body.HasBuff(empyreanIndex))
            {
                return text;
            }

            foreach (BuffIndex buffIndex in BuffCatalog.eliteBuffIndices)
            {
                if (buffIndex == empyreanIndex)
                    continue;

                var eliteToken = Language.GetString(BuffCatalog.GetBuffDef(buffIndex).eliteDef.modifierToken);
                eliteToken = eliteToken.Replace("{0}", string.Empty);
                text = text.Replace(eliteToken, string.Empty);
            }

            return text;
        }

        private static void CreateBlacklist()
        {
            AddEliteToBlacklist(RoR2Content.Elites.Lunar);
            AddEliteToBlacklist(DLC1Content.Elites.Void);
        }

        public sealed class Behavior : BaseBuffBodyBehavior, IOnKilledServerReceiver
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdEmpyrean;
            private string ogSubtitle;
            private CharacterModel model;
            private GameObject rainbowEffect;
            private SetStateOnHurt setStateOnHurt;
            private bool wasStun;
            private bool wasHitStun;
            private bool wasFrozen;
            private string empyreanToken = "SS2_ELITE_EMPYREAN_SUBTITLE";

            private void Start()
            {
                foreach (EliteDef ed in EliteCatalog.eliteDefs)
                {
                    //shitty hardcoded case for blighted; add actual cross compat later!
                    if (ed.IsAvailable() && !blacklistedEliteDefs.Contains(ed) && !body.HasBuff(ed.eliteEquipmentDef.passiveBuffDef) && ed.modifierToken != "LIT_MODIFIER_BLIGHTED")
                        body.AddBuff(ed.eliteEquipmentDef.passiveBuffDef);
                }

                this.setStateOnHurt = base.GetComponent<SetStateOnHurt>();
                if(setStateOnHurt)
                {
                    wasStun = setStateOnHurt.canBeStunned;
                    setStateOnHurt.canBeStunned = false;
                    wasHitStun = setStateOnHurt.canBeHitStunned;
                    setStateOnHurt.canBeHitStunned = false;
                    wasFrozen = setStateOnHurt.canBeFrozen;
                    setStateOnHurt.canBeFrozen = false;
                }
                ogSubtitle = body.subtitleNameToken;
                body.subtitleNameToken = empyreanToken;

                model = body.modelLocator.modelTransform.GetComponent<CharacterModel>();
                if (model != null)
                {
                    this.rainbowEffect = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("RainbowAffixEffect", SS2Bundle.Equipments), model.transform);
                    //if (model.currentOverlays.Contains<Material>(Addressables.LoadAssetAsync<Material>("RoR2/Base/EliteHaunted/matEliteHauntedOverlay.mat").WaitForCompletion()))
                    //    model.currentOverlays.RemoveIfInCollection(Addressables.LoadAssetAsync<Material>("RoR2/Base/EliteHaunted/matEliteHauntedOverlay.mat").WaitForCompletion());
                }
            }

            private void OnDestroy()
            {
                if (rainbowEffect) Destroy(this.rainbowEffect);
                if(setStateOnHurt)
                {
                    setStateOnHurt.canBeStunned = wasStun;
                    setStateOnHurt.canBeHitStunned = wasHitStun;
                    setStateOnHurt.canBeFrozen = wasFrozen;
                }
                foreach (EliteDef ed in EliteCatalog.eliteDefs)
                {
                    if (body.HasBuff(ed.eliteEquipmentDef.passiveBuffDef))
                        body.RemoveBuff(ed.eliteEquipmentDef.passiveBuffDef);
                }

                body.subtitleNameToken = ogSubtitle;
            }

            // item rewards are temporary
            public void OnKilledServer(DamageReport damageReport)
            {
                int numItems = this.body.isChampion ? 4 : 2;
                float spreadAngle = 360f / numItems;
                float startingAngle = -(spreadAngle / 2) * (numItems - 1);
                for(int i = 0; i < numItems; i++)
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

                if(Util.CheckRoll(20f))
                {
                    EliteDef[] eliteDefs = EliteCatalog.eliteDefs.Where(x => !blacklistedEliteDefs.Contains(x) && x.eliteEquipmentDef).ToArray();
                    int eliteIndex = Mathf.FloorToInt(UnityEngine.Random.Range(0, eliteDefs.Length));

                    EquipmentIndex equipmentIndex = eliteDefs[eliteIndex].eliteEquipmentDef.equipmentIndex;
                    PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(equipmentIndex);

                    if(pickupIndex != PickupIndex.none)
                    {
                        PickupDropletController.CreatePickupDroplet(pickupIndex, damageReport.victimBody.corePosition, Vector3.up * 20f);
                    }
                    

                }
            }
        }
    }
}
