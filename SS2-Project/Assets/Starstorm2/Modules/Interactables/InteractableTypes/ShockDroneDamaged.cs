using EntityStates;
using Moonstorm.Starstorm2.Components;
using R2API;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Interactables
{
    [DisabledContent]
    public sealed class ShockDroneDamaged : InteractableBase
    {
        public override GameObject Interactable { get; } = SS2Assets.LoadAsset<GameObject>("ShockDroneBroken", SS2Bundle.Indev);

        public override MSInteractableDirectorCard InteractableDirectorCard { get; } = SS2Assets.LoadAsset<MSInteractableDirectorCard>("msidcShockDrone", SS2Bundle.Indev);
    }
}
