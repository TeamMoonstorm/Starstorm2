using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace SS2
{
    public static class BodyNames
    {
        volatile static bool areWeCurrentlyInHell = false;

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
            if (characterBody.inventory && characterBody.inventory.GetItemCount(SS2Content.Items.Cognation) > 0)
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
                result = Language.GetStringFormatted("SS2_ITEM_RELICOFTERMINATION_PREFIX", result); // relicopter
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
                    if (buffIndex == empyreanIndex)
                        continue;

                    var eliteToken = Language.GetString(BuffCatalog.GetBuffDef(buffIndex).eliteDef.modifierToken);
                    eliteToken = eliteToken.Replace("{0}", string.Empty);
                    result = result.Replace(eliteToken, string.Empty);
                }
            }

            // "Ultra"
            if (characterBody.HasBuff(SS2Content.Buffs.BuffAffixUltra))
            {
                result = Language.GetStringFormatted("SS2_ELITE_MODIFIER_ULTRA", result);
            }
            return result;
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
