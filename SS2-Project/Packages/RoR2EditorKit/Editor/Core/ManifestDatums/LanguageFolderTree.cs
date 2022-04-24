using System;
using ThunderKit.Core.Manifests;
using UnityEngine;

namespace RoR2EditorKit.Core.ManifestDatums
{
    [Serializable]
    public struct LanguageFolder
    {
        public string languageName;
        public TextAsset[] languageFiles;
    }

    public class LanguageFolderTree : ManifestDatum
    {
        public string rootFolderName;
        public LanguageFolder[] languageFolders;
    }
}
