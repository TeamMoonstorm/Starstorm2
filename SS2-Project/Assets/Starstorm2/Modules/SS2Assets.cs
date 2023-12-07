using Moonstorm.Loaders;
using Moonstorm.Starstorm2.PostProcess;
using RoR2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.PostProcessing;
using Path = System.IO.Path;

namespace Moonstorm.Starstorm2
{
    public enum SS2Bundle
    {
        Invalid,
        All,
        Main,
        Base,
        Artifacts,
        Executioner,
        Executioner2,
        Chirr,
        Nemmando,
        NemCommando,
        NemMercenary,
        Equipments,
        Items,
        Events,
        Stages,
        VoidShop,
        Vanilla,
        Indev,
        Interactables,
        Monsters,
        Shared
    }
    public class SS2Assets : AssetsLoader<SS2Assets>
    {
        private const string ASSET_BUNDLE_FOLDER_NAME = "assetbundles";
        private const string MAIN = "ss2main";
        private const string BASE = "ss2base";
        private const string ARTIFACTS = "ss2artifacts";
        private const string EXECUTIONER = "ss2executioner";
        private const string EXECUTIONER2 = "ss2executioner2";
        private const string CHIRR = "ss2chirr";
        private const string NEMMANDO = "ss2nemmando";
        private const string NEMCOMMANDO = "ss2nemcommando";
        private const string NEMMERCENARY = "ss2nemmercenary";
        private const string EQUIPS = "ss2equipments";
        private const string ITEMS = "ss2items";
        private const string EVENTS = "ss2events";
        private const string STAGES = "ss2stages";
        private const string VANILLA = "ss2vanilla";
        private const string DEV = "ss2dev";
        private const string INTERACTABLES = "ss2interactables";
        private const string MONSTERS = "ss2monsters";
        private const string SHARED = "ss2shared";
        

        private static Dictionary<SS2Bundle, AssetBundle> assetBundles = new Dictionary<SS2Bundle, AssetBundle>();
        private static AssetBundle[] streamedSceneBundles = Array.Empty<AssetBundle>();

        [Obsolete("LoadAsset should not be used without specifying the SS2Bundle")]
        public new static TAsset LoadAsset<TAsset>(string name) where TAsset : UnityEngine.Object
        {
#if DEBUG
            SS2Log.Warning($"Method {GetCallingMethod()} is trying to load an asset of name {name} and type {typeof(TAsset).Name} without specifying what bundle to use for loading. This causes large performance loss as SS2Assets has to search thru the entire bundle collection. Avoid calling LoadAsset without specifying the AssetBundle.");
#endif
            return LoadAsset<TAsset>(name, SS2Bundle.All);
        }
        [Obsolete("LoadAllAssetsOfType should not be used without specifying the SS2Bundle")]
        public new static TAsset[] LoadAllAssetsOfType<TAsset>() where TAsset : UnityEngine.Object
        {
#if DEBUG
            SS2Log.Warning($"Method {GetCallingMethod()} is trying to load all assets of type {typeof(TAsset).Name} without specifying what bundle to use for loading. This causes large performance loss as SS2Assets has to search thru the entire bundle collection. Avoid calling LoadAsset without specifying the AssetBundle.");
#endif
            return LoadAllAssetsOfType<TAsset>(SS2Bundle.All);
        } 
        public static TAsset LoadAsset<TAsset>(string name, SS2Bundle bundle) where TAsset : UnityEngine.Object
        {
            if(Instance == null)
            {
                SS2Log.Error("Cannot load asset when there's no instance of SS2Assets!");
                return null;
            }
            return Instance.LoadAssetInternal<TAsset>(name, bundle);
        }
        public static TAsset[] LoadAllAssetsOfType<TAsset>(SS2Bundle bundle) where TAsset : UnityEngine.Object
        {
            if(Instance == null)
            {
                SS2Log.Error("Cannot load asset when there's no instance of SS2Assets!");
                return null;
            }
            return Instance.LoadAllAssetsOfTypeInternal<TAsset>(bundle);
        }

#if DEBUG
        private static string GetCallingMethod()
        {
            var stackTrace = new StackTrace();

            for(int stackFrameIndex = 0; stackFrameIndex < stackTrace.FrameCount; stackFrameIndex++)
            {
                var frame = stackTrace.GetFrame(stackFrameIndex);
                var method = frame.GetMethod();
                
                if (method == null)
                    continue;

                var declaringType = method.DeclaringType;
                if (declaringType == typeof(SS2Assets))
                    continue;

                var fileName = frame.GetFileName();
                var fileLineNumber = frame.GetFileLineNumber();
                var fileColumnNumber = frame.GetFileColumnNumber();

                return $"{declaringType.FullName}.{method.Name}({GetMethodParams(method)}) (fileName: {fileName}, Location: L{fileLineNumber} C{fileColumnNumber})";
            }

            return "[COULD NOT GET CALLING METHOD]";
        }

