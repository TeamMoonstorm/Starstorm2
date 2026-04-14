using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    public class NemToolbotController : NetworkBehaviour
    {
        public enum WeaponType : byte
        {
            Shotgun = 0,
            RapidLaser = 1,
            GrenadeLauncher = 2,
            SniperLaser = 3
        }

        // Range thresholds (meters) for weapon selection
        public static float shotgunMaxRange = 3f;
        public static float rapidLaserMaxRange = 6f;
        public static float grenadeLauncherMaxRange = 9f;

        [SyncVar(hook = nameof(OnWeaponChanged))]
        private WeaponType _currentWeapon = WeaponType.RapidLaser;

        [SyncVar(hook = nameof(OnFormChanged))]
        private bool _isBallForm;

        private CharacterBody characterBody;
        private CharacterMotor characterMotor;

        public WeaponType currentWeapon => _currentWeapon;
        public bool isBallForm => _isBallForm;

        private void Awake()
        {
            if (!TryGetComponent(out characterBody))
            {
                Debug.LogError("NemToolbotController: Failed to get CharacterBody on " + gameObject.name);
            }
            if (!TryGetComponent(out characterMotor))
            {
                Debug.LogError("NemToolbotController: Failed to get CharacterMotor on " + gameObject.name);
            }
        }

        /// <summary>
        /// Maps a raycast distance to the appropriate weapon type.
        /// </summary>
        public static WeaponType GetWeaponFromRange(float distance)
        {
            if (distance < shotgunMaxRange)
                return WeaponType.Shotgun;
            if (distance < rapidLaserMaxRange)
                return WeaponType.RapidLaser;
            if (distance < grenadeLauncherMaxRange)
                return WeaponType.GrenadeLauncher;
            return WeaponType.SniperLaser;
        }

        /// <summary>
        /// Sets the current weapon. Authority-only.
        /// </summary>
        public void SetWeapon(WeaponType weapon)
        {
            if (!NetworkServer.active)
            {
                Debug.LogError("NemToolbotController.SetWeapon called on client.");
                return;
            }
            _currentWeapon = weapon;
        }

        /// <summary>
        /// Sets ball form state. Authority-only.
        /// </summary>
        public void SetBallForm(bool ballForm)
        {
            if (!NetworkServer.active)
            {
                Debug.LogError("NemToolbotController.SetBallForm called on client.");
                return;
            }
            _isBallForm = ballForm;
        }

        /// <summary>
        /// Returns a damage multiplier based on current velocity relative to base move speed.
        /// Used by ball form attacks to scale damage with momentum.
        /// </summary>
        public float GetDamageMultiplierFromSpeed()
        {
            if (characterBody == null || characterMotor == null)
                return 1f;

            float currentSpeed = characterMotor.velocity.magnitude;
            float baseSpeed = characterBody.baseMoveSpeed;
            if (baseSpeed <= 0f)
                return 1f;

            return Mathf.Max(1f, currentSpeed / baseSpeed);
        }

        private void OnWeaponChanged(WeaponType newWeapon)
        {
            _currentWeapon = newWeapon;
        }

        private void OnFormChanged(bool newBallForm)
        {
            _isBallForm = newBallForm;
        }

        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            if (forceAll)
            {
                writer.Write((byte)_currentWeapon);
                writer.Write(_isBallForm);
                return true;
            }
            bool written = false;
            if ((syncVarDirtyBits & 1U) != 0U)
            {
                if (!written)
                {
                    writer.WritePackedUInt32(syncVarDirtyBits);
                    written = true;
                }
                writer.Write((byte)_currentWeapon);
            }
            if ((syncVarDirtyBits & 2U) != 0U)
            {
                if (!written)
                {
                    writer.WritePackedUInt32(syncVarDirtyBits);
                    written = true;
                }
                writer.Write(_isBallForm);
            }
            if (!written)
            {
                writer.WritePackedUInt32(syncVarDirtyBits);
            }
            return written;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                _currentWeapon = (WeaponType)reader.ReadByte();
                _isBallForm = reader.ReadBoolean();
                return;
            }
            int bits = (int)reader.ReadPackedUInt32();
            if ((bits & 1) != 0)
            {
                OnWeaponChanged((WeaponType)reader.ReadByte());
            }
            if ((bits & 2) != 0)
            {
                OnFormChanged(reader.ReadBoolean());
            }
        }
    }
}
