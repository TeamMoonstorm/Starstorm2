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

        // Dirty bit layout: 0=weapon, 1=ballForm, 2=shotgunAmmo, 3=rapidAmmo, 4=grenadeAmmo, 5=sniperAmmo
        private const uint DIRTY_WEAPON = 1U;
        private const uint DIRTY_BALLFORM = 2U;
        private const uint DIRTY_SHOTGUN_AMMO = 4U;
        private const uint DIRTY_RAPID_AMMO = 8U;
        private const uint DIRTY_GRENADE_AMMO = 16U;
        private const uint DIRTY_SNIPER_AMMO = 32U;

        // Range thresholds (meters) for weapon selection
        public static float shotgunMaxRange = 3f;
        public static float rapidLaserMaxRange = 6f;
        public static float grenadeLauncherMaxRange = 9f;

        // Ammo pool configuration
        public static int shotgunMaxAmmo = 10;
        public static int rapidLaserMaxAmmo = 50;
        public static int grenadeLauncherMaxAmmo = 10;
        public static int sniperLaserMaxAmmo = 10;

        // Regen rates (ammo per second, only while in ball form)
        public static float shotgunRegenRate = 1f;
        public static float rapidLaserRegenRate = 5f;
        public static float grenadeLauncherRegenRate = 1f;
        public static float sniperLaserRegenRate = 1f;

        [SyncVar(hook = nameof(OnWeaponChanged))]
        private WeaponType _currentWeapon = WeaponType.RapidLaser;

        [SyncVar(hook = nameof(OnFormChanged))]
        private bool _isBallForm;

        [SyncVar]
        private int _shotgunAmmo;
        [SyncVar]
        private int _rapidLaserAmmo;
        [SyncVar]
        private int _grenadeLauncherAmmo;
        [SyncVar]
        private int _sniperLaserAmmo;

        // Sub-frame regen accumulators (server-only, not synced)
        private float shotgunRegenAccumulator;
        private float rapidLaserRegenAccumulator;
        private float grenadeLauncherRegenAccumulator;
        private float sniperLaserRegenAccumulator;

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

            // Initialize ammo to max
            _shotgunAmmo = shotgunMaxAmmo;
            _rapidLaserAmmo = rapidLaserMaxAmmo;
            _grenadeLauncherAmmo = grenadeLauncherMaxAmmo;
            _sniperLaserAmmo = sniperLaserMaxAmmo;
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
        /// Sets the current weapon. Server-only.
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
        /// Sets ball form state. Server-only.
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

        #region Ammo System

        /// <summary>
        /// Returns the max ammo for a given weapon type.
        /// </summary>
        public static int GetMaxAmmo(WeaponType weapon)
        {
            switch (weapon)
            {
                case WeaponType.Shotgun: return shotgunMaxAmmo;
                case WeaponType.RapidLaser: return rapidLaserMaxAmmo;
                case WeaponType.GrenadeLauncher: return grenadeLauncherMaxAmmo;
                case WeaponType.SniperLaser: return sniperLaserMaxAmmo;
                default: return 0;
            }
        }

        /// <summary>
        /// Returns the current ammo for a given weapon type. Client-readable.
        /// </summary>
        public int GetAmmo(WeaponType weapon)
        {
            switch (weapon)
            {
                case WeaponType.Shotgun: return _shotgunAmmo;
                case WeaponType.RapidLaser: return _rapidLaserAmmo;
                case WeaponType.GrenadeLauncher: return _grenadeLauncherAmmo;
                case WeaponType.SniperLaser: return _sniperLaserAmmo;
                default: return 0;
            }
        }

        /// <summary>
        /// Returns true if the given weapon has at least 1 ammo. Client-readable.
        /// </summary>
        public bool HasAmmo(WeaponType weapon)
        {
            return GetAmmo(weapon) > 0;
        }

        /// <summary>
        /// Attempts to consume 1 ammo from the given weapon's pool. Server-only.
        /// Returns true if ammo was consumed, false if the pool was empty.
        /// </summary>
        public bool TryConsumeAmmo(WeaponType weapon)
        {
            if (!NetworkServer.active)
            {
                Debug.LogError("NemToolbotController.TryConsumeAmmo called on client.");
                return false;
            }

            switch (weapon)
            {
                case WeaponType.Shotgun:
                    if (_shotgunAmmo <= 0) return false;
                    _shotgunAmmo--;
                    return true;
                case WeaponType.RapidLaser:
                    if (_rapidLaserAmmo <= 0) return false;
                    _rapidLaserAmmo--;
                    return true;
                case WeaponType.GrenadeLauncher:
                    if (_grenadeLauncherAmmo <= 0) return false;
                    _grenadeLauncherAmmo--;
                    return true;
                case WeaponType.SniperLaser:
                    if (_sniperLaserAmmo <= 0) return false;
                    _sniperLaserAmmo--;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Regenerates ammo for all weapons. Called by BallMainState each tick.
        /// Server-only. Uses float accumulators for sub-frame precision.
        /// </summary>
        public void RegenAllAmmo(float deltaTime)
        {
            if (!NetworkServer.active)
                return;

            RegenWeaponAmmo(ref _shotgunAmmo, shotgunMaxAmmo, shotgunRegenRate, ref shotgunRegenAccumulator, deltaTime);
            RegenWeaponAmmo(ref _rapidLaserAmmo, rapidLaserMaxAmmo, rapidLaserRegenRate, ref rapidLaserRegenAccumulator, deltaTime);
            RegenWeaponAmmo(ref _grenadeLauncherAmmo, grenadeLauncherMaxAmmo, grenadeLauncherRegenRate, ref grenadeLauncherRegenAccumulator, deltaTime);
            RegenWeaponAmmo(ref _sniperLaserAmmo, sniperLaserMaxAmmo, sniperLaserRegenRate, ref sniperLaserRegenAccumulator, deltaTime);
        }

        private static void RegenWeaponAmmo(ref int currentAmmo, int maxAmmo, float regenRate, ref float accumulator, float deltaTime)
        {
            if (currentAmmo >= maxAmmo)
            {
                accumulator = 0f;
                return;
            }

            accumulator += regenRate * deltaTime;
            if (accumulator >= 1f)
            {
                int toAdd = Mathf.FloorToInt(accumulator);
                accumulator -= toAdd;
                currentAmmo = Mathf.Min(currentAmmo + toAdd, maxAmmo);
            }
        }

        /// <summary>
        /// Resets all regen accumulators. Call when exiting ball form.
        /// </summary>
        public void ResetRegenAccumulators()
        {
            shotgunRegenAccumulator = 0f;
            rapidLaserRegenAccumulator = 0f;
            grenadeLauncherRegenAccumulator = 0f;
            sniperLaserRegenAccumulator = 0f;
        }

        #endregion

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
                writer.WritePackedUInt32((uint)_shotgunAmmo);
                writer.WritePackedUInt32((uint)_rapidLaserAmmo);
                writer.WritePackedUInt32((uint)_grenadeLauncherAmmo);
                writer.WritePackedUInt32((uint)_sniperLaserAmmo);
                return true;
            }
            bool written = false;
            if ((syncVarDirtyBits & DIRTY_WEAPON) != 0U)
            {
                if (!written)
                {
                    writer.WritePackedUInt32(syncVarDirtyBits);
                    written = true;
                }
                writer.Write((byte)_currentWeapon);
            }
            if ((syncVarDirtyBits & DIRTY_BALLFORM) != 0U)
            {
                if (!written)
                {
                    writer.WritePackedUInt32(syncVarDirtyBits);
                    written = true;
                }
                writer.Write(_isBallForm);
            }
            if ((syncVarDirtyBits & DIRTY_SHOTGUN_AMMO) != 0U)
            {
                if (!written)
                {
                    writer.WritePackedUInt32(syncVarDirtyBits);
                    written = true;
                }
                writer.WritePackedUInt32((uint)_shotgunAmmo);
            }
            if ((syncVarDirtyBits & DIRTY_RAPID_AMMO) != 0U)
            {
                if (!written)
                {
                    writer.WritePackedUInt32(syncVarDirtyBits);
                    written = true;
                }
                writer.WritePackedUInt32((uint)_rapidLaserAmmo);
            }
            if ((syncVarDirtyBits & DIRTY_GRENADE_AMMO) != 0U)
            {
                if (!written)
                {
                    writer.WritePackedUInt32(syncVarDirtyBits);
                    written = true;
                }
                writer.WritePackedUInt32((uint)_grenadeLauncherAmmo);
            }
            if ((syncVarDirtyBits & DIRTY_SNIPER_AMMO) != 0U)
            {
                if (!written)
                {
                    writer.WritePackedUInt32(syncVarDirtyBits);
                    written = true;
                }
                writer.WritePackedUInt32((uint)_sniperLaserAmmo);
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
                _shotgunAmmo = (int)reader.ReadPackedUInt32();
                _rapidLaserAmmo = (int)reader.ReadPackedUInt32();
                _grenadeLauncherAmmo = (int)reader.ReadPackedUInt32();
                _sniperLaserAmmo = (int)reader.ReadPackedUInt32();
                return;
            }
            int bits = (int)reader.ReadPackedUInt32();
            if ((bits & (int)DIRTY_WEAPON) != 0)
            {
                OnWeaponChanged((WeaponType)reader.ReadByte());
            }
            if ((bits & (int)DIRTY_BALLFORM) != 0)
            {
                OnFormChanged(reader.ReadBoolean());
            }
            if ((bits & (int)DIRTY_SHOTGUN_AMMO) != 0)
            {
                _shotgunAmmo = (int)reader.ReadPackedUInt32();
            }
            if ((bits & (int)DIRTY_RAPID_AMMO) != 0)
            {
                _rapidLaserAmmo = (int)reader.ReadPackedUInt32();
            }
            if ((bits & (int)DIRTY_GRENADE_AMMO) != 0)
            {
                _grenadeLauncherAmmo = (int)reader.ReadPackedUInt32();
            }
            if ((bits & (int)DIRTY_SNIPER_AMMO) != 0)
            {
                _sniperLaserAmmo = (int)reader.ReadPackedUInt32();
            }
        }
    }
}
