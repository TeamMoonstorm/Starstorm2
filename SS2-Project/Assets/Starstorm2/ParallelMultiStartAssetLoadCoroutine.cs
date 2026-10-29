using MSU;
using SS2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS2
{
    [Obsolete("ParallelMultiStartLoadCoroutine is obsolete. Utilize \"ParallelCoroutine\" and add instances of SS2AssetRequest to it instead", error: true)]
    public class ParallelMultiStartAssetLoadCoroutine
    {
        private ParallelCoroutine _internalCoroutineHelper = ParallelCoroutine.RequestFromPool();
        private Dictionary<int, UnityEngine.Object> _assetsLoaded = new Dictionary<int, UnityEngine.Object>();

        public void Start() { }

        public bool IsDone => _internalCoroutineHelper.isDone;

        public void AddAssetToLoad<T>(string assetName, SS2Bundle bundle) where T : UnityEngine.Object
        {
            _internalCoroutineHelper.Add(LoadFunc<T>(assetName, bundle));
        }

        public T GetLoadedAsset<T>(string assetName) where T : UnityEngine.Object
        {
            int hash = CalculateHash(typeof(T), assetName);
            if (_assetsLoaded.ContainsKey(hash))
            {
                return (T)_assetsLoaded[hash];
            }
            return null;
        }

        private IEnumerator LoadFunc<T>(string assetName, SS2Bundle bundle) where T : UnityEngine.Object
        {
            using var request = SS2Assets.LoadAssetAsync<T>(assetName, bundle);
            while (!request.IsComplete)
                yield return null;

            _assetsLoaded.Add(CalculateHash(typeof(T), assetName), request.Asset);
        }

        private int CalculateHash(Type type, string assetName) => (type.GetHashCode() / 2) + (assetName.GetHashCode() / 2);
    }
}
