using SS2.PostProcess;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using MSU;
using UObject = UnityEngine.Object;
using Path = System.IO.Path;
using System.Collections;
using SS2.Survivors;

namespace SS2
{
    public enum SS2Bundle
    {
        Invalid,
        All,
        Main,
        Base,
        Artifacts,
        Executioner2,
        Chirr,
        NemCommando,
        NemMercenary,
        Equipments,
        Items,
        Events,
        SharedStages,
        Vanilla,
        Indev,
        Interactables,
        Monsters,
        Shared
    }

    public static class SS2Assets
    {
        private const string ASSET_BUNDLE_FOLDER_NAME = "assetbundles";
        private const string MAIN = "ss2main";
        private const string BASE = "ss2base";
        private const string ARTIFACTS = "ss2artifacts";
        private const string EXECUTIONER2 = "ss2executioner2";
        private const string CHIRR = "ss2chirr";
        private const string NEMCOMMANDO = "ss2nemcommando";
        private const string NEMMERCENARY = "ss2nemmercenary";
        private const string EQUIPS = "ss2equipments";
        private const string ITEMS = "ss2items";
        private const string EVENTS = "ss2events";
        private const string SHARED_STAGES = "ss2stages";
        private const string VANILLA = "ss2vanilla";
        private const string DEV = "ss2dev";
        private const string INTERACTABLES = "ss2interactables";
        private const string MONSTERS = "ss2monsters";
        private const string SHARED = "ss2shared";

        private static string AssetBundleFolderPath => Path.Combine(Path.GetDirectoryName(SS2Main.Instance.Info.Location), ASSET_BUNDLE_FOLDER_NAME);

        private static Dictionary<SS2Bundle, AssetBundle> _assetBundles = new Dictionary<SS2Bundle, AssetBundle>();
        private static AssetBundle[] _streamedSceneBundles = Array.Empty<AssetBundle>();

        public static event Action OnSS2AssetsInitialized
        {
            add
            {
                _onSS2AssetsInitialized -= value;
                _onSS2AssetsInitialized += value;
            }
            remove
            {
                _onSS2AssetsInitialized -= value;
            }
        }
        private static Action _onSS2AssetsInitialized;

        public static AssetBundle GetAssetBundle(SS2Bundle bundle)
        {
            return _assetBundles[bundle];
        }

        public static TAsset LoadAsset<TAsset>(string name, SS2Bundle bundle) where TAsset : UObject
        {
            TAsset asset = null;
            if (bundle == SS2Bundle.All)
            {
                return FindAsset<TAsset>(name);
            }

            asset = _assetBundles[bundle].LoadAsset<TAsset>(name);

#if DEBUG
            if (!asset)
            {
                SS2Log.Warning($"The method \"{GetCallingMethod()}\" is calling \"LoadAsset<TAsset>(string, CommissionBundle)\" with the arguments \"{typeof(TAsset).Name}\", \"{name}\" and \"{bundle}\", however, the asset could not be found.\n" +
                    $"A complete search of all the bundles will be done and the correct bundle enum will be logged.");

                return LoadAsset<TAsset>(name, SS2Bundle.All);
            }
#endif
            return asset;
        }

        public static SS2AssetRequest<TAsset> LoadAssetAsync<TAsset>(string name, SS2Bundle bundle) where TAsset : UObject
        {
            return new SS2AssetRequest<TAsset>(name, bundle);
        }

        public static TAsset[] LoadAllAssets<TAsset>(SS2Bundle bundle) where TAsset : UObject
        {
            TAsset[] loadedAssets = null;
            if (bundle == SS2Bundle.All)
            {
                return FindAssets<TAsset>();
            }
            loadedAssets = _assetBundles[bundle].LoadAllAssets<TAsset>();

#if DEBUG
            if (loadedAssets.Length == 0)
            {
                SS2Log.Warning($"Could not find any asset of type {typeof(TAsset).Name} inside the bundle {bundle}");
            }
#endif
            return loadedAssets;
        }

        public static SS2AssetRequest<TAsset> LoadAllAssetsAsync<TAsset>(SS2Bundle bundle) where TAsset : UObject
        {
            return new SS2AssetRequest<TAsset>(bundle);
        }

