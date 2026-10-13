using System.Collections.Generic;
using UnityEngine;

namespace Starstorm2.Components
{
    // this handles pickups that get cloned by dupe drone to make sure they dont dupe the same thing twice .,.,
    // canbecloned is called on clonedronesearch while looking for new pickups and oncloned is called after duping the pickup in clonedronecook ,.,. 
    public class CloneDroneClonedPickups : MonoBehaviour 
    {
        private List<GameObject> clonedPickups = new();
        
        public bool CanBeCloned(GameObject pickupObject) => !clonedPickups.Contains(pickupObject);
        
        public void OnCloned(GameObject pickupObject)
        {
            clonedPickups.Add(pickupObject);
        }
    }
}