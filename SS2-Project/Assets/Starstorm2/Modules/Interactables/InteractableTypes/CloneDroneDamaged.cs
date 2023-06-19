using EntityStates;
using Moonstorm.Starstorm2.Components;
using R2API;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Interactables
{
    //[DisabledContent]
    public sealed class CloneDroneDamaged : InteractableBase
    {
        public override GameObject Interactable { get; } = SS2Assets.LoadAsset<GameObject>("CloneDroneBroken", SS2Bundle.Indev);
        //private static GameObject interactable;

        public override MSInteractableDirectorCard InteractableDirectorCard { get; } = SS2Assets.LoadAsset<MSInteractableDirectorCard>("msidcCloneDrone", SS2Bundle.Indev);

        public override void Initialize()
        {
            base.Initialize();
            //interactable = InteractableDirectorCard.prefab;
            //Debug.Log("MSIDC PREFAB : " + InteractableDirectorCard.prefab.name);
            //Debug.Log("CLONE DRONE INTERACTABLE : " + Interactable.name);
        }
    }
}