        internal static IEnumerator Initialize()
        {
            SS2Log.Info($"Initializing Assets...");

            var loadRoutine = LoadAssetBundles();
            while (loadRoutine.MoveNext()) yield return null;

            ParallelMultiStartCoroutine helper = new ParallelMultiStartCoroutine();
            helper.Add(SwapShaders);
            helper.Add(SwapAddressableShaders);

            helper.Start();
            while (!helper.IsDone()) yield return null;

            _onSS2AssetsInitialized?.Invoke();
            yield break;
        }

        private static IEnumerator LoadAssetBundles()
        {
            ParallelMultiStartCoroutine helper = new ParallelMultiStartCoroutine();

            List<(string path, SS2Bundle bundleEnum, AssetBundle loadedBundle)> pathsAndBundles = new List<(string path, SS2Bundle bundleEnum, AssetBundle loadedBundle)>();

            string[] paths = GetAssetBundlePaths();
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];
                helper.Add(LoadFromPath, pathsAndBundles, path, i, paths.Length);
            }

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            foreach ((string path, SS2Bundle bundleEnum, AssetBundle assetBundle) in pathsAndBundles)
            {
                if (bundleEnum == SS2Bundle.Invalid)
                {
                    HG.ArrayUtils.ArrayAppend(ref _streamedSceneBundles, assetBundle);
                }
                else
                {
                    _assetBundles[bundleEnum] = assetBundle;
                }
            }
        }

        private static IEnumerator LoadFromPath(List<(string path, SS2Bundle bundleEnum, AssetBundle loadedBundle)> list, string path, int index, int totalPaths)
        {
            string fileName = Path.GetFileName(path);
            SS2Bundle? commissionBundleEnum = null;
            switch (fileName)
            {
                case MAIN: commissionBundleEnum = SS2Bundle.Main; break;
                case BASE: commissionBundleEnum = SS2Bundle.Base; break;
                case ARTIFACTS: commissionBundleEnum = SS2Bundle.Artifacts; break;
                case EXECUTIONER2: commissionBundleEnum = SS2Bundle.Executioner2; break;
                case CHIRR: commissionBundleEnum = SS2Bundle.Chirr; break;
                case NEMCOMMANDO: commissionBundleEnum = SS2Bundle.NemCommando; break;
                case NEMMERCENARY: commissionBundleEnum = SS2Bundle.NemMercenary; break;
                case EQUIPS: commissionBundleEnum = SS2Bundle.Equipments; break;
                case ITEMS: commissionBundleEnum = SS2Bundle.Items; break;
                case EVENTS: commissionBundleEnum = SS2Bundle.Events; break;
                case SHARED_STAGES: commissionBundleEnum = SS2Bundle.SharedStages; break;
                case VANILLA: commissionBundleEnum = SS2Bundle.Vanilla; break;
                case INTERACTABLES: commissionBundleEnum = SS2Bundle.Interactables; break;
                case MONSTERS: commissionBundleEnum = SS2Bundle.Monsters; break;
                case DEV: commissionBundleEnum = SS2Bundle.Indev; break;
                case SHARED: commissionBundleEnum = SS2Bundle.Shared; break;
                //This path does not match any of the non scene bundles, could be a scene, we will mark these on only this ocassion as "Invalid".
                default: commissionBundleEnum = SS2Bundle.Invalid; break;
            }

            var request = AssetBundle.LoadFromFileAsync(path);
            while (!request.isDone)
            {
                yield return null;
            }

            AssetBundle bundle = request.assetBundle;

            //Throw if no bundle was loaded
            if (!bundle)
            {
                throw new FileLoadException($"AssetBundle.LoadFromFile did not return an asset bundle. (Path={path})");
            }

            //The switch statement considered this a streamed scene bundle
            if (commissionBundleEnum == SS2Bundle.Invalid)
            {
                //supposed bundle is not streamed scene? throw exception.
                if (!bundle.isStreamedSceneAssetBundle)
                {
                    throw new Exception($"AssetBundle in specified path is not a streamed scene bundle, but its file name was not found in the Switch statement. have you forgotten to setup the enum and file name in your assets class? (Path={path})");
                }
                else
                {
                    //bundle is streamed scene, add to the list and break.
                    list.Add((path, SS2Bundle.Invalid, bundle));
                    yield break;
                }
            }

            //The switch statement considered this to not be a streamed scene bundle, but an assets bundle.
            list.Add((path, commissionBundleEnum.Value, bundle));
            yield break;
        }

        private static IEnumerator SwapShaders()
        {
            return ShaderUtil.SwapStubbedShadersAsync(_assetBundles.Values.ToArray());
        }

        private static IEnumerator SwapAddressableShaders()
        {
            return ShaderUtil.LoadAddressableMaterialShadersAsync(_assetBundles.Values.ToArray());
        }

        private static TAsset FindAsset<TAsset>(string name) where TAsset : UnityEngine.Object
        {
            TAsset loadedAsset = null;
            SS2Bundle foundInBundle = SS2Bundle.Invalid;
            foreach ((var enumVal, var assetBundle) in _assetBundles)
            {
                loadedAsset = assetBundle.LoadAsset<TAsset>(name);

                if (loadedAsset)
                {
                    foundInBundle = enumVal;
                    break;
                }
            }

#if DEBUG
            if (loadedAsset)
                SS2Log.Info($"Asset of type {typeof(TAsset).Name} with name {name} was found inside bundle {foundInBundle}, it is recommended that you load the asset directly.");
            else
                SS2Log.Warning($"Could not find asset of type {typeof(TAsset).Name} with name {name} in any of the bundles.");
#endif

            return loadedAsset;
        }

        private static TAsset[] FindAssets<TAsset>() where TAsset : UnityEngine.Object
        {
            List<TAsset> assets = new List<TAsset>();
            foreach ((_, var bundles) in _assetBundles)
            {
                assets.AddRange(bundles.LoadAllAssets<TAsset>());
            }

#if DEBUG
            if (assets.Count == 0)
                SS2Log.Warning($"Could not find any asset of type {typeof(TAsset).Name} in any of the bundles");
#endif

            return assets.ToArray();
        }

        private static string[] GetAssetBundlePaths()
        {
            return Directory.GetFiles(AssetBundleFolderPath).Where(filePath => !filePath.EndsWith(".manifest")).ToArray();
        }

