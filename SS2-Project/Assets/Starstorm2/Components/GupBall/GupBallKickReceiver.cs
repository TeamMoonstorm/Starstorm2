using UnityEngine;
using RoR2;

namespace SS2.Components
{
    /// <summary>
    /// Receives incoming damage on a GupBall body and converts it into a constant-velocity kick
    /// in the direction away from the attacker. Damage is always rejected so the ball never dies.
    /// 
    /// Implements IOnIncomingDamageServerReceiver (not IOnTakeDamageServerReceiver) because:
    /// - It fires before damage is applied (HealthComponent.cs:1318)
    /// - We can set damageInfo.rejected = true to prevent all damage
    /// - godMode would block ALL hooks, so we use rejection instead
    /// 
    /// Networking: This hook runs server-only inside HealthComponent.TakeDamage [Server].
    /// The ball body is server-authoritative (no player owner), so ApplyForceImpulse
    /// applies directly. CharacterNetworkTransform syncs position to clients.
    /// </summary>
    public class GupBallKickReceiver : MonoBehaviour, IOnIncomingDamageServerReceiver
    {
        [Header("Kick Settings")]
        public float kickSpeed = 30f;
        public float maxSpeed = 60f;
        [Tooltip("Multiplier for vertical kick component. Lower = flatter kicks.")]
        public float verticalKickMultiplier = 0.6f;
        [Tooltip("Upward bias added to every kick so ground-level hits arc slightly.")]
        public float upwardBias = 0.1f;

        private CharacterBody body;
        private CharacterMotor motor;

        private void Awake()
        {
            if (!TryGetComponent(out body))
            {
                Debug.LogError("GupBallKickReceiver: Missing CharacterBody on " + gameObject.name);
            }
            if (!TryGetComponent(out motor))
            {
                Debug.LogError("GupBallKickReceiver: Missing CharacterMotor on " + gameObject.name);
            }
        }

        public void OnIncomingDamageServer(DamageInfo damageInfo)
        {
            damageInfo.rejected = true;

            if (motor == null || body == null)
                return;

            if (damageInfo.attacker == null)
                return;

            CharacterBody attackerBody;
            if (!damageInfo.attacker.TryGetComponent(out attackerBody))
                return;

            Vector3 ballPosition = body.corePosition;
            Vector3 attackerPosition = attackerBody.corePosition;
            Vector3 kickDir = ballPosition - attackerPosition;

            // Epsilon check: attacker overlapping the ball
            if (kickDir.sqrMagnitude < 0.001f)
            {
                kickDir = Vector3.up;
            }
            else
            {
                kickDir.Normalize();
            }

            // Dampen vertical component
            kickDir.y *= verticalKickMultiplier;

            // Add upward bias so ground-level hits arc
            kickDir.y += upwardBias;

            kickDir.Normalize();

            Vector3 kickForce = kickDir * kickSpeed;

            PhysForceInfo forceInfo = PhysForceInfo.Create();
            forceInfo.force = kickForce;
            forceInfo.massIsOne = true;
            forceInfo.ignoreGroundStick = true;
            forceInfo.resetVelocity = true;
            motor.ApplyForceImpulse(in forceInfo);

            // Safety clamp
            if (motor.velocity.sqrMagnitude > maxSpeed * maxSpeed)
            {
                motor.velocity = motor.velocity.normalized * maxSpeed;
            }
        }
    }
}
