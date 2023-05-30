using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    public class ComponentLogger : MonoBehaviour
    {
        private void Awake() { Debug.Log($"{name}.Awake()"); }
        private void OnDestroy() { Debug.Log($"{name}.OnDestroy()"); }
        private void OnEnable() { Debug.Log($"{name}.OnEnable()"); }
        private void OnDisable() { Debug.Log($"{name}.OnDisable()"); }
    }
}