        private static string GetMethodParams(MethodBase methodBase)
        {
            var parameters = methodBase.GetParameters();
            if (parameters.Length == 0)
                return string.Empty;

            StringBuilder stringBuilder = new StringBuilder();
            foreach(var parameter in parameters)
            {
                stringBuilder.Append(parameter.ToString() + ", ");
            }
            return stringBuilder.ToString();
        }
#endif
        public override AssetBundle MainAssetBundle => GetAssetBundle(SS2Bundle.Main);
        public string AssemblyDir => Path.GetDirectoryName(Starstorm.pluginInfo.Location);
        public AssetBundle GetAssetBundle(SS2Bundle bundle)
        {
            return assetBundles[bundle];
        }
        internal void Init()
        {
            var bundlePaths = GetAssetBundlePaths();
            foreach(string path in bundlePaths)
            {
                var fileName = Path.GetFileName(path);
                switch(fileName)
                {
                    case MAIN: LoadAndAssign(path, SS2Bundle.Main); break;
                    case BASE: LoadAndAssign(path, SS2Bundle.Base); break;
                    case ARTIFACTS: LoadAndAssign(path, SS2Bundle.Artifacts); break;
                    case EXECUTIONER: LoadAndAssign(path, SS2Bundle.Executioner); break;
                    case EXECUTIONER2: LoadAndAssign(path, SS2Bundle.Executioner2); break;
                    case CHIRR: LoadAndAssign(path, SS2Bundle.Chirr); break;
                    case NEMMANDO: LoadAndAssign(path, SS2Bundle.Nemmando); break;
                    case NEMCOMMANDO: LoadAndAssign(path, SS2Bundle.NemCommando); break;
                    case NEMMERCENARY: LoadAndAssign(path, SS2Bundle.NemMercenary); break;
                    case EQUIPS: LoadAndAssign(path, SS2Bundle.Equipments); break;
                    case ITEMS: LoadAndAssign(path, SS2Bundle.Items); break;
                    case EVENTS: LoadAndAssign(path, SS2Bundle.Events); break;
                    case STAGES: LoadAndAssign(path, SS2Bundle.Stages); break;
                    case VANILLA: LoadAndAssign(path, SS2Bundle.Vanilla); break;
                    case INTERACTABLES: LoadAndAssign(path, SS2Bundle.Interactables); break;
                    case MONSTERS: LoadAndAssign(path, SS2Bundle.Monsters); break;
                    case DEV: LoadAndAssign(path, SS2Bundle.Indev); break;
                    case SHARED: LoadAndAssign(path, SS2Bundle.Shared); break;
                    default:
                        {
                            try
                            {
                                var ab = AssetBundle.LoadFromFile(path);
                                if (!ab)
                                {
                                    throw new FileLoadException($"AssetBundle.LoadFromFile did not return an asset bundle. (Path:{path} FileName:{fileName})");
                                }
                                if (!ab.isStreamedSceneAssetBundle)
                                {
                                    throw new Exception($"AssetBundle is not a streamed scene bundle, but it's file name was not found on the Switch statement. (Path:{path} FileName:{fileName})");
                                }
                                else
                                {
                                    HG.ArrayUtils.ArrayAppend(ref streamedSceneBundles, ab);
                                }
                                SS2Log.Warning($"Invalid or Unexpected file in the AssetBundles folder (File name: {fileName}, Path: {path})");
                            }
                            catch (Exception e)
                            {
                                SS2Log.Error($"Default statement on bundle loading method hit, Exception thrown.\n{e}");
                            }
                            break;
                        }
                }
            }

            void LoadAndAssign(string path, SS2Bundle bundleEnum)
            {
                try
                {
                    AssetBundle bundle = AssetBundle.LoadFromFile(path);
                    if(!bundle)
                    {
                        throw new FileLoadException("AssetBundle.LoadFromFile did not return an asset bundle");
                    }
                    if(assetBundles.ContainsKey(bundleEnum))
                    {
                        throw new InvalidOperationException($"AssetBundle in path loaded succesfully, but the assetBundles dictionary already contains an entry for {bundleEnum}.");
                    }

                    assetBundles[bundleEnum] = bundle;
                }
                catch(Exception e)
                {
                    SS2Log.Error($"Could not load assetbundle at path {path} and assign to enum {bundleEnum}. {e}");
                }
            }
        }

