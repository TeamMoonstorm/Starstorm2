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

        // Range thresholds(meters) for weapon selection
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
        [Server]
        public void SetWeapon(WeaponType weapon)
        {
            OnWeaponChanged(weapon);
            _currentWeapon = weapon;
        }

        /// <summary>
        /// Sets ball form state. Server-only.
        /// </summary>
        [Server]
        public void SetBallForm(bool ballForm)
        {
            OnFormChanged(ballForm);
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
        [Server]
        public bool TryConsumeAmmo(WeaponType weapon)
        {
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
    }
}
