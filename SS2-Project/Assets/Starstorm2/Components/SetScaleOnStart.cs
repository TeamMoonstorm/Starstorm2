using UnityEngine;
namespace SS2 { 
    public class SetScaleOnStart : MonoBehaviour 
    { 
        public Vector3 scale; 
        private void Start() => base.transform.localScale = scale; 
    } 
}