        private TAsset LoadAssetInternal<TAsset>(string name, SS2Bundle bundle) where TAsset : UnityEngine.Object
        {
            TAsset asset = null;
            if(bundle == SS2Bundle.All)
            {
                asset = FindAsset<TAsset>(name, out SS2Bundle foundInBundle);
#if DEBUG
                if(!asset)
                {
                    SS2Log.Warning($"Could not find asset of type {typeof(TAsset).Name} with name {name} in any of the bundles.");
                }
                else
                {
                    SS2Log.Info($"Asset of type {typeof(TAsset).Name} was found inside bundle {foundInBundle}, it is recommended that you load the asset directly");
                }
#endif
                return asset;
            }

            asset = assetBundles[bundle].LoadAsset<TAsset>(name);
#if DEBUG
            if(!asset)
            {
                SS2Log.Warning($"The  method \"{GetCallingMethod()}\" is calling \"LoadAsset<TAsset>(string, SS2Bundle)\" with the arguments \"{typeof(TAsset).Name}\", \"{name}\" and \"{bundle}\", however, the asset could not be found.\n" +
                    $"A complete search of all the bundles will be done and the correct bundle enum will be logged.");
                return LoadAssetInternal<TAsset>(name, SS2Bundle.All);
            }
#endif
            return asset;

            TAsset FindAsset<TAsset>(string assetName, out SS2Bundle foundInBundle) where TAsset : UnityEngine.Object
            {
                foreach((var enumVal, var assetBundle) in assetBundles)
                {
                    var loadedAsset = assetBundle.LoadAsset<TAsset>(assetName);
                    if (loadedAsset)
                    {
                        foundInBundle = enumVal;
                        return loadedAsset;
                    }
                }
                foundInBundle = SS2Bundle.Invalid;
                return null;
            }
        }

        private TAsset[] LoadAllAssetsOfTypeInternal<TAsset>(SS2Bundle bundle) where TAsset : UnityEngine.Object
        {
            List<TAsset> loadedAssets = new List<TAsset>();
            if(bundle == SS2Bundle.All)
            {
                FindAssets<TAsset>(loadedAssets);
#if DEBUG
                if(loadedAssets.Count == 0)
                {
                    SS2Log.Warning($"Could not find any asset of type {typeof(TAsset)} inside any of the bundles");
                }
#endif
                return loadedAssets.ToArray();
            }

            loadedAssets = assetBundles[bundle].LoadAllAssets<TAsset>().ToList();
#if DEBUG
            if (loadedAssets.Count == 0)
            {
                SS2Log.Warning($"Could not find any asset of type {typeof(TAsset)} inside the bundle {bundle}");
            }
#endif
            return loadedAssets.ToArray();

            void FindAssets<TAsset>(List<TAsset> output) where TAsset: UnityEngine.Object
            {
                foreach((var _, var bndl) in assetBundles)
                {
                    output.AddRange(bndl.LoadAllAssets<TAsset>());
                }
                return;
            }
        }