#if DEBUG
        private static string GetCallingMethod()
        {
            var stackTrace = new StackTrace();

            for (int stackFrameIndex = 0; stackFrameIndex < stackTrace.FrameCount; stackFrameIndex++)
            {
                var frame = stackTrace.GetFrame(stackFrameIndex);
                var method = frame.GetMethod();
                if (method == null)
                    continue;

                var declaringType = method.DeclaringType;
                if (declaringType.IsGenericType && declaringType.DeclaringType == typeof(SS2Assets))
                    continue;

                if (declaringType == typeof(SS2Assets))
                    continue;

                var fileName = frame.GetFileName();
                var fileLineNumber = frame.GetFileLineNumber();
                var fileColumnNumber = frame.GetFileColumnNumber();

                return $"{declaringType.FullName}.{method.Name}({GetMethodParams(method)}) (fileName={fileName}, Location=L{fileLineNumber} C{fileColumnNumber})";
            }
            return "[COULD NOT GET CALLING METHOD]";
        }

        private static string GetMethodParams(MethodBase methodBase)
        {
            var parameters = methodBase.GetParameters();
            if (parameters.Length == 0)
                return string.Empty;

            StringBuilder stringBuilder = new StringBuilder();
            foreach (var parameter in parameters)
            {
                stringBuilder.Append(parameter.ToString() + ", ");
            }
            return stringBuilder.ToString();
        }
