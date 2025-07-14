using UnityEngine;
using RoR2;

/// <summary>
/// Exists to mark the Mimic as a special entity so that player Pings work correctly relative to the state the enemy currently is in - Ex it pings like an interactable, when it is one.
/// </summary>
namespace SS2.Components
{
    [RequireComponent(typeof(CharacterBody))]
    [RequireComponent(typeof(PurchaseInteraction))]
    public class MimicPingCorrecter : MonoBehaviour { }
}
