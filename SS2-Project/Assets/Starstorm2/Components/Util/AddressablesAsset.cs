using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Moonstorm.Starstorm2.Components
{
    [ExecuteAlways]
    public class AddressablesAsset : MonoBehaviour
    {
        public string Key;
        private Object _asset;

        [SerializeField]
        private Component _targetComponent;
        [SerializeField]
        private string _targetMemberInfoName;

        private void Awake()
        {
            Refresh();
        }

        public void Refresh()
        {
            _asset = Addressables.LoadAssetAsync<Object>(Key).WaitForCompletion();
            if (!_asset)
            {
                Debug.LogError($"AddressablesAsset failed loading {Key}");
                return;
            }

            if (!_targetComponent)
            {
                Debug.LogError($"AddressablesAsset failed _targetComponent");
                return;
            }

            if (string.IsNullOrWhiteSpace(_targetMemberInfoName))
            {
                Debug.LogError($"AddressablesAsset failed IsNullOrWhiteSpace _targetMemberInfoName");
                return;
            }

            var targetMemberInfo = _targetComponent.
                GetType().
                GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).
                Where(m =>
                    m.GetType().Name == "MonoProperty" ||
                    m.GetType().Name == "MonoField" ||
                    m.GetType().Name == "FieldInfo" ||
                    m.GetType().Name == "PropertyInfo").
                FirstOrDefault(m => $"({m.DeclaringType.Name}) {m.Name}" == _targetMemberInfoName);

            if (targetMemberInfo == null)
            {
                Debug.LogError($"AddressablesAsset failed finding targetMemberInfo based on name {_targetMemberInfoName}. Target Component {_targetComponent}");
                return;
            }

            var targetMemberInfoType = targetMemberInfo.GetType();
            if (targetMemberInfo is FieldInfo fieldInfo)
            {
                fieldInfo.SetValue(_targetComponent, _asset);
            }
            else if (targetMemberInfo is PropertyInfo propertyInfo)
            {
                propertyInfo.SetValue(_targetComponent, _asset);
            }

            Debug.Log($"Success setting {_targetComponent}::{_targetMemberInfoName}");
        }
    }
}