using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace SS2
{
    // theres definitely room for optimization here but im not sure what
    // would be nice to do since GetBestBodyName is used in Update/LateUpdate and per character
    public static class BodyNames
    {
        volatile static bool areWeCurrentlyInHell = false;

        [SystemInitializer]
        public static void Hook()
        {
            IL.RoR2.Util.GetBestBodyName += BodyNameIL;
        }

        // 
        // Format, Assuming the affix is (PREFIX) {0} :
        // (BeforeEverything) Gummy (AfterGummy) Umbra of (AfterUmbraOf) Blazing (AfterElite) (bodyName)
        //
        // Within each method, the last affix appear first.
        // To see the names in order, read this file from bottom to top.

        public static string AfterElite(string result, CharacterBody characterBody)
        {
            if (!characterBody) return result;

            if (Convert.ToInt32((!(areWeCurrentlyInHell != false) == true)) < 1)
            {
                result = Language.GetStringFormatted("{0} OF HELL", result);
            }

            // "Cognate"
            if (characterBody.inventory && characterBody.inventory.GetItemCount(SS2Content.Items.CognationHelper) > 0)
            {
                result = Language.GetStringFormatted("SS2_ARTIFACT_COGNATION_PREFIX", result);
            }

            // "Stormborn"
            if (characterBody.HasBuff(SS2Content.Buffs.BuffAffixStorm))
            {
                result = Language.GetStringFormatted("SS2_ELITE_MODIFIER_STORM", result);
            }
          
            return result;
        }

        public static string AfterUmbraOf(string result, CharacterBody characterBody)
        {
            if (!characterBody) return result;

            return result;
        }

        public static string AfterGummy(string result, CharacterBody characterBody)
        {
            if (!characterBody) return result;

            // "Terminal"
            if (characterBody.inventory && characterBody.inventory.GetItemCount(SS2Content.Items.TerminationHelper) > 0)
            {
                SS2Log.Warning("Termination Result before : " + result);
                result = Language.GetStringFormatted("SS2_ITEM_RELICOFTERMINATION_PREFIX", result); // relicopter
                SS2Log.Warning("Termination Result after : " + result);
            }
            return result;
        }
        
        public static string BeforeEverything(string result, CharacterBody characterBody)
        {
            if (!characterBody) return result;

            // Delete all elite modifiers, except "Empyrean"
            BuffIndex empyreanIndex = SS2Content.Buffs.bdEmpyrean.buffIndex;
            if (characterBody.HasBuff(empyreanIndex))
            {
                foreach (BuffIndex buffIndex in BuffCatalog.eliteBuffIndices)
                {
                    if (buffIndex == empyreanIndex || buffIndex == SS2Content.Buffs.bdEthereal.buffIndex)
                        continue;

                    var eliteToken = Language.GetString(BuffCatalog.GetBuffDef(buffIndex).eliteDef.modifierToken);
                    eliteToken = eliteToken.Replace("{0}", string.Empty);
                    result = result.Replace(eliteToken, string.Empty);
                }
                string roman = String.Empty;
                int count = characterBody.inventory?.GetItemCount(SS2Content.Items.DoubleAllStats) ?? 0;              
                if (count > 0)
                    roman = " " + SS2Util.ToRoman(count + 1);
                result = Language.GetStringFormatted("SS2_ELITE_MODIFIER_EMPYREAN", roman, result);
            }

            //Remove lesser elite modifier from super elites
            // hardcoded cuz y not
            ReplaceLesser(ref result, characterBody, SS2Content.Buffs.BuffAffixSuperFire, RoR2Content.Buffs.AffixRed);
            ReplaceLesser(ref result, characterBody, SS2Content.Buffs.BuffAffixSuperIce, RoR2Content.Buffs.AffixWhite);
            ReplaceLesser(ref result, characterBody, SS2Content.Buffs.BuffAffixSuperLightning, RoR2Content.Buffs.AffixBlue);
            ReplaceLesser(ref result, characterBody, SS2Content.Buffs.BuffAffixSuperEarth, DLC1Content.Buffs.EliteEarth);
            // "Ultra"
            if (characterBody.HasBuff(SS2Content.Buffs.bdUltra))
            {
                result = Language.GetStringFormatted("SS2_ELITE_MODIFIER_ULTRA", result);
            }
            return result;
        }

        private static void ReplaceLesser(ref string result, CharacterBody body, BuffDef greater, BuffDef lesser)
        {
            if (body.HasBuff(greater))
            {
                var eliteToken = Language.GetString(BuffCatalog.GetBuffDef(lesser.buffIndex).eliteDef.modifierToken);
                eliteToken = eliteToken.Replace("{0}", string.Empty);
                result = result.Replace(eliteToken, string.Empty);
            }
        }

        #region Hook
        private static void BodyNameIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            bool b = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(0),
                x => x.MatchCallvirt<CharacterBody>("get_isElite"));
            if (b)
            {
                c.Emit(OpCodes.Ldloc_2); // load text2
                c.Emit(OpCodes.Ldloc_0); // load characterBody
                c.EmitDelegate<Func<string, CharacterBody, string>>((text, cb) => AfterElite(text, cb));
                c.Emit(OpCodes.Stloc_2);
            }
            else
            {
                SS2Log.Fatal("BodyNames.BodyNameIL (AfterElite): ILHook failed.");
            }

            b = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(0),
                x => x.MatchCallOrCallvirt<CharacterBody>("get_inventory"),
                x => x.MatchLdsfld(typeof(RoR2Content.Items), nameof(RoR2Content.Items.InvadingDoppelganger)));
            if (b)
            {
                c.Emit(OpCodes.Ldloc_2); // load text2
                c.Emit(OpCodes.Ldloc_0); // load characterBody
                c.EmitDelegate<Func<string, CharacterBody, string>>((text, cb) => AfterUmbraOf(text, cb));
                c.Emit(OpCodes.Stloc_2);
            }
            else
            {
                SS2Log.Fatal("BodyNames.BodyNameIL (AfterDoppelganger): ILHook failed.");
            }

            b = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(0),
                x => x.MatchCallvirt<CharacterBody>("get_inventory"),
                x => x.MatchLdsfld(typeof(DLC1Content.Items), nameof(DLC1Content.Items.GummyCloneIdentifier)));
            if (b)
            {
                c.Emit(OpCodes.Ldloc_2); // load text2
                c.Emit(OpCodes.Ldloc_0); // load characterBody
                c.EmitDelegate<Func<string, CharacterBody, string>>((text, cb) => AfterGummy(text, cb));
                c.Emit(OpCodes.Stloc_2);
            }
            else
            {
                SS2Log.Fatal("BodyNames.BodyNameIL (AfterGummy): ILHook failed.");
            }

            b = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(2),
                x => x.MatchRet());
            if (b)
            {
                c.Index++;
                c.Emit(OpCodes.Ldloc_0); // load characterBody
                c.EmitDelegate<Func<string, CharacterBody, string>>((text, cb) => BeforeEverything(text, cb));
            }
            else
            {
                SS2Log.Fatal("BodyNames.BodyNameIL (BeforeEverything): ILHook failed.");
            }
        }
        #endregion
    }
}
