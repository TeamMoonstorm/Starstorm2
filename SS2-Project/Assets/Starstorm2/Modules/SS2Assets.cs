using Moonstorm.Loaders;
using Moonstorm.Starstorm2.PostProcess;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Path = System.IO.Path;

namespace Moonstorm.Starstorm2
{
    public class SS2Assets : AssetsLoader<SS2Assets>
    {
        public override AssetBundle MainAssetBundle => bundle;

        public string AssemblyDir => Path.GetDirectoryName(Starstorm.pluginInfo.Location);

        private static AssetBundle bundle;

        private const string assetBundleFolderName = "assetbundles";
        private const string mainAssetBundleName = "ss2assets";


        internal void Init()
        {
            var bundlePaths = GetAssetBundlePaths();
            bundle = AssetBundle.LoadFromFile(bundlePaths);
        }

        internal void SwapMaterialShaders()
        {
            SwapShadersFromMaterials(MainAssetBundle.LoadAllAssets<Material>().Where(mat => mat.shader.name.StartsWith("Stubbed")));
        }

        private string GetAssetBundlePaths()
        {
            return Path.Combine(AssemblyDir, assetBundleFolderName, mainAssetBundleName);
            /*return Directory.GetFiles(Path.Combine(AssemblyDir, assetBundleFolderName))
               .Where(filePath => !filePath.EndsWith(".manifest"))
               .OrderByDescending(path => Path.GetFileName(path).Equals(mainAssetBundleName))
               .ToArray();*/
        }

        //Not the most pleasant workaround but that's what we get
        //private static PostProcessProfile[] ppProfiles;
        private void LoadPostProcessing()
        {
            var ppProfiles = bundle.LoadAllAssets<PostProcessProfile>();
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