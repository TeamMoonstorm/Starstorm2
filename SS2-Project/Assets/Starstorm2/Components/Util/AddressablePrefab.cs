using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Moonstorm.Starstorm2.Components
{
    public class AddressablePrefab : MonoBehaviour
    {
        public string AssetPath;

        private GameObject instance;

        public bool _refreshInEditor;

        public GameObject GetInstance()
        {
            return instance;
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void OnDisable()
        {
            if (instance)
            {
                DestroyImmediate(instance);
            }
        }

        private void OnValidate()
        {
            if (_refreshInEditor)
                Refresh();
        }

        private void Refresh()
        {
            if (instance)
            {
                DestroyImmediate(instance);
            }

            instance = Instantiate(Addressables.LoadAssetAsync<GameObject>(AssetPath).WaitForCompletion(), gameObject.transform);
            instance.hideFlags = HideFlags.DontSaveInEditor;
        }
    }
}