#endif
    }
    public class SS2AssetRequest<TAsset> where TAsset : UObject
    {
        public TAsset Asset => _asset;
        private TAsset _asset;

        public IEnumerable<TAsset> Assets => _assets;
        private List<TAsset> _assets;

        public SS2Bundle TargetBundle => _targetBundle;
        private SS2Bundle _targetBundle;

        public NullableRef<string> AssetName => _assetName;
        private NullableRef<string> _assetName;

        private bool _singleAssetLoad = true;

        public bool IsComplete => !_internalCoroutine.MoveNext();
        private IEnumerator _internalCoroutine;
        private string _assetTypeName;

        public void StartLoad()
        {
            if (_singleAssetLoad)
            {
                _internalCoroutine = LoadSingleAsset();
            }
            else
            {
                _internalCoroutine = LoadMultipleAsset();
            }
        }

        private IEnumerator LoadSingleAsset()
        {
            AssetBundleRequest request = null;

            request = SS2Assets.GetAssetBundle(TargetBundle).LoadAssetAsync<TAsset>(AssetName); ;
            while (!request.isDone)
                yield return null;

            _asset = (TAsset)request.asset;

#if DEBUG
            //Asset found, dont try to find it.
            if (_asset)
                yield break;

            SS2Log.Warning($"The method \"{GetCallingMethod()}\" is calling a SS2AssetRequest.StartLoad() while the class has the values \"{_assetTypeName}\", \"{AssetName}\" and \"{TargetBundle}\", however, the asset could not be found.\n" +
    $"A complete search of all the bundles will be done and the correct bundle enum will be logged.");

            SS2Bundle foundInBundle = SS2Bundle.Invalid;
            foreach(SS2Bundle bundleEnum in Enum.GetValues(typeof(SS2Bundle)))
            {
                if (bundleEnum == SS2Bundle.All || bundleEnum == SS2Bundle.Invalid)
                    continue;

                request = SS2Assets.GetAssetBundle(bundleEnum).LoadAssetAsync<TAsset>(AssetName);
                while(!request.isDone)
                {
                    yield return null;
                }

                if(request.asset)
                {
                    _asset = (TAsset)request.asset;
                    foundInBundle = bundleEnum;
                    break;
                }
            }

            if(_asset)
            {
                SS2Log.Info($"Asset of type {_assetTypeName} and name {AssetName} was found inside bundle {foundInBundle}. It is recommended to load the asset directly.");
            }
            else
            {
                SS2Log.Fatal($"Could not find asset of type {_assetTypeName} and name {AssetName} In any of the bundles, exceptions may occur.");
            }
#endif
            yield break;
        }

        private IEnumerator LoadMultipleAsset()
        {
            _assets.Clear();

            AssetBundleRequest request = null;
            if (TargetBundle == SS2Bundle.All)
            {
                foreach (SS2Bundle enumVal in Enum.GetValues(typeof(SS2Bundle)))
                {
                    if (enumVal == SS2Bundle.All || enumVal == SS2Bundle.Invalid)
                        continue;

                    request = SS2Assets.GetAssetBundle(enumVal).LoadAllAssetsAsync<TAsset>();
                    while (!request.isDone)
                        yield return null;

                    _assets.AddRange(request.allAssets.OfType<TAsset>());
                }

#if DEBUG
                if (_assets.Count == 0)
                {
                    SS2Log.Warning($"Could not find any asset of type {_assetTypeName} in any of the bundles");
                }
#endif
                yield break;
            }

            request = SS2Assets.GetAssetBundle(TargetBundle).LoadAllAssetsAsync<TAsset>();
            while (!request.isDone) yield return null;

            _assets.AddRange(request.allAssets.OfType<TAsset>());

#if DEBUG
            if (_assets.Count == 0)
            {
                SS2Log.Warning($"Could not find any asset of type {_assetTypeName} inside the bundle {TargetBundle}");
            }
#endif

            yield break;
        }

#if DEBUG
        private static string GetCallingMethod()
        {
            var stackTrace = new StackTrace();

            for (int stackFrameIndex = 0; stackFrameIndex < stackTrace.FrameCount; stackFrameIndex++)
            {
                var frame = stackTrace.GetFrame(stackFrameIndex);
                var method = frame.GetMethod();
                if (method == null)
                    continue;

                var declaringType = method.DeclaringType;
                if (declaringType.IsGenericType && declaringType.DeclaringType == typeof(SS2Assets))
                    continue;

                if (declaringType == typeof(SS2Assets))
                    continue;

                var fileName = frame.GetFileName();
                var fileLineNumber = frame.GetFileLineNumber();
                var fileColumnNumber = frame.GetFileColumnNumber();

                return $"{declaringType.FullName}.{method.Name}({GetMethodParams(method)}) (fileName={fileName}, Location=L{fileLineNumber} C{fileColumnNumber})";
            }
            return "[COULD NOT GET CALLING METHOD]";
        }

        private static string GetMethodParams(MethodBase methodBase)
        {
            var parameters = methodBase.GetParameters();
            if (parameters.Length == 0)
                return string.Empty;

            StringBuilder stringBuilder = new StringBuilder();
            foreach (var parameter in parameters)
            {
                stringBuilder.Append(parameter.ToString() + ", ");
            }
            return stringBuilder.ToString();
        }
#endif

        internal SS2AssetRequest(string name, SS2Bundle bundle)
        {
            _singleAssetLoad = true;
            _assetName = name;
            _assetTypeName = typeof(TAsset).Name;
            _targetBundle = bundle;
        }

        internal SS2AssetRequest(SS2Bundle bundle)
        {
            _singleAssetLoad = false;
            _assetName = new NullableRef<string>();
            _assets = new List<TAsset>();
            _assetTypeName = typeof(TAsset).Name;
            _targetBundle = bundle;
        }
    }
}