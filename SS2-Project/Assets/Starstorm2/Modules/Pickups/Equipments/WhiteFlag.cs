using System;
using MSU;
using MSU.Config;
using RoR2;
using RoR2.Skills;
using System.Collections.Generic;
using System.Linq;
using SS2;
using SS2.Components;
using SS2.Equipments;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace SS2.Equipments
{
    public sealed class WhiteFlag : SS2Equipment, IContentPackModifier
    {
        private const string token = "SS2_EQUIP_WHITEFLAG_DESC";

        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<EquipmentAssetCollection>("acWhiteFlag", SS2Bundle.Equipments);

        private GameObject _flagObject;
        public static SkillDef disabledSkill;// SS2Assets.LoadAsset<SkillDef>("DisabledSkill", SS2Bundle.Items);
        
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Radius of the White Flag's effect, in meters.")]
        [FormatToken(token, 0)]
        public static float flagRadius = 25f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Duration of White Flag when used, in seconds.")]
        [FormatToken(token, 1)]
        public static float flagDuration = 15f;
        
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Enable year round pride.")]
        public static bool yearRoundPride = false;
        
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Pride overrides for survivors. Follows formatting \"BodyName,Flag\" with available options of \"nonbinary\", \"lesbian\", \"gay\", \"trans\", \"pansexual\", \"genderfluid\", \"asexual\", \"aromantic\" and \"bi\".")]
        public static string survivorPrideFlagOverrides = "";
        
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Pride overrides for specific steamids. Will override survivor overrides. Follows formatting \"Steamid,Flag\" with available options of \"nonbinary\", \"lesbian\", \"gay\", \"trans\", \"pansexual\", \"genderfluid\", \"asexual\", \"aromantic\" and \"bi\". Steamid must be in a style such as \"STEAM_0:1:174533492\".")]
        public static string steamidPrideFlagOverrides = "";
        
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Custom pride flags. Follows formatting \"FlagName,#Hex,#Hex\" seperated by \";\".")]
        public static string customFlags = "";
        
        public static readonly bool usePrideEdits = (yearRoundPride || DateTime.Now.Month == 6);

        public override bool Execute(EquipmentSlot slot)
        {            
            GameObject gameObject = Object.Instantiate(_flagObject, slot.characterBody.corePosition, Quaternion.identity);
            BuffWard buffWard = gameObject.GetComponent<BuffWard>();
            buffWard.expireDuration = flagDuration;
            buffWard.radius = flagRadius;
            gameObject.GetComponent<TeamFilter>().teamIndex = slot.teamComponent.teamIndex;
            
            if (usePrideEdits)
            {
                WhiteFlagWardPrider wardPrider = gameObject.GetComponent<WhiteFlagWardPrider>();
                
                if (wardPrider)
                {
                    wardPrider.flagType = slot.characterBody.master.gameObject.GetComponent<PrideFlagTypeStorer>().flagType;
                }
            }
            
            NetworkServer.Spawn(gameObject);
            return true;
        }

        public override void Initialize()
        {
            disabledSkill = SS2Assets.LoadAsset<SkillDef>("DisabledSkill", SS2Bundle.Base);
            BuffDef buffSurrender = AssetCollection.FindAsset<BuffDef>("BuffSurrender");
            _flagObject = AssetCollection.FindAsset<GameObject>("WhiteFlagWard");
            Material overlay = AssetCollection.FindAsset<Material>("matSurrenderOverlay");
            BuffOverlays.AddBuffOverlay(buffSurrender, overlay);
            
            //pride month .,,.
            if (usePrideEdits)
            {
                SS2Log.Info("its pride month nemesis commando ,.. you know what that means ,.,.,..");
                
                PrideHelper.AddFlag(SS2Assets.LoadAsset<Texture>("texWhiteFlagDiffuse", SS2Bundle.Equipments), new[] {Color.white});
                PrideHelper.AddFlag(SS2Assets.LoadAsset<Texture>("nonbinary", SS2Bundle.Equipments), new[] {PrideHelper.GetHex("#FCF434"), PrideHelper.GetHex("#FFFFFF"), PrideHelper.GetHex("#9C59D1"), PrideHelper.GetHex("#2C2C2C")});
                PrideHelper.AddFlag(SS2Assets.LoadAsset<Texture>("trans", SS2Bundle.Equipments), new[] {PrideHelper.GetHex("#20bbf8"), PrideHelper.GetHex("#ec5f7b"), PrideHelper.GetHex("#FFFFFF"), PrideHelper.GetHex("#ec5f7b"), PrideHelper.GetHex("#20bbf8")});
                PrideHelper.AddFlag(SS2Assets.LoadAsset<Texture>("lesbian", SS2Bundle.Equipments), new[] {PrideHelper.GetHex("#D52D00"), PrideHelper.GetHex("#EF7627"), PrideHelper.GetHex("#FF9A56"), PrideHelper.GetHex("#FFFFFF"), PrideHelper.GetHex("#D162A4"), PrideHelper.GetHex("#B55690"), PrideHelper.GetHex("#A30262")});
                PrideHelper.AddFlag(SS2Assets.LoadAsset<Texture>("bi", SS2Bundle.Equipments), new[] {PrideHelper.GetHex("#D60270"), PrideHelper.GetHex("#9B4F96"), PrideHelper.GetHex("#0038A8")});
                PrideHelper.AddFlag(SS2Assets.LoadAsset<Texture>("gay", SS2Bundle.Equipments), new[] {PrideHelper.GetHex("#E40303"), PrideHelper.GetHex("#FF8C00"), PrideHelper.GetHex("#FFED00"), PrideHelper.GetHex("#008026"), PrideHelper.GetHex("#004CFF"), PrideHelper.GetHex("#732982")});
                PrideHelper.AddFlag(SS2Assets.LoadAsset<Texture>("aromantic", SS2Bundle.Equipments), new[] {PrideHelper.GetHex("#3DA542"), PrideHelper.GetHex("#A7D379"), PrideHelper.GetHex("#FFFFFF"), PrideHelper.GetHex("#A9A9A9"), PrideHelper.GetHex("#000000")});
                PrideHelper.AddFlag(SS2Assets.LoadAsset<Texture>("genderfluid", SS2Bundle.Equipments), new[] {PrideHelper.GetHex("#FF76A4"), PrideHelper.GetHex("#FFFFFF"), PrideHelper.GetHex("#C011D7"), PrideHelper.GetHex("#000000"), PrideHelper.GetHex("#2F3CBE")});
                PrideHelper.AddFlag(SS2Assets.LoadAsset<Texture>("pansexual", SS2Bundle.Equipments), new[] {PrideHelper.GetHex("#FF218C"), PrideHelper.GetHex("#FFD800"), PrideHelper.GetHex("#21B1FF")});

                EquipmentDef.pickupIconSprite = SS2Assets.LoadAsset<Sprite>("texIconPickupPrideFlag", SS2Bundle.Equipments);
                
                // was a bit at a loss on how networking the display would go .,.,. figured either a dontdestroyonload object or a master component and since its just a silly event thing master probabl;y fine .,,. if you know a better way please yell at me 🥺 ,..,
                Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Core/PlayerMaster.prefab").Completed += handle => { handle.Result.AddComponent<PrideFlagTypeStorer>(); };

                PrideHelper.baseTex = (Texture2D)SS2Assets.LoadAsset<Texture>("texWhiteFlagDiffuse", SS2Bundle.Equipments);
                PrideHelper.texPixels = PrideHelper.baseTex.GetPixels(0, 0, PrideHelper.baseTex.width, PrideHelper.baseTex.height);
                foreach (string customFlag in customFlags.Split(";"))
                {
                    if (customFlag.Split(',').Length == 1) return;
                    
                    string[] flagColorString = customFlag.Split(',')[1..];
                    string flagName = customFlag.Split(',')[0];

                    Color[] flagColors = new Color[flagColorString.Length];
                    for (int i = 0; i < flagColorString.Length; i++)
                    {
                        flagColors[flagColorString.Length - 1 - i] = PrideHelper.GetHex(flagColorString[i].Trim());
                    }
                    
                    PrideHelper.AddFlagFromColors(flagName, flagColors);
                }
            }
        }

        //RoR2Application.OnLoad listener is too early for msu to process the config description, this seems to be good ,.. 
        [InitDuringStartupPhase(GameInitPhase.PostProgressBar)]
        private static void Init()
        {
            RenameWhiteFlag();
        }
        
        private static void RenameWhiteFlag()
        {
            if (!usePrideEdits) return;
            
            //this wont change with game config changes even with a listener on ModOptionPanelController.OnModOptionsExit since msu does a coroutine .,.,,. i think its fine since it only effects the desc and its like a silly single word change but istg someones going to open a github issue abt it <////3 .,,.
            List<KeyValuePair<string, string>> replacementTokens = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("SS2_EQUIP_WHITEFLAG_NAME", Language.GetString("SS2_EQUIP_WHITEFLAG_NAME").Replace("White", "Pride")),
                new KeyValuePair<string, string>("SS2_EQUIP_WHITEFLAG_PICKUP", Language.GetString("SS2_EQUIP_WHITEFLAG_PICKUP").Replace("white", "pride")),
                new KeyValuePair<string, string>("SS2_EQUIP_WHITEFLAG_DESC", Language.GetString("SS2_EQUIP_WHITEFLAG_DESC").Replace("white", "pride")),
            };
            
            Language.english.SetStringsByTokens(replacementTokens);
        }

        public override void OnEquipmentLost(CharacterBody CharacterBody)
        {
        }

        public override void OnEquipmentObtained(CharacterBody CharacterBody)
        {
        }

        public sealed class Behavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffSurrender;


            //captain is allowed to bomb mobs in the white flag zone because technically safe travels isnt in the zone :3
            private void OnEnable()
            {
                if (characterBody.skillLocator)
                {
                    GenericSkill primary = characterBody.skillLocator.primary;
                    if (primary && !(primary.skillDef is CaptainOrbitalSkillDef))// && !primary.skillDef.isCombatSkill)
                        primary.SetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    GenericSkill secondary = characterBody.skillLocator.secondary;
                    if (secondary && !(secondary.skillDef is CaptainOrbitalSkillDef))// && !secondary.skillDef.isCombatSkill)
                        secondary.SetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    GenericSkill utility = characterBody.skillLocator.utility;
                    if (utility && !(utility.skillDef is CaptainOrbitalSkillDef))// && !utility.skillDef.isCombatSkill)
                        utility.SetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    GenericSkill special = characterBody.skillLocator.special;
                    if (special && !(special.skillDef is CaptainOrbitalSkillDef))// && !special.skillDef.isCombatSkill)
                        special.SetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
                }
            }
            private void OnDisable()
            {
                if (characterBody.skillLocator)
                {
                    if (characterBody.skillLocator.primary)
                        characterBody.skillLocator.primary.UnsetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    if (characterBody.skillLocator.secondary)
                        characterBody.skillLocator.secondary.UnsetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    if (characterBody.skillLocator.utility)
                        characterBody.skillLocator.utility.UnsetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);

                    if (characterBody.skillLocator.special)
                        characterBody.skillLocator.special.UnsetSkillOverride(this, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
                }
            }
        }
    }
}