        internal void SwapMaterialShaders()
        {
            SwapShadersFromMaterials(LoadAllAssetsOfType<Material>(SS2Bundle.All).Where(mat => mat.shader.name.StartsWith("Stubbed")));
        }

        internal void FinalizeCopiedMaterials()
        {
            foreach(var (_, bundle) in assetBundles)
            {
                FinalizeMaterialsWithAddressableMaterialShader(bundle);
            }
        }

        private string[] GetAssetBundlePaths()
        {
            return Directory.GetFiles(Path.Combine(AssemblyDir, ASSET_BUNDLE_FOLDER_NAME))
               .Where(filePath => !filePath.EndsWith(".manifest"))
               .ToArray();
        }

        //Not the most pleasant workaround but that's what we get
        //private static PostProcessProfile[] ppProfiles;
        private void LoadPostProcessing()
        {
            var ppProfiles = LoadAllAssetsOfType<PostProcessProfile>(SS2Bundle.All);
            foreach (var ppProfile in ppProfiles)
            {
                SS2Log.Error(ppProfile);
                bool modified = false;
                if (ppProfile.TryGetSettings(out SS2RampFog tempFog))
                {
                    var fog = ppProfile.AddSettings<RampFog>();
                    fog.enabled = tempFog.enabled;
                    fog.active = tempFog.active;
                    fog.fogIntensity = tempFog.fogIntensity;
                    fog.fogPower = tempFog.fogPower;
                    fog.fogZero = tempFog.fogZero;
                    fog.fogOne = tempFog.fogOne;
                    fog.fogHeightStart = tempFog.fogHeightStart;
                    fog.fogHeightEnd = tempFog.fogHeightEnd;
                    fog.fogHeightIntensity = tempFog.fogHeightIntensity;
                    fog.fogColorStart = tempFog.fogColorStart;
                    fog.fogColorMid = tempFog.fogColorMid;
                    fog.fogColorEnd = tempFog.fogColorEnd;
                    fog.skyboxStrength = tempFog.skyboxStrength;
                    ppProfile.RemoveSettings(typeof(SS2RampFog));
                    modified = true;
                }
                if (ppProfile.TryGetSettings(out SS2SobelOutline tempOutline))
                {
                    var outline = ppProfile.AddSettings<SobelOutline>();
                    outline.enabled = tempOutline.enabled;
                    outline.active = tempOutline.active;
                    outline.outlineIntensity = tempOutline.outlineIntensity;
                    outline.outlineScale = tempOutline.outlineScale;
                    ppProfile.RemoveSettings(typeof(SS2SobelOutline));
                    modified = true;
                }
                if (ppProfile.TryGetSettings(out SS2SobelRain tempRain))
                {
                    var rain = ppProfile.AddSettings<SobelRain>();
                    rain.enabled = tempRain.enabled;
                    rain.active = tempRain.active;
                    rain.rainIntensity = tempRain.rainIntensity;
                    rain.outlineScale = tempRain.outlineScale;
                    rain.rainDensity = tempRain.rainDensity;
                    rain.rainTexture = tempRain.rainTexture;
                    rain.rainColor = tempRain.rainColor;
                    ppProfile.RemoveSettings(typeof(SS2SobelRain));
                    modified = true;
                }
                ppProfile.isDirty = modified;
            }
        }
    }
}