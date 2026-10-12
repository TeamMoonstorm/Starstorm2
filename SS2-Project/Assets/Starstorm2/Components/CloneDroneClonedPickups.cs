using System.Collections.Generic;
using UnityEngine;

namespace Starstorm2.Components
{
    public class CloneDroneClonedPickups : MonoBehaviour 
    {
        private List<GameObject> clonedPickups = new();
        
        public bool CanBeCloned(GameObject cloner) => !clonedPickups.Contains(cloner);
        
        public void OnCloned(GameObject cloner)
        {
            clonedPickups.Add(cloner);
        }
    }
}