using MSU;
using R2API.ScriptableObjects;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using HG;
using System;
using System.Linq;
using System.Text;
using UObject = UnityEngine.Object;
namespace SS2
{
    public class ExtendedAssetCollection : AssetCollection
    {
        public void OnValidate()
        {
            // make new entries empty instead of duplicates, so they dont get nuked by the duplicate remover
            for(int i = this.assets.Length - 1; i >= 1; i--)
            {
                if (this.assets[i] == this.assets[i - 1]) this.assets[i] = null;
                else break;
            }

            //ensure field values are contained in AssetCollection.assets
            foreach (FieldInfo f in this.GetType().GetFields())
            {
                object val = f.GetValue(this);
                if (val == null) continue;
                if (!(val is UObject) || val is MonoScript) continue;
                UObject obj = (UObject)val; // "specified cast is not valid" yes it is dumbfuck
                if (!obj) continue;
                bool contains = false;
                for (int i = 0; i < assets.Length; i++)
                {
                    if (assets[i] == obj)
                    {
                        contains = true;
                        break;
                    }
                }
                if (!contains)
                {
                    ArrayUtils.ArrayInsert<UObject>(ref this.assets, 0, obj);
                }
            }

            //remove duplicate entries
            //Distinct() removes duplicate null entries(not wanted)
            List<UObject> objects = new List<UObject>();
            int duplicates = 0;
            foreach (UObject obj in this.assets)
            {
                bool contains = false;
                foreach (UObject obj2 in objects)
                {
                    if (obj == obj2 && obj)
                    {
                        duplicates++;
                        contains = true;
                        break;
                    }
                }

                if (!contains) objects.Add(obj);
            }
            this.assets = objects.ToArray();
        }
    }
}
