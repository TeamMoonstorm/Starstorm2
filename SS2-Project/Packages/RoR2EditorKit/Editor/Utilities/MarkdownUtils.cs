using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.Networking;

namespace RoR2EditorKit.Utilities
{
    public static class MarkdownUtils
    {
        public static string GenerateAssetLink(UnityEngine.Object obj) => $"[{obj.name}](assetlink://{UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(obj))})";

        public static string GenerateAssetLink(string name, string path) => $"[{name}](assetlink://{path})";
    }
}
