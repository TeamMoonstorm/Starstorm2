using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThunderKit.Core;
using ThunderKit.Core.Config;
using ThunderKit.Integrations.Thunderstore;
using System;
using RoR2.Editor;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using System.Linq;

namespace SS2.Editor.Importers
{
    public sealed class AssetReimporterUtility
    {
        Type[] _types;
        EditorCoroutine _editorCoroutine;

        public bool IsDone()
        {
            return _editorCoroutine == null;
        }

        private IEnumerator ReimportAssetsCoroutine(int batchSize)
        {
            try
            {
                AssetDatabase.StartAssetEditing();

                //Find the guids of all the prefabs
                string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Starstorm2/AssetStorm" });
                GameObject[] prefabs = new GameObject[guids.Length];

                //Load the prefabs, yield return null every x game objects loaded
                int batchTracker = 0;
                for(int i = 0; i < guids.Length; i++)
                {
                    prefabs[i] = AssetDatabaseUtil.LoadAssetFromGUID<GameObject>(guids[i]);

                    batchTracker++;
                    if(batchTracker > batchSize)
                    {
                        batchTracker = 0;
                        yield return null;
                    }
                }

                //See if prefab has any of the components
                for(int i = 0; i < prefabs.Length; i++)
                {
                    batchTracker++;

                    GameObject p = prefabs[i];
                    Type[] componentTypes = p.GetComponents<MonoBehaviour>()
                        .Where(c => c != null)
                        .Select(c => c.GetType())
                        .ToArray();

                    foreach(var type in _types)
                    {
                        //Reimport prefab if it has one of our components
                        if(componentTypes.Contains(type))
                        {
                            Debug.Log($"Importing {p}");
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(p));
                        }
                    }

                    if (batchTracker > batchSize)
                    {
                        batchTracker = 0;
                        yield return null;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorCoroutineUtility.StopCoroutine(_editorCoroutine);
                _editorCoroutine = null;
            }
        }

        public AssetReimporterUtility(params string[] typeAssemblyQualifieds)
        {
            List<Type> types = new List<Type>();
            foreach(var typeName in typeAssemblyQualifieds)
            {
                Type t = Type.GetType(typeName);
                if (t != null)
                {
                    types.Add(t);
                }
            }


            _types = types.ToArray();
            _editorCoroutine = EditorCoroutineUtility.StartCoroutine(ReimportAssetsCoroutine(1024), this);
        }
    }

    internal class AssetReimporter : OptionalExecutor
    {
        public static readonly string[] assemblyQualifiedTypeNames = new string[] {
"ChildLocator, RoR2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
"RoR2.ItemDisplay, RoR2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
"RoR2.FlickerLight, RoR2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
"RoR2.BuffWard, RoR2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
};
        public override int Priority => int.MinValue + 1;

        private AssetReimporterUtility _reimporterUtility;

        public override bool Execute()
        {
            _reimporterUtility ??= new AssetReimporterUtility(assemblyQualifiedTypeNames);

            return _reimporterUtility.IsDone();
        }

        private void OnValidate()
        {
            if(!enabled)
            {
                Debug.Log("Nuh-uh");
                //enabled = true;
            }
        }

        public override void Cleanup()
        {
            _reimporterUtility = null;    
        }
    }
}