//pride month stuff ,.,. 
public static class PrideHelper
{
    public static List<Material> flagMaterials = new List<Material>();
    public static List<Color[]> flagColors = new List<Color[]>();
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    
    //magic numbers .,,.. ,.:nikodurr:,.,..,., (cordinates of where the white flag actually is on the texture(,.,.
    private const int flagXStart = 0;
    private const int flagXEnd = 621;
    private const int flagYStart = 103;
    private const int flagYEnd = 502;
    private const int total = flagYEnd - flagYStart;
    private const int textureHeight = 1024;
    public static Texture2D baseTex;
    public static Color[] texPixels;

    public static void AddFlag(Texture texture, Color[] colors)
    {
        flagColors.Add(colors);
        Material flagMat = Object.Instantiate(SS2Assets.LoadAsset<Material>("matWhiteFlag", SS2Bundle.Equipments));
        flagMat.SetTexture(MainTex, texture);
        flagMat.name = texture.name;
        flagMaterials.Add(flagMat);
    }

    public static void AddFlagFromColors(string flagName, Color[] colors)
    {
        Texture2D returnTexture = new Texture2D(baseTex.width, baseTex.height);
        Color[] instanceColors = new Color[texPixels.Length];
        Array.Copy(texPixels, instanceColors, texPixels.Length);
        
        for (int x = flagXStart; x < flagXEnd; x++)
        {
            for (int y = textureHeight - flagYEnd; y < textureHeight - flagYStart; y++)
            {
                for (int i = 1; i <= colors.Length; i++)
                {
                    if (y - (textureHeight - flagYEnd) < (total / colors.Length) * i)
                    {
                        Color newCol = colors[i - 1];
                        newCol.a = texPixels[x + (y * textureHeight)].a;
                        instanceColors[x + (y * textureHeight)] = newCol;
                        break;
                    }
                }

            }
        }

        returnTexture.SetPixels(instanceColors);
        returnTexture.Apply();
        returnTexture.name = flagName;

        PrideHelper.AddFlag(returnTexture, colors);
    }
    
    public static Material GetFlagMaterial(CharacterMaster master)
    {
        if (!master || !WhiteFlag.usePrideEdits)
        {
            return flagMaterials[0];
        }

        PrideFlagTypeStorer store = master.gameObject.GetComponent<PrideFlagTypeStorer>();
        return store == null ? flagMaterials[0] : flagMaterials[store.flagType];
    }
    
    public static Color[] GetFlagColors(int index)
    {
        return !WhiteFlag.usePrideEdits ? flagColors[0] : flagColors[index];
    }
    
    public static int GetFlagIndexFromName(string name)
    {
        for (int i = 0; i < flagMaterials.Count; i++)
        {
            if (flagMaterials[i].name != name) continue;
            return i;
        }

        return 0;
    }

    public static Color GetHex(string hex)
    {
        if (!hex.StartsWith("#"))
        {
            hex = "#" + hex;
        }
        
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;
    }
}

public class PrideFlagTypeStorer : NetworkBehaviour
{
    [SyncVar]
    public int flagType;

    public void OnEnable()
    {
        if (!NetworkServer.active) return;

        flagType = Run.instance.runRNG.RangeInt(1, PrideHelper.flagMaterials.Count);

        CharacterMaster master = gameObject.GetComponent<CharacterMaster>();
        
        string[] characterOverrides = WhiteFlag.survivorPrideFlagOverrides.Split(',').ToArray();

        string bodyName = master?.GetBody()?.name.Replace("(Clone)", "");
        int characterIndex = -1;
        for (int i = 0; i < characterOverrides.Length; i++)
        {
            if (characterOverrides[i] == bodyName && characterOverrides.Length != i + 1)
            {
                characterIndex = i;
            }
        }
        if (characterIndex != -1)
        {
            int flagIndex = PrideHelper.GetFlagIndexFromName(characterOverrides[characterIndex + 1]);
            if (flagIndex != -1)
            {
                flagType = flagIndex;
                SS2Log.Debug($"overriding flag to {characterOverrides[characterIndex + 1]} !!");
            }
        }
        
        string steamid = master?.playerCharacterMasterController?.networkUser?.id.steamId.ToSteamID();
        if (steamid != null)
        {
            string[] steamIds = WhiteFlag.steamidPrideFlagOverrides.Split(',').ToArray();
            int index = -1;
            for (int i = 0; i < steamIds.Length; i++)
            {
                //in my experience steam ids can be slightly different prior to the numbers .,,. just in case ! ,.
                if (steamIds[i].Split(":")[steamIds[i].Split(":").Length - 1] == steamid.Split(":")[steamid.Split(":").Length - 1] && steamIds.Length != i + 1)
                {
                    index = i;
                }
            }
            if (index != -1)
            {
                int flagIndex = PrideHelper.GetFlagIndexFromName(steamIds[characterIndex + 1]);
                if (flagIndex != -1)
                {
                    flagType = flagIndex;
                    SS2Log.Debug($"overriding flag to {steamIds[characterIndex + 1]} !!");
                }
            }
        }
        
        SS2Log.Debug($"set master flag type {PrideHelper.flagMaterials[flagType].name}");
    }
}